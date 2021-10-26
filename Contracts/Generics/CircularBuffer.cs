using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Generics
{
    public class CircularBuffer<T>
    {
        private const int DEFAULT_BUFFER_SIZE = 5000;

        public int Limit { get; set; }
        private Queue<T> _items { get; set; }

        public CircularBuffer(int limit = DEFAULT_BUFFER_SIZE)
        {
            _items = new Queue<T>();
            Limit = limit;
        }

        public void Add(T item)
        {
            while (_items.Count >= this.Limit)
                _items.Dequeue();
            _items.Enqueue(item);
        }
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                return;

            foreach (var item in items)
                Add(item);
        }

        public bool TryPeekLast(out T item)
        {
            bool success = false;
            item = default(T);
            try
            {
                item = _items.ToList()[_items.Count - 1];
                success = true;
            }
            catch { success = false; }

            return success;
        }
        public bool TryPeekFirst(out T item) => _items.TryPeek(out item);

        public List<T> ToList() => _items.ToList();
        public void Clear() => _items.Clear();
        public int Count => _items.Count;
    }
}
