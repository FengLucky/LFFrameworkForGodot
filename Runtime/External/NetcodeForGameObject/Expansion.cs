using System;
using System.Collections.Generic;

namespace LF.Runtime
{
    public static class Expansion
    {
        public static NetworkDictionary<TKey, TValue> ToNetwork<TKey,TValue>(this IDictionary<TKey,TValue> dictionary)
            where TKey:unmanaged,IEquatable<TKey> where TValue:unmanaged
        {
            var networkDictionary = new NetworkDictionary<TKey, TValue>(dictionary);
            return networkDictionary;
        }
        
        public static NetworkList<TValue> ToNetwork<TValue>(this IList<TValue> list) where TValue:unmanaged,IEquatable<TValue>
        {
            var networkList = new NetworkList<TValue>(list);
            return networkList;
        }
        
        public static NetworkHashSet<TValue> ToNetwork<TValue>(this ISet<TValue> hashSet) where TValue:unmanaged,IEquatable<TValue>
        {
            var networkHashSet = new NetworkHashSet<TValue>(hashSet);
            return networkHashSet;
        }
        
        public static NetworkDictionary<TKey, TValue> ToNetwork<TKey,TValue>(this IReadOnlyDictionary<TKey,TValue> dictionary)
            where TKey:unmanaged,IEquatable<TKey> where TValue:unmanaged
        {
            var networkDictionary = new NetworkDictionary<TKey, TValue>(dictionary);
            return networkDictionary;
        }
        
        public static NetworkList<TValue> ToNetwork<TValue>(this IReadOnlyList<TValue> list) where TValue:unmanaged,IEquatable<TValue>
        {
            var networkList = new NetworkList<TValue>(list);
            return networkList;
        }
    }
}