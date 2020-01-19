# WorldChunkTool
Tool to decompress and extract Monster Hunter: World and Monster Hunter World: Iceborne chunk*.bin files.

```
==============================
WorldChunkTool v1.2 by MHVuze
==============================
Usage: WorldChunkTool <chunk*_file|PKG_file|chunk*_dir> (options)

Options:
        -UnpackAll: Unpack all chunk*.bin files in the provided directory into a single folder.
        -AutoConfirm: No confirmations required. (-UnpackAll uses own confirmation settings.)
        -BuildPKG: Build PKG file from chunks and create data sheet. No extraction. For research purposes only.
        -BaseGame: Switch to legacy mode for MH:W base game chunks (pre-IB update).
```

*oo2core_8_win64.dll* required. Monster Hunter World: Iceborne **DOES NOT** ship with this file. *oo2core_5_win64.dll* from the base game is outdated and won't work! You can find the new DLL in recent games like [Warframe](https://store.steampowered.com/app/230410/Warframe/) (F2P) and [STAR WARS Jedi: Fallen Order](https://store.steampowered.com/app/1172380/STAR_WARS_Jedi_Fallen_Order/). Simply search for it in the game installation directory and copy it to the same folder as the WorldChunkTool executable.

**WARNING:** Please be careful and don't download *oo2core_8_win64.dll* randomly from the web.

### Acknowledgements:
- For help with the tool itself: Jodo, Asterisk, Stracker
- For providing the final Iceborne chunk key order: legendff
- For working on a keygen regarding Iceborne chunk encryption: MoonBunnie, Asterisk
- For help with bruteforcing the Iceborne chunk key order early on: XunLi, Asterisk, Dallagen, MoonBunnie, Kiranico, DMQW, Aradi147, Ice, Jodo, Miralis
