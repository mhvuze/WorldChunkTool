using System;
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
        static bool FlagPKGExtraction = true;
        static bool FlagAutoConfirm = false;
        static bool FlagUnpackAll = false;
        static bool FlagPKGDelete = false;

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
                Console.WriteLine("\t-PkgDelete: Delete PKG file after extraction.");
                Console.WriteLine("\t-PkgOnly: Only decompress the PKG file. No further extraction.");
                Console.WriteLine("\t-AutoConfirm: No confirmation required to extract the PKG file.");
                Console.Read();
                return 0;
            }

            FileInput = args[0];            

            // Set options
            if (args.Any("-PkgOnly".Contains)) { FlagPKGExtraction = true; Console.WriteLine("PKG extraction turned off."); }
            if (args.Any("-AutoConfirm".Contains)) { FlagAutoConfirm = true; Console.WriteLine("Auto confirmation turned on."); }
            if (args.Any("-UnpackAll".Contains)) { FlagUnpackAll = true; Console.WriteLine("Unpacking all chunk*.bin files into a single folder."); }
            if (args.Any("-PkgDelete".Contains)) { FlagPKGDelete = true; Console.WriteLine("Deleting PKG file after extraction."); }

            // Determine action based on file magic
            try
            {
                if (FlagUnpackAll && File.GetAttributes(FileInput).HasFlag(FileAttributes.Directory))
                {
                    // MHW: Base Game
                    string[] ChunkFiles = Directory.GetFiles(FileInput, "chunk*.bin", SearchOption.TopDirectoryOnly).CustomSort().ToArray();
                    foreach (string ChunkFile in ChunkFiles) { Console.WriteLine($"Processing {ChunkFile}."); ProcessFile(ChunkFile); }
                }
                else  ProcessFile(FileInput);
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
                if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\oo2core_5_win64.dll"))
                {
                    Console.WriteLine("ERROR: oo2core_5_win64.dll is missing. Can't decompress chunk file.");
                    if (!FlagAutoConfirm) { Console.Read(); }
                    return 2;
                }
                Chunk.DecompressChunks(FileInput, FlagPKGExtraction, FlagAutoConfirm, FlagUnpackAll, FlagPKGDelete);
                if (!FlagAutoConfirm) { Console.Read(); }
                return 0;
            }
            else if (MagicInputFile == MagicPKG)
            {
                Console.WriteLine("PKG file detected.");
                PKG.ExtractPKG(FileInput, FlagPKGExtraction, FlagAutoConfirm, FlagUnpackAll, FlagPKGDelete);
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
