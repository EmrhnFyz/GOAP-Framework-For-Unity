using System.Collections.Generic;

namespace AI.GOAP.Helpers
{
    public class PriorityQueue<T>
    {
        private List<(T item, float priority)> _elements = new();

        public int Count => _elements.Count;
        public void Clear() => _elements.Clear();
        public void Enqueue(T item, float priority)
        {
            _elements.Add((item, priority));
            int childIndex = _elements.Count - 1;
            while (childIndex > 0)
            {
                int parentIndex = (childIndex - 1) / 2;
                if (_elements[parentIndex].priority <= _elements[childIndex].priority)
                {
                    break;
                }
                (_elements[parentIndex], _elements[childIndex]) = (_elements[childIndex], _elements[parentIndex]);
                childIndex = parentIndex;
            }
        }

        public T Dequeue()
        {
            var result = _elements[0].item;
            _elements[0] = _elements[_elements.Count - 1];
            _elements.RemoveAt(_elements.Count - 1);

            int parentIndex = 0;
            while (true)
            {
                int minIndex = parentIndex;
                int leftChild = 2 * parentIndex + 1;
                int rightChild = 2 * parentIndex + 2;

                if (leftChild < _elements.Count && _elements[leftChild].priority < _elements[minIndex].priority)
                {
                    minIndex = leftChild;
                }
                if (rightChild < _elements.Count && _elements[rightChild].priority < _elements[minIndex].priority)
                {
                    minIndex = rightChild;
                }
                if (minIndex == parentIndex)
                {
                    break;
                }

                (_elements[parentIndex], _elements[minIndex]) = (_elements[minIndex], _elements[parentIndex]);
                parentIndex = minIndex;
            }

            return result;
        }
    }

}
