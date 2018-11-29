using System;
using System.IO;

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
            string StrPKGExtraction = "";
            string AutoConfirm = "";
            bool FlagPKGExtraction = true;
            bool FlagAutoConfirm = false;

            Console.WriteLine("==============================");
            Utils.Print("WorldChunkTool v1.1.1 by MHVuze", false);            

            // Display commands
            if (args.Length == 0)
            {                
                Console.WriteLine("Usage: WorldChunkTool <chunkN_file|PKG_file> (PKGext) (AutoConf)");
                Console.WriteLine("PKGext: use 'false' to turn off PKG file extraction, defaults to 'true'");
                Console.WriteLine("AutoConf: Use 'true' to bypass confirmation prompts, defaults to 'false'");
                Console.Read();
                return 0;
            }

            // Check file
            FileInput = args[0];
            if (!File.Exists(FileInput)) { Console.WriteLine("ERROR: Specified file doesn't exist."); Console.Read(); return 1; }

            // Turn PKG extraction output on or off
            if (args.Length > 1) StrPKGExtraction = args[1];
            if (StrPKGExtraction.Equals("false", StringComparison.InvariantCultureIgnoreCase)) { FlagPKGExtraction = false; Console.WriteLine("PKG extraction turned off."); }

            // Accept confirmation prompts
            if (args.Length > 2) AutoConfirm = args[2];
            if (AutoConfirm.Equals("true", StringComparison.InvariantCultureIgnoreCase)) { FlagAutoConfirm = true; Console.WriteLine("Auto Confirmation turned on."); }


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
