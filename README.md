# GTFO.SkipDrop

Dev repo for **GTFO.SkipDrop**, a BepInEx IL2CPP plugin for GTFO that skips the elevator drop
sequence at the start of every expedition, so you get into the level without sitting through it
or holding the skip button. That is the entire mod: no config, no other features.

## How it works

One Harmony postfix on `ElevatorRide.StartPreReleaseSequence` calls the game's own
`ElevatorRide.SkipPreReleaseSequence()`, the same thing holding the in-game skip button does.
It also loads faster, because the game only starts building the level once that sequence ends.

Both methods are looked up by name at runtime (`AccessTools`), so the project builds against
BepInEx's core DLLs alone and doesn't need the game's generated interop assemblies. If a game
update renames either method, the plugin logs an error to `BepInEx/LogOutput.log` and does
nothing, rather than crashing the game.

## Credits

The skip logic is taken from the **Skip Elevator Animation** feature of
[TheArchive](https://github.com/AuriRex/GTFO_TheArchive) by AuriRex, used under the MIT License.
The copyright and license notice ship in `package/LICENSE`. If you want that feature along with a
lot of other quality-of-life options, install TheArchive instead of this mod.

## Layout

```
src/GTFO.SkipDrop/    the plugin project (one file: Plugin.cs)
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

There's nothing to unit test; the only code is the patch itself. Build, launch a rundown in the
profile, and confirm the drop is skipped and `LogOutput.log` shows `GTFO.SkipDrop v1.0.0 loaded.`

## Versioning

v1, v2, v3... — major-only bumps, matching the convention across other MindAttic projects.
Thunderstore requires `manifest.json`'s `version_number` to be strict `x.y.z`, so "v1" is
`1.0.0`. `package/CHANGELOG.md` headers use the short `vN` form.

## Publishing to Thunderstore

Still needed before upload: a 256x256 `package/icon.png`. Thunderstore rejects packages without
one.
