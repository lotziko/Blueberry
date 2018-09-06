using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore
{
    public class DelayedRemovalList<T> 
    {
        private int iterating;
        private List<int> remove = new List<int>(0);
        private int clear;
        private List<T> items;
        public int Count => items.Count;

        public DelayedRemovalList()
        {
            items = new List<T>();
        }

        public DelayedRemovalList(List<T> list)
        {
            items = new List<T>(list);
        }

        public DelayedRemovalList(int capacity)
        {
            items = new List<T>(capacity);
        }

        public T this[int index]
        {
            get
            {
                return items[index];
            }
            set
            {
                items[index] = value;
            }
        }

        public List<T> Items()
        {
            return items;
        }

        public void Begin()
        {
            iterating++;
        }

        public void End()
        {
            if (iterating == 0) throw new Exception("begin must be called before end.");
            iterating--;
            if (iterating == 0)
            {
                if (clear > 0 && clear == Count)
                {
                    remove.Clear();
                    items.Clear();
                }
                else
                {
                    for (int i = 0, n = remove.Count; i < n; i++)
                    {
                        int index = remove.Pop();
                        if (index >= clear) items.RemoveAt(index);
                    }
                    for (int i = clear - 1; i >= 0; i--)
                        items.RemoveAt(i);
                }
                clear = 0;
            }
        }

        private void Remove(int index)
        {
            if (index < clear) return;
            for (int i = 0, n = remove.Count; i < n; i++)
            {
                int removeIndex = remove[i];
                if (index == removeIndex) return;
                if (index < removeIndex)
                {
                    remove.Insert(i, index);
                    return;
                }
            }
            remove.Add(index);
        }

        public bool Remove(T value)
        {
            if (iterating > 0)
            {
                int index = items.IndexOf(value);
                if (index == -1) return false;
                Remove(index);
                return true;
            }
            return items.Remove(value);
        }

        public T RemoveIndex(int index)
        {
            if (iterating > 0)
            {
                Remove(index);
                return items.ElementAtOrDefault(index);
            }
            T item = items.ElementAtOrDefault(index);
            items.RemoveAt(index);
            return item;
        }

        public void RemoveRange(int start, int end)
        {
            if (iterating > 0)
            {
                for (int i = end; i >= start; i--)
                    Remove(i);
            }
            else
                items.RemoveRange(start, end);
        }

        public void Clear()
        {
            if (iterating > 0)
            {
                clear = Count;
                return;
            }
            items.Clear();
        }

        public void Set(int index, T value)
        {
            if (iterating > 0) throw new Exception("Invalid between begin/end.");
            items[index] = value;
        }

        public void Insert(int index, T value)
        {
            if (iterating > 0) throw new Exception("Invalid between begin/end.");
            items.Insert(index, value);
        }
        
        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public void Add(T item)
        {
            items.Add(item);
        }

        /*public void Swap(int first, int second)
        {
            if (iterating > 0) throw new Exception("Invalid between begin/end.");
            items.Swap(first, second);
        }*/

        public T Pop()
        {
            if (iterating > 0) throw new Exception("Invalid between begin/end.");
            return items.Pop();
        }

        public void Sort()
        {
            if (iterating > 0) throw new Exception("Invalid between begin/end.");
            items.Sort();
        }
    }
}
