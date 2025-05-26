# ImageEncryptor-Huffman-LFSR

A C++ project that encrypts grayscale images using **Huffman Encoding** and **Linear Feedback Shift Register (LFSR)** techniques. This project demonstrates basic concepts of lossless data compression and stream cipher encryption applied to images.

## 📌 Features

- ✅ Read and process grayscale BMP images.
- ✅ Implement Huffman encoding for image compression.
- ✅ Use LFSR for stream cipher encryption and decryption.
- ✅ Save and visualize the encrypted and decrypted image data.
- ✅ Simple GUI using OpenCV to display original and processed images.

## 🧠 Concepts

### Huffman Encoding
Huffman coding is a lossless compression algorithm that assigns shorter codes to frequently occurring pixel values in an image. This project builds a Huffman tree and encodes image data accordingly.

### Linear Feedback Shift Register (LFSR)
LFSR is used to generate a pseudorandom binary sequence that serves as a stream cipher. This stream is XORed with the original (or compressed) image data for encryption.



