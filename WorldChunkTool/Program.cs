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
        [DllImport("oo2core_5_win64.dll")]
        private static extern int OodleLZ_Decompress(byte[] buffer, long bufferSize, byte[] outputBuffer, long outputBufferSize, uint a, uint b, ulong c, uint d, uint e, uint f, uint g, uint h, uint i, uint threadModule);

        static void Main(string[] args)
        {
            int magic_chunk = 0x00504D43;   // 43 4D 50 00
            int magic_pkg = 0x20474B50;     // 50 4B 47 20
            string input_str = "";
            string PKGextraction = "";
            bool PKGextraction_flag = true;

            // Display commands
            if (args.Length == 0)
            {
                Console.WriteLine("WorldChunkTool by MHVuze");
                Console.WriteLine("Usage: WorldChunkTool <chunkN_file> (PKGext)");
                Console.WriteLine("PKGext: use 'false' to turn off PKG file extraction, defaults to 'true'");
                Console.Read();
                return;
            }

            // Check file
            input_str = args[0];         
            if (!File.Exists(input_str)) { Console.WriteLine("ERROR: Specified file doesn't exist."); return; }

            // Turn file output on or off
            if (args.Length > 1) { PKGextraction = args[1]; }
            if (PKGextraction.Equals("false", StringComparison.InvariantCultureIgnoreCase)) { PKGextraction_flag = false; }
            if (PKGextraction_flag == false) { Console.WriteLine("PKG extraction turned off.\n"); }

            using (BinaryReader reader = new BinaryReader(File.Open(input_str, FileMode.Open)))
            {
                // Check magic
                int magic = reader.ReadInt32();

                #region Chunk Handling
                if (magic == magic_chunk) {

                    Console.WriteLine("Chunk file detected.");

                    // Read header
                    int file_count = reader.ReadInt32();
                    Console.WriteLine(file_count + " (h" + file_count.ToString("X4") + ") chunks in this file.\n");

                    // Prep pkg output
                    int pkg_split_count = 0;
                    int pkg_file_count = 0;
                    int pkg_pos = 0;
                    int pkg_part_size = file_count * 0x40000;
                    bool split_pkg = false;
                    if (file_count > 8000)
                    {
                        pkg_part_size = 8000 * 0x40000;
                        split_pkg = true;
                    }
                    byte[] pkg = new byte[pkg_part_size];

                    // Read file list
                    for (int i = 0; i < file_count; i++)
                    {
                        // Process file size
                        byte[] temp1 = new byte[8];
                        byte[] bfile_size = reader.ReadBytes(3);
                        var low = bfile_size[0] & 0x0F;
                        var high = bfile_size[0] >> 4;
                        bfile_size[0] = BitConverter.GetBytes(high)[0];
                        Array.Copy(bfile_size, temp1, bfile_size.Length);
                        long file_size = BitConverter.ToInt64(temp1, 0);

                        // Process offset
                        byte[] temp2 = new byte[8];
                        byte[] bfile_offset = reader.ReadBytes(5);
                        Array.Copy(bfile_offset, temp2, bfile_offset.Length);
                        long file_offset = BitConverter.ToInt64(temp2, 0);
                        long cur_offset = reader.BaseStream.Position;

                        // Extract file
                        reader.BaseStream.Seek(file_offset, SeekOrigin.Begin);
                        int cmp_flag = 0;
                        if (file_size != 0)
                        {
                            cmp_flag = reader.ReadInt16();
                        }
                        reader.BaseStream.Seek(-2, SeekOrigin.Current);

                        // Write uncompressed pkg to disk
                        byte[] block = new byte[0x40000];

                        if (file_size == 0)
                        {
                            pkg_pos += 0x40000;
                            pkg_file_count++;
                        }
                        else
                        {
                            byte[] file_data = new byte[long.Parse(file_size.ToString("X17").Remove(15, 1), System.Globalization.NumberStyles.HexNumber)];
                            file_data = reader.ReadBytes(file_data.Length);
                            block = Decompress(file_data, file_data.Length, 0x40000);
                            Buffer.BlockCopy(block, 0, pkg, pkg_pos, 0x40000);

                            pkg_pos += 0x40000;
                            pkg_file_count++;
                        }

                        reader.BaseStream.Seek(cur_offset, SeekOrigin.Begin);

                        Console.WriteLine("File " + i.ToString("D8") +
                            " | Offset: " + file_offset.ToString("X16") +
                            " | Size: " + file_size.ToString("X17").Remove(15, 1) +
                            " | Low: " + low.ToString("X8") +
                            " | CMP: " + cmp_flag.ToString("X4")
                            );

                        // Handle pkg splitting if necessary
                        if (split_pkg == true && pkg_file_count == 8000)
                        {
                            Console.WriteLine("Writing pkg part to disk...");
                            File.WriteAllBytes(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(input_str) + "." + pkg_split_count.ToString("D2") + ".pkgpart", pkg);
                            pkg_split_count++;
                            pkg_file_count = 0;
                            pkg_pos = 0;
                            pkg = new byte[pkg_part_size];
                        }
                    }

                    // Final pkg handling
                    if (split_pkg == false)
                    {
                        File.WriteAllBytes(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(input_str) + ".pkg", pkg);
                    }
                    else
                    {
                        // Write last part
                        Console.WriteLine("Writing pkg part to disk...");
                        byte[] remaining_array = new byte[(file_count - (8000 * pkg_split_count)) * 0x40000];
                        Buffer.BlockCopy(pkg, 0, remaining_array, 0, remaining_array.Length);
                        File.WriteAllBytes(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(input_str) + "." + pkg_split_count.ToString("D2") + ".pkgpart", remaining_array);

                        // Merge all parts
                        Console.WriteLine("Merging all pkg parts... this will take a while.");
                        string[] inputFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.pkgpart");
                        using (Stream StreamOutput = File.OpenWrite(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(input_str) + ".pkg"))
                        {
                            foreach (string inputFile in inputFiles)
                            {
                                using (Stream input = File.OpenRead(inputFile))
                                {
                                    input.CopyTo(StreamOutput);
                                }
                                File.Delete(inputFile);
                            }
                        }
                    }

                    Console.WriteLine("\nFinished");
                    Console.WriteLine("Output at: " + Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(input_str) + ".pkg");
                }
                #endregion

                # region PKG Handling
                if (magic == magic_pkg)
                {
                    Console.WriteLine("PKG file detected.");

                    // Initialize csv writer
                    StreamWriter csv = new StreamWriter("pkg.csv", false);
                    csv.WriteLine("Index,Offset,Size,EntryType,Unk4,Path,FileType");

                    // Get PKG info
                    reader.BaseStream.Seek(0xC, SeekOrigin.Begin);
                    int total_parent_count = reader.ReadInt32();
                    int total_children_count = reader.ReadInt32();
                    Console.WriteLine("PKG file has " + total_parent_count + " parent directory entries.");
                    Console.WriteLine("PKG file has " + total_children_count + " children entries.");
                    Console.WriteLine("---------------------------");

                    reader.BaseStream.Seek(0x100, SeekOrigin.Begin);
                    long reader_pos = 0;

                    // Process entries
                    for (int j = 0; j < total_parent_count; j++)
                    {
                        byte[] top_name = reader.ReadBytes(0x3C).Where(b => b != 0x00).ToArray();
                        long file_size = reader.ReadInt64();
                        long file_offset = reader.ReadInt64();
                        int EntryType = reader.ReadInt32();
                        int sub_file_count = reader.ReadInt32();

                        string string_name = Encoding.UTF8.GetString(top_name);

                        Console.WriteLine(string_name + " has " + sub_file_count + " sub files or folders.");

                        for (int i = 0; i < sub_file_count; i++)
                        {                            
                            long pos = reader.BaseStream.Position;
                            var remap = new byte[50];

                            byte[] name = reader.ReadBytes(0xA0).Where(b => b != 0x00).ToArray();
                            file_size = reader.ReadInt64();
                            file_offset = reader.ReadInt64();
                            EntryType = reader.ReadInt32();
                            int unk4 = reader.ReadInt32();

                            // Stuff for 0x02 flag files
                            if (EntryType == 0x02)
                            {
                                reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                                name = reader.ReadBytes(0x50).Where(b => b != 0x00).ToArray();
                                remap = reader.ReadBytes(0x50).Where(b => b != 0x00).ToArray();
                                reader.BaseStream.Seek(0x18, SeekOrigin.Current);
                                string remap_string_name = Encoding.UTF8.GetString(remap);
                            }

                            // Extract regular files
                            string_name = Encoding.UTF8.GetString(name);
                            if (EntryType == 0x02 || EntryType == 0x00)
                            {
                                reader_pos = reader.BaseStream.Position;
                                reader.BaseStream.Seek(file_offset, SeekOrigin.Begin);
                                byte[] file = reader.ReadBytes(Convert.ToInt32(file_size));                                
                                if (PKGextraction_flag == true) {
                                    (new FileInfo(Environment.CurrentDirectory + "\\out\\" + string_name)).Directory.Create();
                                    File.WriteAllBytes(Environment.CurrentDirectory + "\\out\\" + string_name, file);
                                }                                
                                reader.BaseStream.Seek(reader_pos, SeekOrigin.Begin);
                            }

                            // Output entry info to csv and console
                            csv.WriteLine(j + "," +
                                file_offset.ToString("X16") + "," +
                                file_size + "," +
                                EntryType + "," +
                                unk4 + "," +
                                string_name + "," +
                                string_name.Substring(string_name.IndexOf('.') + 1));

                            Console.WriteLine("Index: " + i.ToString("D8") + " | " +
                                "Offset: " + file_offset.ToString("X16") + " | " +
                                "Size: " + file_size.ToString("X16") + " | " + 
                                "Type Flag: " + EntryType.ToString("X8") + " | " + 
                                "Unk4: " + unk4.ToString("X8"));
                        }
                        Console.WriteLine("---------------------------");                        
                    }
                    Console.WriteLine("Finished");
                    csv.Close();
                }
                # endregion
                if (magic != magic_chunk && magic != magic_pkg)
                {
                    Console.WriteLine("Invalid magic. Check your input file.");
                }
            }
            Console.Read();
        }

        // Read string null-terminated
        public static string ReadNullterminated(BinaryReader reader)
        {
            var char_array = new List<byte>();
            string str = "";
            if (reader.BaseStream.Position == reader.BaseStream.Length)
            {
                byte[] char_bytes2 = char_array.ToArray();
                str = Encoding.UTF8.GetString(char_bytes2);
                return str;
            }
            byte b = reader.ReadByte();
            while ((b != 0x00) && (reader.BaseStream.Position != reader.BaseStream.Length))
            {
                char_array.Add(b);
                b = reader.ReadByte();
            }
            byte[] char_bytes = char_array.ToArray();
            str = Encoding.UTF8.GetString(char_bytes);
            return str;
        }

        // Decompress oodle file
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
