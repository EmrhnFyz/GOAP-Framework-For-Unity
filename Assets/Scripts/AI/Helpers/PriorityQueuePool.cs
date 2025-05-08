using System.Collections.Generic;

namespace AI.GOAP.Helpers
{
    public static class PriorityQueuePool<T>
    {
        private static readonly Stack<PriorityQueue<T>> Pool = new();

        public static PriorityQueue<T> Get()
        {
            if (Pool.Count > 0)
            {
                var queue = Pool.Pop();
                queue.Clear();
                return queue;
            }
            return new PriorityQueue<T>();
        }

        public static void Return(PriorityQueue<T> queue)
        {
            queue.Clear();
            Pool.Push(queue);
        }
    }
}