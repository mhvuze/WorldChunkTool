# WorldChunkTool
Tool to decompress Monster Hunter: World chunkN.bin files and extract the resulting PKG file.

```
==============================
WorldChunkTool v1.1.4 by MHVuze
==============================
Usage:  WorldChunkTool <chunkN_file|PKG_file|chunkN_dir> (options)

Options:
        -UnpackAll: Unpack all chunkN.bin files in the provided directory into a single folder.
        -PkgDelete: Delete PKG file after extraction.
        -PkgOnly: Only decompress the PKG file. No further extraction.
        -AutoConfirm: No confirmation required to extract the PKG file.
```

*oo2core_5_win64.dll* required. Copy it from the install directory of Monster Hunter: World to the same folder as the WorldChunkTool executable.

**Warning:** You need A LOT of space to decompress huge chunkN.bin files. To be sure, calculate 3 * chunkN.bin size.
