# GTFO.SkipDrop

Skips the opening elevator animation and shortens the fall at the start of every expedition, so
you get into the level as fast as the game can build it. That's the whole mod — one on/off
switch, no other features.

The cage can't land before the level finishes generating, so the drop won't always be instant.

## Config

`BepInEx/config/MindAttic.GTFO.SkipDrop.cfg`, created on first launch:

| Setting | Default | What it does |
|---|---|---|
| `[0. Mod] Enabled` | `true` | Turn the whole mod on or off. When false, the mod loads nothing: no patches, and the drop plays at the game's normal speed. Takes effect on restart. |

## Credits

The skip logic comes from the **Skip Elevator Animation** feature of
[TheArchive](https://github.com/AuriRex/GTFO_TheArchive) by AuriRex, used under the MIT License
(see `LICENSE`). If you want that feature plus a lot of other quality-of-life options, install
TheArchive instead.
