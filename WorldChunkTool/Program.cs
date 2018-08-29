using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace WorldChunkTool
{
    class Program
    {
        // Import Oodle decompression
        [DllImport("oo2core_5_win64.dll")]
        private static extern int OodleLZ_Decompress(byte[] buffer, long bufferSize, byte[] outputBuffer, long outputBufferSize, uint a, uint b, ulong c, uint d, uint e, uint f, uint g, uint h, uint i, uint threadModule);

        static void Main(string[] args)
        {
            int MagicChunk = 0x00504D43;
            int MagicPKG = 0x20474B50;
            int MagicInputFile;

            string FileInput = "";
            string StrPKGExtraction = "";
            bool FlagPKGExtraction = true;

            Console.WriteLine("WorldChunkTool by MHVuze");
            Console.WriteLine("==============================");

            // Display commands
            if (args.Length == 0)
            {                
                Console.WriteLine("Usage: WorldChunkTool <chunkN_file|PKG_file> (PKGext)");
                Console.WriteLine("PKGext: use 'false' to turn off PKG file extraction, defaults to 'true'");
                Console.Read();
                return;
            }

            // Check file
            FileInput = args[0];
            if (!File.Exists(FileInput)) { Console.WriteLine("ERROR: Specified file doesn't exist."); Console.Read(); return; }

            // Turn file output on or off
            if (args.Length > 1) StrPKGExtraction = args[1];
            if (StrPKGExtraction.Equals("false", StringComparison.InvariantCultureIgnoreCase)) { FlagPKGExtraction = false; Console.WriteLine("PKG extraction turned off."); }

            using (BinaryReader Reader = new BinaryReader(File.Open(FileInput, FileMode.Open))) MagicInputFile = Reader.ReadInt32();

            if (MagicInputFile == MagicChunk) { Console.WriteLine("Chunk file detected."); Chunk.DecompressChunks(FileInput, FlagPKGExtraction); Console.Read(); }

            else if (MagicInputFile == MagicPKG)
            {
                Console.WriteLine("PKG file detected.");
                PKG.ExtractPKG(FileInput, FlagPKGExtraction);
                Console.Read();
            }
            else { Console.WriteLine($"Invalid magic {MagicInputFile.ToString("X8")}."); Console.Read(); return; }
            Console.Read();
        }

        // Decompress oodle chunk
        // Part of https://github.com/Crauzer/OodleSharp
        public static byte[] Decompress(byte[] buffer, int size, int uncompressedSize)
        {
            byte[] decompressedBuffer = new byte[uncompressedSize];
            int decompressedCount = OodleLZ_Decompress(buffer, size, decompressedBuffer, uncompressedSize, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);

            if (decompressedCount == uncompressedSize)
            {
                return decompressedBuffer;
            }
            else if (decompressedCount < uncompressedSize)
            {
                return decompressedBuffer.Take(decompressedCount).ToArray();
            }
            else
            {
                throw new Exception("There was an error while decompressing.");
            }
        }
    }
}
