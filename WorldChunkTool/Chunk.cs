using System;
using System.Collections.Generic;
using System.IO;

namespace WorldChunkTool
{
    class Chunk
    {
        public static void DecompressChunks(String FileInput, bool FlagAutoConfirm, bool FlagUnpackAll, bool FlagBaseGame)
        {
            string NamePKG = $"{Environment.CurrentDirectory}\\{Path.GetFileNameWithoutExtension(FileInput)}.pkg";
            BinaryReader Reader = new BinaryReader(File.Open(FileInput, FileMode.Open, FileAccess.Read, FileShare.Read));

            // Key = ChunkOffset, Value = ChunkSize
            Dictionary<long, long> MetaChunk = new Dictionary<long, long>();

            // Read header
            Reader.BaseStream.Seek(4, SeekOrigin.Begin);
            int ChunkCount = Reader.ReadInt32(); int ChunkPadding = ChunkCount.ToString().Length;
            Utils.Print($"{ChunkCount} chunks in this file.", false);

            // Read file list
            long totalChunkSize = 0;
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
                ChunkSize = (ChunkSize >> 4) + (ChunkSize & 0xF);
                totalChunkSize += ChunkSize;

                // Process offset
                byte[] ArrayTmp2 = new byte[8];
                byte[] ArrayChunkOffset = Reader.ReadBytes(5);
                Array.Copy(ArrayChunkOffset, ArrayTmp2, ArrayChunkOffset.Length);
                long ChunkOffset = BitConverter.ToInt64(ArrayTmp2, 0);

                MetaChunk.Add(ChunkOffset, ChunkSize);
                Console.WriteLine((i + 1).ToString("D8") + "," + ChunkOffset + "," + ChunkSize);
            }
            Console.WriteLine(totalChunkSize);
            //SortedDictionary<long, long> sortedDict = new SortedDictionary<long, long>(MetaChunk);

            // Write decompressed chunks to pkg
            BinaryWriter Writer = new BinaryWriter(File.Create(NamePKG));
            int DictCount = 1;

            foreach (KeyValuePair<long, long> Entry in MetaChunk)
            {
                Console.Write($"\rProcessing {DictCount.ToString().PadLeft(ChunkPadding)} / {ChunkCount}...");
                if (Entry.Value != 0)
                {
                    Reader.BaseStream.Seek(Entry.Key, SeekOrigin.Begin);
                    byte[] ChunkCompressed = Reader.ReadBytes((int)Entry.Value); // Unsafe cast
                    //BinaryWriter Writer3 = new BinaryWriter(File.Create($"{Environment.CurrentDirectory}\\G60_cmp\\chunk_{DictCount.ToString("D8")}_cmp.bin"));
                    //Writer3.Write(ChunkCompressed);
                    //Writer3.Close();

                    byte[] ChunkDecompressed = Utils.Decompress(ChunkCompressed, ChunkCompressed.Length, 0x40000);
                    //BinaryWriter Writer2 = new BinaryWriter(File.Create($"{Environment.CurrentDirectory}\\G60_decmp\\chunk_{DictCount.ToString("D8")}_decmp.bin"));
                    //Writer2.Write(ChunkDecompressed);
                    //Writer2.Close();

                    if (!FlagBaseGame) { Utils.DecryptChunk(ChunkDecompressed, Utils.GetChunkKey(DictCount - 1)); }
                    //BinaryWriter Writer4 = new BinaryWriter(File.Create($"{Environment.CurrentDirectory}\\G60_decrypt\\chunk_{DictCount.ToString("D8")}_decrypt.bin"));
                    //Writer4.Write(ChunkDecompressed);
                    //Writer4.Close();

                    Writer.Write(ChunkDecompressed);
                }
                else
                {
                    Reader.BaseStream.Seek(Entry.Key, SeekOrigin.Begin);
                    byte[] ChunkDecompressed = Reader.ReadBytes(0x40000);
                    Writer.Write(ChunkDecompressed);
                }
                DictCount++;
            }
            Reader.Close();
            Writer.Close();

            Utils.Print("Finished.", true);
            Utils.Print($"Output at: {NamePKG}", false);

            if (!FlagAutoConfirm)
            {
                Console.WriteLine("Press Enter to quit.");
                Console.Read();
            }

            Console.SetCursorPosition(0, Console.CursorTop - 1); // remove this
            Console.WriteLine("=============================="); // remove this
            PKG.ExtractPKG(NamePKG, FlagAutoConfirm, FlagUnpackAll); // remove this
            if (!FlagAutoConfirm) { Console.Read(); } // remove this
        }
    }
}