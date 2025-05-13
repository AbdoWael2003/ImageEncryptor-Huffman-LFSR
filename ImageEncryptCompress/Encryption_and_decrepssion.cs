using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageEncryptCompress
{
    internal class Encryption_and_decrepssion
    {
        public class LFSR
        {
            private int N;        // number of bits
            private int tap;      // tap position
            private string seed_string;
            private long seed;         // initial seed
            private bool alpha_numeric;
            public LFSR(int n, int t, string seed_string, bool alpha_numeric)
            {
                N = n;
                tap = t;
                if (alpha_numeric)
                    seed = Handle_AlphaNumeric_Password(seed_string);
                else
                    seed = Convert.ToInt64(seed_string, 2);
            }

            public byte Step()
            {
                byte tap_position_bit = 0, most_significant_bit = 0; // 1 bit
                if (((1 << tap) & seed) != 0)
                    tap_position_bit = 1;

                if (((1 << (N - 1)) & seed) != 0)
                    most_significant_bit = 1;


                byte new_bit = (byte)(tap_position_bit ^ most_significant_bit);
                update_seed(new_bit);
                return new_bit;
            }
            private void update_seed(byte new_bit)
            {
                seed = ((seed << 1) | new_bit) & ((1 << N) - 1);
            }

            public int GenerateBits(int k)
            {
                int result = 0;
                for (int i = 0; i < k; i++)
                {
                    result <<= 1;
                    result |= Step(); // append the new bit to the result
                }
                return result;
            }

            public long GetSeed()
            {
                return seed;  // return the current seed
            }
        }


        public static long Handle_AlphaNumeric_Password(string theString)
        {
            long convertedSeed = 0;

            foreach (char c in theString)
                convertedSeed = (convertedSeed << 8) | (long)c;
            
            return convertedSeed;
        }

        public static string FindEncryptionSeed(RGBPixel[,] originalImage, RGBPixel[,] encryptedImage, int n, int tap)
        {
            int width = originalImage.GetLength(0);
            int height = originalImage.GetLength(1);

            for (long candidateSeedint = 0; candidateSeedint < (1 << n); candidateSeedint++)
            {
                LFSR password = new LFSR(n, tap, candidateSeedint.ToString(), false);
                bool seedFound = true;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        RGBPixel originalPixel = originalImage[x, y];
                        int redBits = password.GenerateBits(8);
                        int greenBits = password.GenerateBits(8);
                        int blueBits = password.GenerateBits(8);

                        RGBPixel encryptedPixel = encryptedImage[x, y];
                        encryptedPixel.red = (byte)(originalPixel.red ^ redBits);
                        encryptedPixel.green = (byte)(originalPixel.green ^ greenBits);
                        encryptedPixel.blue = (byte)(originalPixel.blue ^ blueBits);

                        if (encryptedPixel.red != originalPixel.red ||
                            encryptedPixel.green != originalPixel.green ||
                            encryptedPixel.blue != originalPixel.blue)
                        {
                            seedFound = false;
                            break;
                        }
                    }

                    if (!seedFound)
                        break;
                }

                if (seedFound)
                    return candidateSeedint.ToString();
            }

            return "Error in finding the seed";
        }
        public static RGBPixel[,] EncryptImage(ref RGBPixel[,] originalImage, int n, int tap, string seed, bool alpha_numeric)
        {
            int width = originalImage.GetLength(0);
            int height = originalImage.GetLength(1);
            RGBPixel[,] encryptedImage = new RGBPixel[width, height];

            LFSR password = new LFSR(n, tap, seed, alpha_numeric);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    RGBPixel originalPixel = originalImage[x, y];
                    int redBits = password.GenerateBits(8);
                    int greenBits = password.GenerateBits(8);
                    int blueBits = password.GenerateBits(8);

                    RGBPixel encryptedPixel = new RGBPixel
                    {
                        red = (byte)(originalPixel.red ^ redBits),
                        green = (byte)(originalPixel.green ^ greenBits),
                        blue = (byte)(originalPixel.blue ^ blueBits)
                    };

                    encryptedImage[x, y] = encryptedPixel;
                }
            }

           

            return encryptedImage;
        }

        public static (long seed, int tap, RGBPixel[,] decryptedImage) Break_Password(RGBPixel[,] encryptedImage, int n)
        {
            int width = encryptedImage.GetLength(0);
            int height = encryptedImage.GetLength(1);
            double maxStdDeviation = 0;
            long bestSeed = -1;
            int bestTap = -1;

            long maxSeed = (1 << n);
            int maxTap = n;
            object lockObj = new object(); // For thread safety when updating bestSeed and bestTap

            Parallel.For(0, maxTap, tap =>
            {
                Parallel.For(0, maxSeed, candidateSeedint =>
                {
                    string candidateSeed = Convert.ToString(candidateSeedint, 2).PadLeft(n, '0');
                    LFSR password = new LFSR(n, (int)tap, candidateSeed, false);

                    int[] frequency = new int[256];

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            int redBits = password.GenerateBits(8);
                            int greenBits = password.GenerateBits(8);
                            int blueBits = password.GenerateBits(8);

                            RGBPixel decryptedPixel = new RGBPixel
                            {
                                red = (byte)(encryptedImage[x, y].red ^ redBits),
                                green = (byte)(encryptedImage[x, y].green ^ greenBits),
                                blue = (byte)(encryptedImage[x, y].blue ^ blueBits)
                            };

                            frequency[decryptedPixel.red]++;
                            frequency[decryptedPixel.green]++;
                            frequency[decryptedPixel.blue]++;
                        }
                    }

                    int totalPixels = width * height * 3;
                    double mean = totalPixels / 256.0;
                    double sumOfSquaredDifferences = 0;

                    for (int i = 0; i < 256; i++)
                    {
                        sumOfSquaredDifferences += (frequency[i] - mean) * (frequency[i] - mean);
                    }

                    double variance = sumOfSquaredDifferences / 256;
                    double stdDeviation = Math.Sqrt(variance);

                    lock (lockObj)
                    {
                        if (stdDeviation > maxStdDeviation)
                        {
                            maxStdDeviation = stdDeviation;
                            bestSeed = candidateSeedint;
                            bestTap = tap;
                        }
                    }
                });
            });

            string bestSeedString = Convert.ToString(bestSeed, 2);
            RGBPixel[,] decryptedImage = EncryptImage(ref encryptedImage, n, bestTap, bestSeedString, false);

            return (bestSeed, bestTap, decryptedImage);
        }
    }

}
