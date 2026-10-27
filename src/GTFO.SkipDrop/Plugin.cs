using System.Diagnostics;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace GTFO.SkipDrop;

// The pre-release skip is based on TheArchive's "Skip Elevator Animation" feature by AuriRex (MIT):
// https://github.com/AuriRex/GTFO_TheArchive/blob/main/TheArchive.Essentials/Features/QoL/SkipElevatorAnimation.cs
[BepInPlugin(GUID, NAME, VERSION)]
[BepInProcess("GTFO.exe")]
public class Plugin : BasePlugin
{
    public const string GUID = "MindAttic.GTFO.SkipDrop";
    public const string NAME = "GTFO.SkipDrop";
    public const string VERSION = "18.0.0";

    // m_endDur is a duration the landing lerps over, so it stays just above 0 to avoid a divide-by-zero.
    private static readonly (string Field, float Value)[] RideTimings =
    {
        ("m_startDelay", 0f),
        ("m_engineStartDelay", 0f),
        ("m_endDur", 0.1f),
    };

    private static ManualLogSource _log = null!;
    private static MethodInfo _skipPreRelease = null!;
    private static PropertyInfo _fastElevator = null!;
    private static PropertyInfo _minTimeLooping = null!;
    private static PropertyInfo _current = null!;
    private static PropertyInfo _rideInProgress = null!;
    private static readonly List<PropertyInfo> _rideTimings = new();
    private static object? _savedFastElevator;
    private static readonly Stopwatch _dropTimer = new();
    private static double _phaseStart;
    private static string? _phase;

    public override void Load()
    {
        _log = Log;

        // The master switch is the cfg's first entry ("0. Mod" sorts ahead of every other section), so
        // the mod can be turned off from its .cfg where a mod manager profile doesn't list it.
        var enabled = Config.Bind("0. Mod", "Enabled", true,
            "Turn the whole mod on or off. When false, the mod loads nothing: no patches, and the drop " +
            "plays at the game's normal speed.");
        if (!enabled.Value)
        {
            Log.LogInfo($"{NAME} is switched off in its config ([0. Mod] Enabled) - not loading.");
            return;
        }

        // Resolved by name so the build only needs BepInEx, not the game's generated interop assemblies.
        var ride = AccessTools.TypeByName("ElevatorRide");
        var startPreRelease = AccessTools.Method(ride, "StartPreReleaseSequence");
        var startRide = AccessTools.Method(ride, "StartElevatorRide");
        var stopRide = AccessTools.Method(ride, "StopElevatorRide");
        var changeState = AccessTools.Method(ride, "ChangeState");
        var cleanup = AccessTools.Method(ride, "Cleanup");
        _rideInProgress = AccessTools.Property(ride, "ElevatorRideInProgress");
        _skipPreRelease = AccessTools.Method(ride, "SkipPreReleaseSequence");
        _minTimeLooping = AccessTools.Property(ride, "MinTimeLooping");
        _current = AccessTools.Property(ride, "Current");
        _fastElevator = AccessTools.Property(AccessTools.TypeByName("Globals.Global"), "FastElevator");

        foreach (var (field, _) in RideTimings) _rideTimings.Add(AccessTools.Property(ride, field));

        if (new object?[] { startPreRelease, startRide, stopRide, changeState, cleanup, _rideInProgress, _skipPreRelease, _minTimeLooping, _fastElevator, _current }
                .Concat(_rideTimings).Any(m => m == null))
        {
            Log.LogError("Elevator members not found - game update? Drop will not be skipped.");
            return;
        }

        var harmony = new Harmony(GUID);
        harmony.Patch(startPreRelease, postfix: new HarmonyMethod(typeof(Plugin), nameof(SkipPreRelease)));
        harmony.Patch(startRide, prefix: new HarmonyMethod(typeof(Plugin), nameof(SpeedUpDrop)));
        harmony.Patch(stopRide, postfix: new HarmonyMethod(typeof(Plugin), nameof(RestoreAfterDrop)));
        harmony.Patch(changeState, prefix: new HarmonyMethod(typeof(Plugin), nameof(LogPhase)));
        harmony.Patch(cleanup, postfix: new HarmonyMethod(typeof(Plugin), nameof(RestoreAfterAbortedDrop)));
        Log.LogInfo($"{NAME} v{VERSION} loaded.");
    }

