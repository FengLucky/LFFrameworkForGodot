using System;
using System.Collections.Generic;

namespace LF.Runtime
{
    [Serializable]
    public struct KeyValueNgc<K,V>:IEquatable<KeyValueNgc<K,V>>
    {
        public K Key;
        public V Value;
        
        public KeyValueNgc(K key, V value)
        {
            Key = key;
            Value = value;
        }

        public bool Equals(KeyValueNgc<K, V> other)
        {
            return EqualityComparer<K>.Default.Equals(Key, other.Key) && EqualityComparer<V>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is KeyValueNgc<K, V> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value);
        }
        
        public static bool operator ==(KeyValueNgc<K, V> left, KeyValueNgc<K, V> right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(KeyValueNgc<K, V> left, KeyValueNgc<K, V> right)
        {
            return !left.Equals(right);
        }
    }
}