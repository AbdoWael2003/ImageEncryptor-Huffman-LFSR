using ImageEncryptCompress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;




public class Node<T> : IComparable<Node<T>>
{
    
    public T data;
    public int frequency;
    public Node<T> left, right;


    public Node(){}
    public Node(T item)
    {
        this.data = item;
        this.left = this.right = null;
    }
    public Node(T item, int frequency)
    {
        data = item;
        left = right = null;
        this.frequency = frequency;
    }
    public int CompareTo(Node<T> other) => this.frequency.CompareTo(other.frequency);
}

public class Huffman
{

    string path;
    public RGBPixel[,] image;

    public Dictionary<byte, int> red_frequency;
    public Dictionary<byte, int> green_frequency;
    public Dictionary<byte, int> blue_frequency;

    // for output
    public Node<byte> red_tree;
    public Node<byte> green_tree;
    public Node<byte> blue_tree;

    public Dictionary<byte, string> red_code;
    public Dictionary<byte, string> green_code;
    public Dictionary<byte, string> blue_code;

    private void Init()
    {
        this.red_frequency = new Dictionary<byte, int>();
        this.green_frequency = new Dictionary<byte, int>();
        this.blue_frequency = new Dictionary<byte, int>();

        red_tree = new Node<byte>();
        green_tree = new Node<byte>();
        blue_tree = new Node<byte>();


        red_code = new Dictionary<byte, string>();
        green_code = new Dictionary<byte, string>();
        blue_code = new Dictionary<byte, string>();
    }

    public Huffman() { Init(); }
    public Huffman(ref RGBPixel[,] image)
    {
        this.image = image;
        Init();
    }
    
    public static RGBPixel[,] Decompression(ref byte[] binary, ref Node<byte> red_tree, ref Node<byte> green_tree, ref Node<byte> blue_tree, int width, int height)
    {
        RGBPixel[,] image = new RGBPixel[width, height];

        var trees = new Node<byte>[3]; trees[0] = red_tree; trees[1] = green_tree; trees[2] = blue_tree;

        long bit_index = 0;

        Node<byte> iterator;
        for(int tree = 0; tree < 3; tree++)
        {
            iterator = trees[tree];

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    while (true) // guaranteed that will not exceed 8;
                    {
                        if  ((binary[bit_index / 8] & (1 << (7 - (byte)(bit_index % 8)))) != 0)
                            iterator = iterator.right;
                        else
                            iterator = iterator.left;

                        bit_index++;

                        if (iterator.left == null && iterator.right == null)
                        {
                            if(tree == 0)
                                image[i, j].red = iterator.data;
                            else if(tree == 1)
                                image[i, j].green = iterator.data;
                            else
                                image[i, j].blue = iterator.data;

                            iterator = trees[tree];
                            break;
                        }

                    }
                }
        }
       
        return image;
    }

    // two approaches either brute force or D&C using multi-threading which is faster than brute force
    private void GetFrequency(int state = 0)
    { 
        if(state == 0)
        {
            for(int i = 0; i < image.GetLength(0); i++)
                for(int j = 0; j < image.GetLength(1); j++)
                {
                    if (red_frequency.ContainsKey(image[i, j].red))
                        red_frequency[image[i, j].red]++;
                    else
                        red_frequency.Add(image[i, j].red, 1);
                    
                    if (green_frequency.ContainsKey(image[i, j].green))
                        green_frequency[image[i, j].green]++;
                    else
                        green_frequency.Add(image[i, j].green, 1);

                    if (blue_frequency.ContainsKey(image[i, j].blue))
                        blue_frequency[image[i, j].blue]++;
                    else
                        blue_frequency.Add(image[i, j].blue, 1);
                }
        }
    }

    private Node<byte> ConstructHuffmanTree(ref Dictionary<byte,int> freq, string field_name)
    {
        PriorityQueue<Node<byte>> pq = new PriorityQueue<Node<byte>>(freq.Count, (x, y) => x.frequency.CompareTo(y.frequency) < 0);

        foreach (byte key in freq.Keys)
        {
            Node<byte> node = new Node<byte>();
            node.data = key;
            node.frequency = freq[key];
            pq.Add(node);
        }
        for(int i = 0; i < freq.Count - 1; i++)
        {
            Node<byte> node = new Node<byte>();
            node.left = pq.Peek(); pq.Pop();
            node.right = pq.Peek(); pq.Pop();
            node.frequency = node.left.frequency + node.right.frequency;
            pq.Add(node);
        }

        return pq.Peek();
    }

    private void AssignCodes(ref Dictionary<byte, string> table, ref Node<byte> root, string code = "")
    {
        if (root.left == null && root.right == null)
        {
            table[root.data] = code;
            return;
        }
        if (root.left != null)
            AssignCodes(ref table, ref root.left, code + "0");
        if (root.right != null)
            AssignCodes(ref table, ref root.right, code + "1");
    }

    public byte[] Compress()
    {
        GetFrequency();
        this.red_tree = ConstructHuffmanTree(ref this.red_frequency, "red");
        this.green_tree = ConstructHuffmanTree(ref this.green_frequency, "green");
        this.blue_tree = ConstructHuffmanTree(ref this.blue_frequency, "blue");

        AssignCodes(ref this.red_code, ref this.red_tree);
        AssignCodes(ref this.green_code, ref this.green_tree);
        AssignCodes(ref this.blue_code, ref this.blue_tree);

       
        long size = 0;
        
        for (int i = 0; i < image.GetLength(0); i++)
            for (int j = 0; j < image.GetLength(1); j++)
            {
                size += red_code[image[i, j].red].Length;
                size += green_code[image[i, j].green].Length;
                size += blue_code[image[i, j].blue].Length; 
            }

        byte[] binary = new byte[(int)(Math.Ceiling(size / 8f))];

        int bit_index = 0, byte_index = 0;

        for (int i = 0; i < image.GetLength(0); i++)
            for (int j = 0; j < image.GetLength(1); j++)
            {
                string red_string = red_code[image[i, j].red];
                for (int k = 0; k < red_string.Length; k++, bit_index++)
                {
                    if (bit_index == 8)
                    { 
                        byte_index++;
                        bit_index = 0;
                    }
                    if (red_string[k] == '1')
                    {
                        binary[byte_index] |= (byte)(1 << (7 - bit_index));
                    }
                }
            }

        for (int i = 0; i < image.GetLength(0); i++)
            for (int j = 0; j < image.GetLength(1); j++)
            {
                string green_string = green_code[image[i, j].green];
                for (int k = 0; k < green_string.Length; k++, bit_index++)
                {
                    if (bit_index == 8)
                    { 
                        byte_index++;
                        bit_index = 0;
                    }
                    if (green_string[k] == '1')
                    {
                        binary[byte_index] |= (byte)(1 << (7 - bit_index));
                    }
                }
            }

        for (int i = 0; i < image.GetLength(0); i++)
            for (int j = 0; j < image.GetLength(1); j++)
            {
                string blue_string = blue_code[image[i, j].blue];
                for (int k = 0; k < blue_string.Length; k++, bit_index++)
                {
                    if (bit_index == 8)
                    { 
                        byte_index++;
                        bit_index = 0;
                    }
                    if (blue_string[k] == '1')
                    {
                        binary[byte_index] |= (byte)(1 << (7 - bit_index));
                    }
                }
            }



        return binary;
    }
}