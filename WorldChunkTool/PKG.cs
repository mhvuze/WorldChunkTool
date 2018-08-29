// VisualStudio is a piece of garbage and nuked this file while it was open during an outage, 
// so I had to decompile and edit it from my last build before that.
// Excuse the lack of comments.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WorldChunkTool
{
    class PKG
    {
        public static void ExtractPKG(string FileInput, bool FlagPKGExtraction)
        {
            Dictionary<string, int> AffectedFiles = new Dictionary<string, int>();
            BinaryReader Reader = new BinaryReader(File.Open(FileInput, FileMode.Open));
            StreamWriter LogWriter = new StreamWriter($"{Path.GetFileNameWithoutExtension(FileInput)}.csv", false);
            LogWriter.WriteLine("Index,Offset,Size,EntryType,Unk,Directory,FileName,FileType");

            Reader.BaseStream.Seek(0x0C, SeekOrigin.Begin);
            int TotalParentCount = Reader.ReadInt32();
            int ParentPadding = TotalParentCount.ToString().Length;
            int TotalChildrenCount = Reader.ReadInt32();
            Console.WriteLine($"PKG file has {TotalParentCount} parent entries with {TotalChildrenCount} children entries.");
            Console.WriteLine("==============================");

            Reader.BaseStream.Seek(0x100, SeekOrigin.Begin);
            for (int i = 0; i < TotalParentCount; i++)
            {
                byte[] ArrayNameParent = Reader.ReadBytes(0x3C).Where(b => b != 0x00).ToArray();
                string StringNameParent;
                long FileSize = Reader.ReadInt64();
                long FileOffset = Reader.ReadInt64();
                int EntryType = Reader.ReadInt32();
                int CountChildren = Reader.ReadInt32();

                for (int j = 0; j < CountChildren; j++)
                {
                    Console.Write($"\rParent entry {(i + 1).ToString().PadLeft(ParentPadding)}/{TotalParentCount}. Processing child entry {(j + 1).ToString().PadLeft(4)} / {CountChildren.ToString().PadLeft(4)}...");

                    long ReaderPositionSubFile = Reader.BaseStream.Position;
                    byte[] ArrayNameChild = Reader.ReadBytes(0xA0).Where(b => b != 0x00).ToArray();
                    FileSize = Reader.ReadInt64();
                    FileOffset = Reader.ReadInt64();
                    EntryType = Reader.ReadInt32();
                    int Unknown = Reader.ReadInt32();

                    if (EntryType == 0x02)
                    {
                        Reader.BaseStream.Seek(ReaderPositionSubFile, SeekOrigin.Begin);
                        ArrayNameChild = Reader.ReadBytes(0x50).Where(b => b != 0x00).ToArray();
                        Reader.BaseStream.Seek(0x68, SeekOrigin.Current);
                    }
                    StringNameParent = Encoding.UTF8.GetString(ArrayNameChild);

                    if (EntryType == 0x02 || EntryType == 0x00)
                    {
                        long ReaderPositionBeforeEntry = Reader.BaseStream.Position;
                        Reader.BaseStream.Seek(FileOffset, SeekOrigin.Begin);
                        byte[] ArrayFileData = Reader.ReadBytes(Convert.ToInt32(FileSize));

                        if (FlagPKGExtraction)
                        {
                            new FileInfo($"{Environment.CurrentDirectory}\\{Path.GetFileNameWithoutExtension(FileInput)}\\{StringNameParent}").Directory.Create();
                            File.WriteAllBytes($"{Environment.CurrentDirectory}\\{Path.GetFileNameWithoutExtension(FileInput)}\\{StringNameParent}", ArrayFileData);
                        }
                        Reader.BaseStream.Seek(ReaderPositionBeforeEntry, SeekOrigin.Begin);
                    }

                    if (EntryType != 0x01)
                    {
                        LogWriter.WriteLine(
                            i + "," +
                            FileOffset.ToString("X16") + "," +
                            FileSize + "," +
                            EntryType + "," +
                            Unknown + "," +
                            StringNameParent.Remove(StringNameParent.LastIndexOf('\\') + 1) + "," +
                            StringNameParent.Substring(StringNameParent.LastIndexOf('\\') + 1) + "," +
                            StringNameParent.Substring(StringNameParent.IndexOf('.') + 1)
                        );
                    }
                }
            }
            Reader.Close();
            LogWriter.Close();

            Console.WriteLine("\n==============================");
            Console.WriteLine("Finished. Press Enter to quit.");
        }
    }
}
