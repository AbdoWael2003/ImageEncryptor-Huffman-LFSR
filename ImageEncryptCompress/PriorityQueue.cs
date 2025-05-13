using System;


public class PriorityQueue<T> where T : IComparable<T>
{
    //private byte state = 0;
    private int capacity = 0;
    private T[] arr;
    private int last_position = 0;
    private Func<T, T, bool> in_this_order;

    private static bool Compare<T>(T parent, T child) where T : IComparable<T>  // Ɵ(1)
    {
        if (child.CompareTo(parent) >= 0)
            return true;
        return false;
    }

    public PriorityQueue()
    {
        ///<summary>
        /// default constructor creats  priority queue
        ///<summary>
        capacity = 100;
        arr = new T[capacity];
        in_this_order = PriorityQueue<T>.Compare<T>;
    }

    public PriorityQueue(int n)
    {
        ///<summary>
        /// overloaded constructor to specify the size of the priority queue
        ///<summary>
        /// <param name="n"> which is the Capacity of the priority queue </param>
        ///<summary>

        capacity = n;
        arr = new T[n];
        in_this_order = PriorityQueue<T>.Compare<T>;
    }

    public PriorityQueue(int n, Func<T, T, bool> fn)
    {
        ///<summary>
        /// overloaded constructor to specify the size and the priority judgment
        ///<summary>
        /// <param name="n"> which is the Capacity of the priority queue </param>
        /// <param name="fn">which is the function<T,T> ==> bool the judge function.</param>
        /// <remarks>
        /// note the fn must take two elements and it returns true if the first parameter should have priority over the second one and false otherwise
        /// </remarks>

        capacity = n;
        arr = new T[n];
        in_this_order = fn;
    }

    public int Size() => last_position; // Ɵ(1)

    public T Peek() => arr[0]; // Ɵ(1)

    public void Clear() { int t = last_position; while (t-- > 0) { Pop(); } } // Ɵ(1)

    private void Expand() // Ɵ(n) it's guaranteed that the number of calls is a small constant
    {
        T[] new_arr = new T[capacity *= 2];
        for (int i = 0; i < last_position; i++)
            new_arr[i] = arr[i];
        arr = new_arr;
    }

    public void Pop() // Ɵ(log(n))
    {
        if (last_position == 0) return;

        arr[0] = arr[--last_position];

        int current_position = 0;
        int left_child_position = current_position * 2 + 1;

        int candidate_parent_position = left_child_position;

        if (left_child_position + 1 < last_position && in_this_order(arr[left_child_position + 1], arr[left_child_position]))
            candidate_parent_position = left_child_position + 1;

        while (candidate_parent_position < last_position && in_this_order(arr[candidate_parent_position], arr[current_position]))
        {
            (arr[current_position], arr[candidate_parent_position]) = (arr[candidate_parent_position], arr[current_position]);

            current_position = candidate_parent_position;

            left_child_position = current_position * 2 + 1;

            candidate_parent_position = left_child_position;
            if (left_child_position + 1 < last_position && in_this_order(arr[left_child_position + 1], arr[left_child_position]))
                candidate_parent_position = left_child_position + 1;
        }


    }

    public void Add(T element) // Ɵ(log(n)) 
    {
        // check to expand

        if (last_position == capacity)
            Expand();

        arr[last_position] = element;

        int current_index = last_position;
        int parent_index = (current_index - 1) / 2;

        while (parent_index >= 0 && in_this_order(arr[current_index], arr[parent_index])) // Ɵ(log(n))
        {
            if (current_index == parent_index) break;

            // swap
            (arr[current_index], arr[parent_index]) = (arr[parent_index], arr[current_index]);

            current_index = parent_index;
            parent_index = (current_index - 1) / 2;
        }

        last_position++;
    }
};





// for test only
//class RunerOfCode
//{

//    static void Main()
//    {
//        //PriorityQueue<int> p = new PriorityQueue<int>();

//        //p.Add(30000);
//        //p.Add(8);
//        //p.Add(10);
//        //p.Add(330);
//        //p.Add(19);
//        //p.Add(400);
//        //p.Add(2);
//        //p.Add(80);
//        //p.Add(16);
//        //p.Add(15);
//        //p.Add(1900);

//        //while(p.Size() > 0)
//        //{
//        //    Console.WriteLine(p.Peek());
//        //    p.Pop();   
//        //}



//        //BinaryTree<int> tree = new BinaryTree<int>();

//        //// Insert some nodes
//        //tree.Insert(50);
//        //tree.Insert(30);
//        //tree.Insert(20);
//        //tree.Insert(40);
//        //tree.Insert(70);
//        //tree.Insert(60);
//        //tree.Insert(80);

//        //// Print inorder traversal
//        //Console.WriteLine("Inorder traversal:");
//        //tree.Inorder();
//        //Console.WriteLine();

//        //// Search for a node
//        //int key = 40;
//        //Node<int> result = tree.Search(key);
//        //if (result != null)
//        //    Console.WriteLine("Node with key " + key + " found in the tree.");
//        //else
//        //    Console.WriteLine("Node with key " + key + " not found in the tree.");

//    }
//}

