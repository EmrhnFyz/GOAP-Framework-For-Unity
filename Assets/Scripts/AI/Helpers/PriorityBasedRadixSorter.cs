using System.Collections.Generic;

namespace AI.GOAP.Helpers
{
    public class PriorityBasedRadixSorter<T> where T : IPrioritizable
    {
        private void CountingSortByDigit(List<T> items, int[] priorities, int exp)
        {
            int n = items.Count;
            T[] output = new T[n];
            int[] count = new int[10];

            for (int i = 0; i < n; i++)
            {
                int index = priorities[i] / exp % 10;

                // Clamp index to [0,9]
                if (index < 0)
                {
                    index = 0;
                }
                else if (index > 9)
                {
                    index = 9;
                }

                count[index]++;
            }

            for (int i = 1; i < 10; i++)
            {
                count[i] += count[i - 1];
            }

            for (int i = n - 1; i >= 0; i--)
            {
                int index = priorities[i] / exp % 10;
                if (index < 0)
                {
                    index = 0;
                }
                else if (index > 9)
                {
                    index = 9;
                }

                output[count[index] - 1] = items[i];
                count[index]--;
            }

            for (int i = 0; i < n; i++)
            {
                items[i] = output[i];
            }
        }

        public void Sort(List<T> items, T recentItem, bool descending = true)
        {
            if (items.Count <= 1)
            {
                return;
            }

            int[] priorities = new int[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                float priority = items[i].Equals(recentItem) ? items[i].Priority - 0.01f : items[i].Priority;
                priorities[i] = (int)(priority * 100);
            }

            int maxPriority = priorities[0];
            for (int i = 1; i < priorities.Length; i++)
            {
                if (priorities[i] > maxPriority)
                {
                    maxPriority = priorities[i];
                }
            }

            for (int exp = 1; maxPriority / exp > 0; exp *= 10)
            {
                CountingSortByDigit(items, priorities, exp);
            }

            if (descending)
            {
                items.Reverse();
            }
        }
    }
}