    private static void SkipPreRelease()
    {
        try
        {
            _skipPreRelease.Invoke(null, null);
        }
        catch (Exception e)
        {
            _log.LogError($"Couldn't skip the opening animation: {e}");
        }
    }

    // The cage can't land before the level finishes generating, so this removes the forced minimum
    // fall time and turns on the game's own dev fast-elevator mode; level generation is the floor.
    // StartElevatorRide is static, so the ride instance comes from ElevatorRide.Current. Any failure
    // here is swallowed: an exception in this prefix would stop the game's own ride from starting.
    private static void SpeedUpDrop()
    {
        _dropTimer.Restart();
        _phaseStart = 0;
        _phase = null;
        try
        {
            // A drop aborted before StopElevatorRide leaves our own `true` in place; don't save that as the original.
            _savedFastElevator ??= _fastElevator.GetValue(null);
            _fastElevator.SetValue(null, true);
            _minTimeLooping.SetValue(null, Convert.ChangeType(0, _minTimeLooping.PropertyType));

            var ride = _current.GetValue(null);
            if (ride == null) { _log.LogWarning("ElevatorRide.Current is null - ride delays not cut."); return; }
            for (var i = 0; i < RideTimings.Length; i++)
                _rideTimings[i].SetValue(ride, Convert.ChangeType(RideTimings[i].Value, _rideTimings[i].PropertyType));
        }
        catch (Exception e)
        {
            _log.LogError($"Couldn't speed up the drop, leaving it vanilla: {e}");
        }
    }

    // Per-phase timings show what's left of the drop; "Middle" is the cage looping while the level generates.
    private static void LogPhase(object __0)
    {
        if (!_dropTimer.IsRunning) return;
        try
        {
            var now = _dropTimer.Elapsed.TotalSeconds;
            if (_phase != null) _log.LogInfo($"Drop phase {_phase}: {now - _phaseStart:0.0}s");
            _phase = __0?.ToString();
            _phaseStart = now;
        }
        catch (Exception e)
        {
            _log.LogError($"Couldn't log drop phase: {e}");
        }
    }

    // Global.FastElevator is a game-wide dev flag, so it's only held on for the drop itself.
    private static void RestoreAfterDrop()
    {
        try
        {
            if (_savedFastElevator != null) _fastElevator.SetValue(null, _savedFastElevator);
            _savedFastElevator = null;
        }
        catch (Exception e)
        {
            _log.LogError($"Couldn't restore FastElevator: {e}");
        }
        if (_dropTimer.IsRunning) _log.LogInfo($"Drop took {_dropTimer.Elapsed.TotalSeconds:0.0}s.");
        _dropTimer.Reset();
    }

    // A drop cut short (leaving the lobby or the expedition ending mid-drop) never reaches
    // StopElevatorRide, so the flag would stay on until the next drop. The elevator's own Cleanup
    // puts it back - but only once no ride is in progress, so a cleanup during a live drop leaves it.
    private static void RestoreAfterAbortedDrop()
    {
        try
        {
            if (_savedFastElevator == null || _rideInProgress.GetValue(null) is true) return;
            _fastElevator.SetValue(null, _savedFastElevator);
            _savedFastElevator = null;
            _dropTimer.Reset();
            _log.LogInfo("Drop was cut short - fast elevator flag restored.");
        }
        catch (Exception e)
        {
            _log.LogError($"Couldn't restore FastElevator after a cut-short drop: {e}");
        }
    }
}
