using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omron
{
    public class MaxHeap<T>
    {
        int lchild(int parent) { return 2 * parent + 1; }
        int rchild(int parent) { return 2 * parent + 2; }
        int parent(int child) { return (int)((child - 1) / 2); }

        List<KeyValuePair<float, T>> heap;

        public bool HasItems
        {
            get { return heap.Count > 0; }
        }

        public MaxHeap()
        {
            heap = new List<KeyValuePair<float, T>>(64);
        }
        public void Push(T item, float key)
        {
            var kvpItem = new KeyValuePair<float, T>(key, item);

            heap.Add(kvpItem);
            int i = heap.Count - 1;
            while (i > 0 && kvpItem.Key > heap[parent(i)].Key)
            {
                heap[i] = heap[parent(i)];
                i = parent(i);
            }
            heap[i] = kvpItem;
        }
        public T Peek()
        {
            return heap[0].Value;
        }
        public T Pop()
        {
            if (heap.Count == 0) throw new Exception("no more items");

            var max = heap[0].Value;

            //
            //restructure tree
            //
            heap[0] = heap.Last(); //overwrite root with last node
            var toInsert = heap[0];
            heap.RemoveAt(heap.Count - 1); //remove last node

            if (heap.Count == 0) return max; //exit if tree is empty

            int i = 0;
            while (lchild(i) < heap.Count)
            {
                int largestChild = lchild(i);
                if (rchild(i) < heap.Count && heap[rchild(i)].Key > heap[lchild(i)].Key)
                    largestChild = rchild(i);

                if (toInsert.Key < heap[largestChild].Key)
                {
                    heap[i] = heap[largestChild];
                    i = largestChild;
                }
                else
                {
                    break;
                }

            }
            heap[i] = toInsert;

            return max;
        }
    }
}
