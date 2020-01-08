# WorldChunkTool

**Please note:** There was an encryption layer added to Iceborne chunks. They cannot be extracted until this is adressed. The repo will be updated asap when a proper way of handling it was figured out.

Tool to decompress Monster Hunter: World chunk*.bin files and extract the resulting PKG file.

```
==============================
WorldChunkTool v1.2 by MHVuze
==============================
Usage:  WorldChunkTool <chunk_file|PKG_file|chunk_dir> (options)

Options:
        -UnpackAll: Unpack all chunk*.bin files in the provided directory into a single folder.
        -PkgDelete: Delete PKG file after extraction.
        -PkgOnly: Only decompress the PKG file. No further extraction.
        -AutoConfirm: No confirmation required to extract the PKG file.
```

*oo2core_5_win64.dll* required. Copy it from the install directory of Monster Hunter: World to the same folder as the WorldChunkTool executable.

**Warning:** You need A LOT of space to decompress huge chunkN.bin files. To be sure, calculate 3 * chunk*.bin size.
