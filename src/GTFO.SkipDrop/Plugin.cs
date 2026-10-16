using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace GTFO.SkipDrop;

// Based on TheArchive's "Skip Elevator Animation" feature by AuriRex (MIT):
// https://github.com/AuriRex/GTFO_TheArchive/blob/main/TheArchive.Essentials/Features/QoL/SkipElevatorAnimation.cs
[BepInPlugin(GUID, NAME, VERSION)]
[BepInProcess("GTFO.exe")]
public class Plugin : BasePlugin
{
    public const string GUID = "MindAttic.GTFO.SkipDrop";
    public const string NAME = "GTFO.SkipDrop";
    public const string VERSION = "1.0.0";

    public override void Load()
    {
        // Resolved by name so the build only needs BepInEx, not the game's generated interop assemblies.
        var elevatorRide = AccessTools.TypeByName("ElevatorRide");
        var start = AccessTools.Method(elevatorRide, "StartPreReleaseSequence");
        var skip = AccessTools.Method(elevatorRide, "SkipPreReleaseSequence");
        if (start == null || skip == null)
        {
            Log.LogError("ElevatorRide.StartPreReleaseSequence/SkipPreReleaseSequence not found - game update? Drop will not be skipped.");
            return;
        }

        _skip = skip;
        new Harmony(GUID).Patch(start, postfix: new HarmonyMethod(typeof(Plugin), nameof(Postfix)));
        Log.LogInfo($"{NAME} v{VERSION} loaded.");
    }

    private static System.Reflection.MethodInfo _skip = null!;

    private static void Postfix() => _skip.Invoke(null, null);
}
