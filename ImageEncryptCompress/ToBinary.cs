using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;


namespace ImageEncryptCompress
{
    internal class ToBinary
    {
        private class TreeSerializer
        {
            private static List<byte> values;
            private static List<byte> flags;

            private static int bit_index = 0;
            private static byte current_byte = 0;
            private static int cnt = 0;

            private static void __Serialize(ref Node<byte> root,BinaryWriter writer)
            {
                cnt++;

                if (bit_index == 8)
                {
                    flags.Add(current_byte);
                    current_byte = 0;
                    bit_index = 0;
                }

                if (root == null)
                {
                    current_byte |= (byte)(1 << (7 - bit_index));
                    bit_index++;
                    return;
                }
                bit_index++;
                
                values.Add(root.data);

                __Serialize(ref root.left, writer);
                __Serialize(ref root.right, writer);
            }
            
            public static void Serialize(ref Node<byte> root,BinaryWriter writer)
            {
                values = new List<byte>();
                flags = new List<byte>();

                TreeSerializer.__Serialize(ref root, writer);
                if (current_byte > 0) flags.Add(current_byte);
                bit_index = 0;
                current_byte = 0;
                writer.Write(flags.Count);
                writer.Write(flags.ToArray());
                writer.Write(values.ToArray());
                flags = values = null;
            }
            private static Node<byte> __Deserialize(BinaryReader reader)
            {
                if ((flags[bit_index / 8] & (1 << (7 - (bit_index % 8)))) != 0)
                {
                    bit_index++;
                    return null; 
                }
                bit_index++;

                byte value = reader.ReadByte();
                Node<byte> node = new Node<byte>((byte)value);

                Node<byte> left = __Deserialize(reader);
                Node<byte> right = __Deserialize(reader);
                
                node.left = left;
                node.right = right;

                return node;
            }

            public static Node<byte> Deserialize(BinaryReader reader)
            {
                int count = reader.ReadInt32();
                flags = reader.ReadBytes(count).ToList<byte>();
                Node<byte> tree = TreeSerializer.__Deserialize(reader);
                flags = null;
                bit_index = 0;
                return tree;
            }
        }


        public static string LongToBinaryString(long value)
        {
            char[] binaryArray = new char[64];

            for (int i = 0; i < 64; i++)
                binaryArray[63 - i] = (value & (1L << i)) != 0 ? '1' : '0';
            
            return new string(binaryArray);
        }

        public static byte[] ConvertBitStringToBytes(string bitString)
        {
            int numOfBytes = (bitString.Length) / 8; // Calculate number of bytes needed
            byte[] bytes = new byte[numOfBytes];

            for (int i = 0; i < bitString.Length; i++)
            {
                int byteIndex = i / 8; // Find the byte index
                int bitIndex = i % 8;  // Find the bit index within the current byte

                if (bitString[i] == '1')
                {
                    bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));
                }
            }
            return bytes;
        }  
        

        public static void WriteFile(ref string file_path, ref Huffman obj, int n, int tap,ref string seed, bool alpha_numeric = false, bool with_encryption = false)
        {
            byte[] compressed_image = obj.Compress();

            int width = obj.image.GetLength(0);
            int height = obj.image.GetLength(1);

            using (BinaryWriter binWriter = new BinaryWriter(File.Open(file_path, FileMode.Create)))
            {
                binWriter.Write(with_encryption);
                binWriter.Write(width); // 4 byte 
                binWriter.Write(height);// 4 byte
                binWriter.Write(compressed_image.Length);
                binWriter.Write(compressed_image); // (N) bytes
                binWriter.Write(seed);// M bytes where M is the length of the seed
                binWriter.Write(tap); // 4 bytes  
                TreeSerializer.Serialize(ref obj.red_tree,binWriter); // (2 * #nodes) byte
                TreeSerializer.Serialize(ref obj.green_tree,binWriter);
                TreeSerializer.Serialize(ref obj.blue_tree,binWriter);
            }
        }


        public static RGBPixel[,] ReadFile(ref string file_path, bool alpha_numeric = false)
        {
            bool with_encryption;
            int width;
            int height;
            byte[] compressed_image;
            string seed;
            int tap;

            Huffman obj = new Huffman();

            using (var binReader = new BinaryReader(File.Open(file_path, FileMode.Open)))
            {

                with_encryption = binReader.ReadBoolean();
                width = binReader.ReadInt32();
                height = binReader.ReadInt32();
                int length = binReader.ReadInt32();
                compressed_image = binReader.ReadBytes(length);
                seed = binReader.ReadString();
                tap = binReader.ReadInt32();
                obj.red_tree = TreeSerializer.Deserialize(binReader);
                obj.green_tree = TreeSerializer.Deserialize(binReader);
                obj.blue_tree = TreeSerializer.Deserialize(binReader);
            }



            RGBPixel[,] decompressed_image = Huffman.Decompression(ref compressed_image, ref obj.red_tree, ref obj.green_tree, ref obj.blue_tree, width, height);
            if (with_encryption)
                decompressed_image = Encryption_and_decrepssion.EncryptImage(ref decompressed_image, seed.Length, tap, seed, alpha_numeric);
            return decompressed_image;
        }

    }
}

