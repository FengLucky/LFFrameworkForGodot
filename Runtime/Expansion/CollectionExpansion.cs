using System;
using System.Collections.Generic;

namespace LF.Runtime
{
    public static class CollectionExpansion
    {
        public static bool IsNullOrEmpty<T>(this IList<T> c)
        {
            return c == null || c.Count == 0;
        }
        
        public static bool IsNotNullOrEmpty<T>(this IList<T> c)
        {
            return !IsNullOrEmpty(c);
        }

        public static T TryGet<T>(this IList<T> list, int index, T defaultValue = default)
        {
            if (list.IsNullOrEmpty())
            {
                return default;
            }
            return index >= 0 && index < list.Count ? list[index] : defaultValue;
        }
        
        public static T CycleGet<T>(this IList<T> list, int index)
        {
            if (list.IsNullOrEmpty())
            {
                return default;
            }
            return list[index % list.Count];
        }

        public static T RandomOne<T>(this IList<T> list)
        {
            return list[Random.Shared.Next(0, list.Count)];
        }

        public static T RandomOneWithWeight<T>(this IList<T> list, List<int> weights)
        {
            var totalWeight = 0;
            foreach (var weight in weights)
            {
                totalWeight += weight;
            }

            return RandomOneWithWeight(list, weights, totalWeight);
        }

        public static T RandomOneWithWeight<T>(this IList<T> list, List<int> weights,int totalWeight)
        {
            var random = Random.Shared.Next(0, totalWeight + 1);
            for (var i = 0; i < weights.Count; i++)
            {
                if (random <= weights[i])
                {
                    return list[i];
                }
                random -= weights[i];
            }
            
            return list.IsNullOrEmpty() ? default : list[^1];
        }

        public static T AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
            return default;
        }
    }
}