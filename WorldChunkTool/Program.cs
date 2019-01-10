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

        static int Main(string[] args)
        {
            int MagicChunk = 0x00504D43;
            int MagicPKG = 0x20474B50;
            int MagicInputFile;

            string FileInput = "";
            bool FlagPKGExtraction = true;
            bool FlagAutoConfirm = false;

            Console.WriteLine("==============================");
            Utils.Print("WorldChunkTool v1.1.3 by MHVuze", false);            

            // Display commands
            if (args.Length == 0)
            {                
                Console.WriteLine("Usage: \tWorldChunkTool <chunkN_file|PKG_file> (options)\n");
                Console.WriteLine("Options:");
                Console.WriteLine("\t-PkgOnly: Only decompress the PKG file. No further extraction.");
                Console.WriteLine("\t-AutoConfirm: No confirmation required to extract the PKG file.");
                Console.Read();
                return 0;
            }

            // Check file
            FileInput = args[0];
            if (!File.Exists(FileInput)) { Console.WriteLine("ERROR: Specified file doesn't exist."); Console.Read(); return 1; }

            // Set options
            if (args.Any("-PkgOnly".Contains)) { FlagPKGExtraction = true; Console.WriteLine("PKG extraction turned off."); }
            if (args.Any("-AutoConfirm".Contains)) { FlagAutoConfirm = true; Console.WriteLine("Auto confirmation turned on."); }

            // Determine action based on file magic
            try
            {
                using (BinaryReader Reader = new BinaryReader(File.Open(FileInput, FileMode.Open))) MagicInputFile = Reader.ReadInt32();
                if (MagicInputFile == MagicChunk)
                {
                    Console.WriteLine("Chunk file detected.");
                    if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\oo2core_5_win64.dll")) {
                        Console.WriteLine("ERROR: oo2core_5_win64.dll is missing. Can't decompress chunk file.");
                        if (!FlagAutoConfirm) { Console.Read(); }
                        return 2;
                    }
                    Chunk.DecompressChunks(FileInput, FlagPKGExtraction, FlagAutoConfirm);
                    if (!FlagAutoConfirm) { Console.Read(); }
                    return 0;
                }
                else if (MagicInputFile == MagicPKG) 
                {
                    Console.WriteLine("PKG file detected.");
                    PKG.ExtractPKG(FileInput, FlagPKGExtraction, FlagAutoConfirm);
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
            catch (Exception e)
            {
                Console.WriteLine("ERROR: The following exception was caught.");
                Console.WriteLine(e);
                if (!FlagAutoConfirm) { Console.Read(); }
                return 3;
            }
        }
    }
}
