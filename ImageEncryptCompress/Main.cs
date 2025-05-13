using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ImageEncryptCompress
{
    internal class Main
    {
        public static RGBPixel[,] Encrypt_and_compress(string file_name, RGBPixel[,] image, int n,int tap,string seed, bool alpha_numeric, bool with_encryption)
        {
            if (with_encryption)
                image = Encryption_and_decrepssion.EncryptImage(ref image, n, tap, seed, alpha_numeric);
            Huffman compressor = new Huffman(ref image);
            ToBinary.WriteFile(ref file_name, ref compressor, n, tap,ref seed, alpha_numeric, with_encryption);
            return image;
        }
        public static RGBPixel[,] Decrypt_and_Decompress(string file_path, bool alpha_numeric = false) => ToBinary.ReadFile(ref file_path, alpha_numeric);
    }
}
