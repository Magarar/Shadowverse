using System.Collections.Generic;

namespace Unit
{
    /// <summary>
    /// A pool reuses Memory for objects that are constantly created/destroyed, to prevent always allocating more memory
    /// Makes the AI much much much faster
    /// </summary>
    public class Pool<T> where T : new()
    {
        private HashSet<T> inUse = new HashSet<T>();
        private Stack<T> available = new Stack<T>();

        public T Create()
        {
            if (available.Count > 0)
            {
                T elem = available.Pop();
                inUse.Add(elem);
                return elem;
            }
            T newObj = new T();
            inUse.Add(newObj);
            return newObj;
        }
        
        public void Dispose(T elem)
        {
            inUse.Remove(elem);
            available.Push(elem);
        }

        public void DisposeAll()
        {
            foreach (T elem in inUse)
                available.Push(elem);
            inUse.Clear();
        }

        public void Clear()
        {
            inUse.Clear();
            available.Clear();
        }
        
        public HashSet<T> GetAllActive()
        {
            return inUse;
        }
        
        public int Count => inUse.Count;
        public int CountAvailable => available.Count;
        public int CountCapacity => Count + CountAvailable;
    }
}