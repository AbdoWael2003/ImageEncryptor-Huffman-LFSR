using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace ImageEncryptCompress
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void btnEncoding_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();



            RGBPixel[,] encoded_image = Main.Encrypt_and_compress("out2", ImageMatrix, (int)numericUpDown1.Value, int.Parse(textBox2.Text.ToString()), textBox1.Text.ToString(), this.checkBox1.Checked, this.checkBox2.Checked);
            ImageOperations.DisplayImage(encoded_image, this.pictureBox2);
            stopwatch.Stop();

            // save as bmp
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.pictureBox2.Image.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
            }
            
            TimeSpan encodingTime = stopwatch.Elapsed;
            MessageBox.Show($"Encoding time: {encodingTime.TotalSeconds} seconds");
        }

        private void btnDecoding_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog.FileName;
                stopwatch.Start();

                RGBPixel[,] decoded_image = Main.Decrypt_and_Decompress(OpenedFilePath, checkBox1.Checked);
                ImageOperations.DisplayImage(decoded_image, pictureBox1);
                stopwatch.Stop();

                // save as bmp
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                saveFileDialog1.RestoreDirectory = true;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.pictureBox1.Image.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
                }
            }
            TimeSpan encodingTime = stopwatch.Elapsed;
            MessageBox.Show($"Encoding time: {encodingTime.TotalSeconds} seconds");
        }

        private void btnTryDecrypt_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            (long seed, int tap, RGBPixel[,] decryptedImage) = Encryption_and_decrepssion.Break_Password(ImageMatrix, (int)numericUpDown1.Value);
            this.textBox1.Text = ToBinary.LongToBinaryString(seed);
            this.textBox2.Text = tap.ToString();
            ImageOperations.DisplayImage(decryptedImage, this.pictureBox2);

            stopwatch.Stop();
            TimeSpan encodingTime = stopwatch.Elapsed;
            MessageBox.Show($"Encoding time: {encodingTime.TotalSeconds} seconds");

        }
    }
} 