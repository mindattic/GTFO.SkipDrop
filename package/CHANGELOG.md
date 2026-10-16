# Changelog

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
