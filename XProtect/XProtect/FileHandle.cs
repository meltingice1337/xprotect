using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Resources;
using System.Text.RegularExpressions;
    class FileHandle
    {
        public struct FileEx
        {
           public string name;
           public byte[] data;
        }

        public static string GetString(byte[] data)
        {
            return Encoding.Default.GetString(data);
        }
        public static byte[] GetBytes( string data)
        {
            return Encoding.Default.GetBytes(data);
        }
        private static byte[] toBytes( long data, int size)
        {
            byte[] bytes = new byte[size];
            return bytes;
        }
        public static byte[] CombineFiles(FileEx[] data)
        {
            List<byte> result = new List<byte>();

            // Generating the header
            int pos = 0;
            string toAdd  = "";
            foreach(var file in data)
            {
                int future = pos + file.data.Length;
                toAdd += string.Format("[|{0}|{1}|{2}|]", file.name, pos, file.data.Length);
                pos = future;
            }
            result.AddRange(GetBytes(toAdd));

            //Adding the header's size
            result.InsertRange(0, BitConverter.GetBytes(result.Count));

            //Adding the file data
            foreach(var file in data)
            {
                result.AddRange(file.data);
            }
            return result.ToArray();

        }
        public static FileEx[] SplitFiles(byte[] data)
        {
            List<FileEx> result = new List<FileEx>();

            // Get the header size
            int headerSize = BitConverter.ToInt32(data,0);

            // Get the header
            byte[] header = new byte[headerSize];
            Buffer.BlockCopy(data, 4, header, 0, headerSize);
            string headerText = GetString(header);
            
            // The offset from where the bytes of the first file will start
            int initialOffset = headerSize + 4;

            // For each file create a new fileex item and add it to the result
            foreach (Match match in Regex.Matches(headerText, @"(\[\|)(.*?)(\|\])"))
            {
                var matches = Regex.Matches(match.Value, @"(?<=\|)(.*?)(?=\|)");
                FileEx item = new FileEx();
                int start=0, len=0;
                for(int i = 0;i<3;i++)
                {
                    string val = matches[i].Value;
                    switch(i)
                    {
                        case 0:
                            item.name = val;
                            break;
                        case 1:
                            start = int.Parse(val);
                            break;
                        case 2:
                            len = int.Parse(val);
                            break;
                    }
                }
                item.data = new byte[len];
                Buffer.BlockCopy(data, initialOffset + start, item.data, 0,  len);
                result.Add(item);
            }
            return result.ToArray();
        }
    }
