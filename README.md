# GTFO.SkipDrop

Dev repo for **GTFO.SkipDrop**, a BepInEx IL2CPP plugin for GTFO that skips the elevator drop
sequence at the start of every expedition, so you get into the level without sitting through it
or holding the skip button. That is the entire mod: no config, no other features.

## How it works

Two parts:

1. **Opening animation** — a postfix on `ElevatorRide.StartPreReleaseSequence` calls the game's own
   `ElevatorRide.SkipPreReleaseSequence()`, the same thing holding the in-game skip button does.
2. **The fall** — a prefix on `ElevatorRide.StartElevatorRide` sets `ElevatorRide.MinTimeLooping`
   (the game's forced minimum fall time) to 0 and turns on the game's built-in dev flag
   `Globals.Global.FastElevator`. It also zeroes the ride's fixed delays (`m_startDelay`,
   `m_engineStartDelay`) and cuts the landing brake (`m_endDur`) to 0.1s. A postfix on
   `StopElevatorRide` restores the fast-elevator flag, since it's game-wide, and logs
   `Drop took N.Ns.` to `BepInEx/LogOutput.log`; a prefix on `ChangeState` logs each phase's
   duration (`Middle` is the cage looping while the level generates).

The cage can't land before the level finishes generating, so level generation time is the floor;
on a slow machine or a big level the drop won't get down to a couple of seconds.

Everything is looked up by name at runtime (`AccessTools`), so the project builds against
BepInEx's core DLLs alone and doesn't need the game's generated interop assemblies. If a game
update renames any of it, the plugin logs an error and does nothing, rather than crashing the game.

Multiplayer is untested. The host decides when the ride stops, so in a lobby where others don't
have the mod, expect the drop to be gated by the slowest player.

## Credits

The skip logic is taken from the **Skip Elevator Animation** feature of
[TheArchive](https://github.com/AuriRex/GTFO_TheArchive) by AuriRex, used under the MIT License.
The copyright and license notice ship in `package/LICENSE`. If you want that feature along with a
lot of other quality-of-life options, install TheArchive instead of this mod.

## Layout

```
src/GTFO.SkipDrop/    the plugin project (one file: Plugin.cs)
tests/                GTFO.SkipDrop.Tests - NUnit, runs with plain `dotnet test`, no game needed
package/              exactly what ships to Thunderstore / gets zipped for sharing
  manifest.json       Thunderstore package name is "GtfoSkipDrop" (no period allowed there)
  README.md           becomes the Thunderstore page
  CHANGELOG.md
  LICENSE             TheArchive's MIT notice (required attribution)
build.ps1             builds the plugin and packages dist/GTFO.SkipDrop-<version>.zip
```

## Building

Requires BepInExPack_GTFO installed in a Thunderstore Mod Manager or r2modman profile.

```powershell
.\build.ps1                      # uses the Thunderstore Mod Manager profile "Easy Mode DEV"
.\build.ps1 -Profile MyProfile
```

If you're on the older r2modman app, its profiles live under a different path. Pass it directly:
```powershell
dotnet build .\src\GTFO.SkipDrop\GTFO.SkipDrop.csproj -p:BepInEx="$env:AppData\r2modmanPlus-local\GTFO\profiles\Default\BepInEx"
```

This builds straight into that profile's `BepInEx/plugins/GTFO.SkipDrop/` folder, so you can
launch the game and test immediately, and also produces `dist/GTFO.SkipDrop-<version>.zip` for
sharing or upload.

## Testing

```powershell
dotnet test tests/GTFO.SkipDrop.Tests/GTFO.SkipDrop.Tests.csproj
```

NUnit, no game needed. The tests point the plugin's reflected members at fake `ElevatorRide` /
`Global` types and drive the hook methods directly: timings get cut, `FastElevator` is restored
(including after an aborted drop), and no hook ever throws into the game's own method. They still
need the BepInEx profile from Building, since they reference its core DLLs.

The Harmony patching itself only runs against the real game, so after a build also launch a
rundown in the profile and confirm the drop is skipped and `LogOutput.log` shows
`GTFO.SkipDrop vN.0.0 loaded.`

## Versioning

v1, v2, v3... — major-only bumps, matching the convention across other MindAttic projects.
Thunderstore requires `manifest.json`'s `version_number` to be strict `x.y.z`, so "v1" is
`1.0.0`. `package/CHANGELOG.md` headers use the short `vN` form.

## Publishing to Thunderstore

`package/icon.png` (256x256, resized from `logo.png`) is the package icon; Thunderstore rejects
packages without one.
