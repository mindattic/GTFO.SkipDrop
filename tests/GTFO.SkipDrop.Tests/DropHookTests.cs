using System.Collections;
using System.Reflection;
using BepInEx.Logging;
using NUnit.Framework;

namespace GTFO.SkipDrop.Tests;

// Stand-ins for the game's ElevatorRide and Globals.Global; Il2CppInterop exposes the game's fields as properties.
public class FakeRide
{
    public static FakeRide? Current { get; set; }
    public static float MinTimeLooping { get; set; }
    public float m_startDelay { get; set; }
    public float m_engineStartDelay { get; set; }
    public float m_endDur { get; set; }
}

public static class FakeGlobal
{
    public static bool ThrowOnSet { get; set; }
    private static bool _fastElevator;

    public static bool FastElevator
    {
        get => _fastElevator;
        set
        {
            if (ThrowOnSet) throw new InvalidOperationException("setter failed");
            _fastElevator = value;
        }
    }
}

public class ThrowingState
{
    public override string ToString() => throw new InvalidOperationException("ToString failed");
}

// The hooks share static state in Plugin, so these must not run in parallel.
[NonParallelizable]
public class DropHookTests
{
    private const BindingFlags Statics = BindingFlags.NonPublic | BindingFlags.Static;

    [SetUp]
    public void SetUp()
    {
        SetStatic("_log", new ManualLogSource("tests"));
        SetStatic("_fastElevator", typeof(FakeGlobal).GetProperty(nameof(FakeGlobal.FastElevator)));
        SetStatic("_minTimeLooping", typeof(FakeRide).GetProperty(nameof(FakeRide.MinTimeLooping)));
        SetStatic("_current", typeof(FakeRide).GetProperty(nameof(FakeRide.Current)));
        SetStatic("_savedFastElevator", null);
        SetStatic("_phase", null);

        var timings = (IList)GetStatic("_rideTimings")!;
        timings.Clear();
        timings.Add(typeof(FakeRide).GetProperty(nameof(FakeRide.m_startDelay)));
        timings.Add(typeof(FakeRide).GetProperty(nameof(FakeRide.m_engineStartDelay)));
        timings.Add(typeof(FakeRide).GetProperty(nameof(FakeRide.m_endDur)));

        ((System.Diagnostics.Stopwatch)GetStatic("_dropTimer")!).Reset();

        FakeGlobal.ThrowOnSet = false;
        FakeGlobal.FastElevator = false;
        FakeRide.MinTimeLooping = 30f;
        FakeRide.Current = new FakeRide { m_startDelay = 5f, m_engineStartDelay = 3f, m_endDur = 4f };
    }

    [Test]
    public void SpeedUpDrop_TurnsOnFastElevatorAndCutsTimings()
    {
        Call("SpeedUpDrop");

        Assert.That(FakeGlobal.FastElevator, Is.True);
        Assert.That(FakeRide.MinTimeLooping, Is.EqualTo(0f));
        Assert.That(FakeRide.Current!.m_startDelay, Is.EqualTo(0f));
        Assert.That(FakeRide.Current.m_engineStartDelay, Is.EqualTo(0f));
        Assert.That(FakeRide.Current.m_endDur, Is.EqualTo(0.1f));
    }

    [Test]
    public void SpeedUpDrop_KeepsLandingDurationAboveZero()
    {
        Call("SpeedUpDrop");

        Assert.That(FakeRide.Current!.m_endDur, Is.GreaterThan(0f));
    }

    [Test]
    public void SpeedUpDrop_WithNoCurrentRide_StillAppliesGlobalsWithoutThrowing()
    {
        FakeRide.Current = null;

        Assert.DoesNotThrow(() => Call("SpeedUpDrop"));
        Assert.That(FakeGlobal.FastElevator, Is.True);
        Assert.That(FakeRide.MinTimeLooping, Is.EqualTo(0f));
    }

    [Test]
    public void SpeedUpDrop_SwallowsFailuresSoTheRideStillStarts()
    {
        FakeGlobal.ThrowOnSet = true;

        Assert.DoesNotThrow(() => Call("SpeedUpDrop"));
    }

    [Test]
    public void RestoreAfterDrop_PutsFastElevatorBackToItsOriginalValue()
    {
        Call("SpeedUpDrop");
        Call("RestoreAfterDrop");

        Assert.That(FakeGlobal.FastElevator, Is.False);
    }

    [Test]
    public void RestoreAfterDrop_KeepsFastElevatorOnWhenItWasAlreadyOn()
    {
        FakeGlobal.FastElevator = true;

        Call("SpeedUpDrop");
        Call("RestoreAfterDrop");

        Assert.That(FakeGlobal.FastElevator, Is.True);
    }

    [Test]
    public void AbortedDrop_NextDropStillRestoresTheOriginalValue()
    {
        Call("SpeedUpDrop"); // aborted: StopElevatorRide never runs
        Call("SpeedUpDrop");
        Call("RestoreAfterDrop");

        Assert.That(FakeGlobal.FastElevator, Is.False);
    }

    [Test]
    public void RestoreAfterDrop_WithoutADrop_LeavesFastElevatorAlone()
    {
        FakeGlobal.FastElevator = true;

        Assert.DoesNotThrow(() => Call("RestoreAfterDrop"));
        Assert.That(FakeGlobal.FastElevator, Is.True);
    }

    [Test]
    public void RestoreAfterDrop_StopsTheDropTimer()
    {
        Call("SpeedUpDrop");
        Call("RestoreAfterDrop");

        Assert.That(((System.Diagnostics.Stopwatch)GetStatic("_dropTimer")!).IsRunning, Is.False);
    }

    [Test]
    public void LogPhase_TracksTheCurrentPhaseDuringADrop()
    {
        Call("SpeedUpDrop");
        Call("LogPhase", "Middle");

        Assert.That(GetStatic("_phase"), Is.EqualTo("Middle"));
    }

    [Test]
    public void LogPhase_OutsideADrop_DoesNothing()
    {
        Call("LogPhase", "Middle");

        Assert.That(GetStatic("_phase"), Is.Null);
    }

    [Test]
    public void LogPhase_NeverThrowsIntoTheGamesStateChange()
    {
        Call("SpeedUpDrop");

        Assert.DoesNotThrow(() => Call("LogPhase", new ThrowingState()));
    }

    private static void Call(string method, params object?[] args)
    {
        try
        {
            typeof(Plugin).GetMethod(method, Statics)!.Invoke(null, args);
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            throw e.InnerException;
        }
    }

    private static object? GetStatic(string field) => typeof(Plugin).GetField(field, Statics)!.GetValue(null);

    private static void SetStatic(string field, object? value) => typeof(Plugin).GetField(field, Statics)!.SetValue(null, value);
}
