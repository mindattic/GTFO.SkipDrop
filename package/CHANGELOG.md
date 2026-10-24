# Changelog

## v17

- **Joins the GTFO.EZ family's `vN` versioning scheme** (previously numbered independently; last
  independent version was v5), so a matching version number across every GTFO.* mod is easy to spot.
  No change in play.

## v5

- The Thunderstore description now says what the mod does: it skips the opening animation and
  shortens the drop. No change in play.

## v4

- Fixes the game's fast-elevator dev flag getting stuck on after a drop that was cut short (for
  example, leaving mid-drop): it's put back as soon as the game cleans up the elevator, and a later
  drop still restores the original value rather than the stuck one.
- The drop-phase logging can no longer break the elevator's state changes if it hits an error.
- Adds a package icon.
- Adds a config file with one setting, a master switch: `[0. Mod] Enabled` (default `true`) is the
  file's first entry, and setting it to `false` turns the whole mod off - nothing is patched and the
  drop plays at normal speed.

## v3

- Cuts the remaining fixed delays: no wait before the cage releases, no engine start delay, and the
  braking at the bottom drops from its normal duration to 0.1s.
- Logs how long each drop phase took, so whatever's left is visible in `BepInEx/LogOutput.log`.

## v2

- Shortens the fall itself, not just the opening animation: removes the game's forced minimum fall
  time and turns on its built-in fast-elevator mode for the duration of the drop. The cage still
  can't land before the level finishes generating, so that is the new floor.
- Logs how long each drop took to `BepInEx/LogOutput.log`.

## v1

- Initial release: skips the elevator drop sequence at the start of every expedition.
