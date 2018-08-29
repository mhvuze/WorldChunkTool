using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldChunkTool
{
    class Chunk
    {
        public static void DecompressChunks(String FileInput, bool FlagPKGExtraction)
        {
            string NamePKG = $"{Environment.CurrentDirectory}\\{Path.GetFileNameWithoutExtension(FileInput)}.pkg";
            BinaryReader Reader = new BinaryReader(File.Open(FileInput, FileMode.Open));

            // Key = ChunkOffset, Value = ChunkSize
            Dictionary<long, long> MetaChunk = new Dictionary<long, long>();

            // Read header
            Reader.BaseStream.Seek(4, SeekOrigin.Begin);
            int ChunkCount = Reader.ReadInt32(); int ChunkPadding = ChunkCount.ToString().Length;
            Console.WriteLine($"{ChunkCount} chunks in this file.");
            Console.WriteLine("==============================");

            // Read file list
            for (int i = 0; i < ChunkCount; i++)
            {
                // Process file size
                byte[] ArrayTmp1 = new byte[8];
                byte[] ArrayChunkSize = Reader.ReadBytes(3);
                //int Low = ArrayChunkSize[0] & 0x0F;
                int High = ArrayChunkSize[0] >> 4;
                ArrayChunkSize[0] = BitConverter.GetBytes(High)[0];
                Array.Copy(ArrayChunkSize, ArrayTmp1, ArrayChunkSize.Length);
                long ChunkSize = BitConverter.ToInt64(ArrayTmp1, 0);

                // Process offset
                byte[] ArrayTmp2 = new byte[8];
                byte[] ArrayChunkOffset = Reader.ReadBytes(5);
                Array.Copy(ArrayChunkOffset, ArrayTmp2, ArrayChunkOffset.Length);
                long ChunkOffset = BitConverter.ToInt64(ArrayTmp2, 0);

                MetaChunk.Add(ChunkOffset, ChunkSize);
            }

            // Write decompressed chunks to pkg
            BinaryWriter Writer = new BinaryWriter(File.Create(NamePKG));
            int DictCount = 1;
            //byte[] LastNonNullChunk = new byte[0x40000];
            byte[] Nullspace = new byte[0x40000];

            foreach (KeyValuePair<long, long> Entry in MetaChunk)
            {
                Console.Write($"\rProcessing {DictCount.ToString().PadLeft(ChunkPadding)} / {ChunkCount}...");                

                // DEBUG remove before release
                if (DictCount == 633)
                {
                    break;
                }
                if (Entry.Value != 0)
                {
                    Reader.BaseStream.Seek(Entry.Key, SeekOrigin.Begin);
                    byte[] ChunkCompressed = Reader.ReadBytes((int)Entry.Value); // Unsafe cast
                    byte[] ChunkDecompressed = Program.Decompress(ChunkCompressed, ChunkCompressed.Length, 0x40000);
                    //LastNonNullChunk = ChunkDecompressed;
                    Writer.Write(ChunkDecompressed);
                }
                else
                {
                    //Writer.Write(LastNonNullChunk);
                    Writer.Write(Nullspace);
                }
                DictCount++;
            }
            Reader.Close();
            Writer.Close();

            Console.WriteLine("\n==============================");
            Console.WriteLine("Finished.");
            Console.WriteLine($"Output at: {NamePKG}");
            Console.WriteLine("==============================");
            Console.WriteLine("The PKG file will now be extracted. Press Enter to continue or close window to quit.");
            Console.Read();

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine("==============================");
            PKG.ExtractPKG(NamePKG, FlagPKGExtraction);
            Console.Read();
        }
    }
}
