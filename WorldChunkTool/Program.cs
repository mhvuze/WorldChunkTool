using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WorldChunkTool
{
    class Program
    {
        /* Exit Codes:
         * 0 - No error
         * 1 - Input file missing
         * 2 - oo2core_5_win64.dll file missing
         * 3 - Other exception
         */

        static int MagicChunk = 0x00504D43;
        static int MagicPKG = 0x20474B50;
        static int MagicInputFile;

        static string FileInput = "";
        static bool FlagBuildPkg = false;
        static bool FlagBaseGame = false;
        static bool FlagAutoConfirm = false;
        static bool FlagUnpackAll = false;

        static int Main(string[] args)
        {
            Console.WriteLine("==============================");
            Utils.Print("WorldChunkTool v1.2 by MHVuze", false);            

            // Display commands
            if (args.Length == 0)
            {                
                Console.WriteLine("Usage: \tWorldChunkTool <chunk*_file|PKG_file|chunk*_dir> (options)\n");
                Console.WriteLine("Options:");
                Console.WriteLine("\t-UnpackAll: Unpack all chunk*.bin files in the provided directory into a single folder.");
                Console.WriteLine("\t-AutoConfirm: No confirmations required.");
                Console.WriteLine("\t-BuildPKG: Build PKG file from chunks and create data sheet. No extraction. For research purposes only.");
                Console.WriteLine("\t-BaseGame: Switch to legacy mode for MH:W base game chunks (pre-IB update).");
                Console.Read();
                return 0;
            }

            FileInput = args[0];            

            // Set options
            if (args.Any("-AutoConfirm".Contains)) { FlagAutoConfirm = true; Utils.Print("Auto confirmation turned on.", false); }
            if (args.Any("-UnpackAll".Contains)) { FlagUnpackAll = true; FlagAutoConfirm = true; Utils.Print("Unpacking all chunk*.bin files into a single folder.", false); }
            if (args.Any("-BuildPKG".Contains)) { FlagBuildPkg = true; Utils.Print("Building PKG.", false); }
            if (args.Any("-BaseGame".Contains)) { FlagBaseGame = true; Utils.Print("Using legacy mode for MH:W base game chunks.", false); }

            // Determine action based on file magic
            try
            {
                if (FlagUnpackAll && File.GetAttributes(FileInput).HasFlag(FileAttributes.Directory))
                {
                    // MHW: Base Game
                    string[] ChunkFiles = Directory.GetFiles(FileInput, "chunk*.bin", SearchOption.TopDirectoryOnly).CustomSort().ToArray();
                    foreach (string ChunkFile in ChunkFiles) { Console.WriteLine($"Processing {ChunkFile}."); ProcessFile(ChunkFile); }
                }
                else ProcessFile(FileInput);
                if (FlagUnpackAll) { Console.WriteLine($"Output at: {Environment.CurrentDirectory}\\chunk_combined"); Console.WriteLine("Press Enter to quit"); Console.Read(); }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: The following exception was caught.");
                Console.WriteLine(e);
                if (!FlagAutoConfirm) { Console.Read(); }
                return 3;
            }
        }

        static int ProcessFile(string FileInput)
        {
            if (!File.Exists(FileInput)) { Console.WriteLine("ERROR: Specified file doesn't exist."); Console.Read(); return 1; }
            using (BinaryReader Reader = new BinaryReader(File.Open(FileInput, FileMode.Open, FileAccess.Read, FileShare.Read))) MagicInputFile = Reader.ReadInt32();
            if (MagicInputFile == MagicChunk)
            {
                Console.WriteLine("Chunk file detected.");
                if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\oo2core_8_win64.dll"))
                {
                    Console.WriteLine("ERROR: oo2core_8_win64.dll is missing. Can't decompress chunk file.");
                    if (!FlagAutoConfirm) { Console.Read(); }
                    return 2;
                }

                // Build PKG
                if (FlagBuildPkg) { Chunk.DecompressChunks(FileInput, FlagAutoConfirm, FlagUnpackAll, FlagBaseGame); }
                // On-the-fly decompression and unpacking
                else
                {
                    ChunkOTF ChunkOtfInst = new ChunkOTF();
                    List<FileNode> FileCatalog = new List<FileNode>();
                    string FilePath = $"{Environment.CurrentDirectory}\\{Path.GetFileNameWithoutExtension(FileInput)}";
                    if (FlagUnpackAll) { FilePath = $"{Environment.CurrentDirectory}\\chunk_combined"; }
                    FileCatalog = ChunkOtfInst.AnalyzeChunk(FileInput, FileCatalog, FlagBaseGame);
                    Console.WriteLine("Extracting chunk file, please wait.");
                    ChunkOtfInst.ExtractSelected(FileCatalog, FilePath, FlagBaseGame);
                    Utils.Print("\nFinished.", false);
                    if (!FlagUnpackAll) { Utils.Print($"Output at: {FilePath}", false); }
                    if (!FlagAutoConfirm) { Console.WriteLine("Press Enter to quit"); }
                    if (!FlagAutoConfirm) { Console.Read(); }
                }

                return 0;
            }
            else if (MagicInputFile == MagicPKG)
            {
                Console.WriteLine("PKG file detected.");
                PKG.ExtractPKG(FileInput, FlagAutoConfirm, FlagUnpackAll, false);
                if (!FlagAutoConfirm) { Console.Read(); }
                return 0;
            }
            else
            {
                Console.WriteLine($"ERROR: Invalid magic {MagicInputFile.ToString("X8")}.");
                if (!FlagAutoConfirm) { Console.Read(); }
                return 0;
            }
        }
    }
}
