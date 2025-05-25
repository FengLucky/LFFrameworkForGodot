using System;
using Unity.Netcode;
using UnityEngine;

namespace LF.Runtime
{
    [Serializable]
    public struct SerializableGuid : IComparable, IComparable<SerializableGuid>, IEquatable<SerializableGuid>
#if NETCODE_EXPANSION
    ,INetworkSerializable
#endif
    {
        public static SerializableGuid None => default;
        
        [SerializeField] [HideInInspector] private uint m_Value0;
        [SerializeField] [HideInInspector] private uint m_Value1;
        [SerializeField] [HideInInspector] private uint m_Value2;
        [SerializeField] [HideInInspector] private uint m_Value3;

        public uint Value0 => m_Value0;
        public uint Value1 => m_Value1;
        public uint Value2 => m_Value2;
        public uint Value3 => m_Value3;

        public static bool operator ==(SerializableGuid x, SerializableGuid y) => (int) x.m_Value0 == (int) y.m_Value0 &&
                                                                  (int) x.m_Value1 == (int) y.m_Value1 &&
                                                                  (int) x.m_Value2 == (int) y.m_Value2 &&
                                                                  (int) x.m_Value3 == (int) y.m_Value3;

        public static bool operator !=(SerializableGuid x, SerializableGuid y) => !(x == y);

        public static bool operator <(SerializableGuid x, SerializableGuid y)
        {
            if ((int) x.m_Value0 != (int) y.m_Value0)
                return x.m_Value0 < y.m_Value0;
            if ((int) x.m_Value1 != (int) y.m_Value1)
                return x.m_Value1 < y.m_Value1;
            return (int) x.m_Value2 != (int) y.m_Value2 ? x.m_Value2 < y.m_Value2 : x.m_Value3 < y.m_Value3;
        }

        public static bool operator >(SerializableGuid x, SerializableGuid y) => !(x < y) && !(x == y);

        public override bool Equals(object obj) => obj != null && obj is SerializableGuid && this.Equals((SerializableGuid) obj);

        public bool Equals(SerializableGuid obj) => this == obj;

        public override int GetHashCode() =>
            (((int) this.m_Value0 * 397 ^ (int) this.m_Value1) * 397 ^ (int) this.m_Value2) * 397 ^ (int) this.m_Value3;

        public int CompareTo(object obj) => obj == null ? 1 : this.CompareTo((SerializableGuid) obj);

        public int CompareTo(SerializableGuid rhs)
        {
            if (this < rhs)
                return -1;
            return this > rhs ? 1 : 0;
        }

        public bool Empty() => this.m_Value0 == 0U && this.m_Value1 == 0U && this.m_Value2 == 0U && this.m_Value3 == 0U;

        public static bool TryParse(string configId, out SerializableGuid result)
        {
            result = default;

            var split = configId.Split('-');

            if (split.Length != 4)
            {
                return false;
            }

            result.m_Value0 = uint.Parse(split[0]);
            result.m_Value1 = uint.Parse(split[1]);
            result.m_Value2 = uint.Parse(split[2]);
            result.m_Value3 = uint.Parse(split[3]);
            return true;
        }

        public static SerializableGuid GenerateDefinite(uint v0, uint v1, uint v2, uint v3)
        {
            SerializableGuid result = default;
            result.m_Value0 = v0;
            result.m_Value1 = v1;
            result.m_Value2 = v2;
            result.m_Value3 = v3;
            return result;
        }

        public static SerializableGuid Generate()
        {
            SerializableGuid res = default;
            var guid = Guid.NewGuid();
            var byteArray = guid.ToByteArray();

            res.m_Value0 = BitConverter.ToUInt32(byteArray, 0);
            res.m_Value1 = BitConverter.ToUInt32(byteArray, 4);
            res.m_Value2 = BitConverter.ToUInt32(byteArray, 8);
            res.m_Value3 = BitConverter.ToUInt32(byteArray, 12);
            return res;
        }

        public override string ToString()
        {
            return $"{m_Value0}-{m_Value1}-{m_Value2}-{m_Value3}";
        }

#if NETCODE_EXPANSION
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_Value0);
            serializer.SerializeValue(ref m_Value1);
            serializer.SerializeValue(ref m_Value2);
            serializer.SerializeValue(ref m_Value3);
        }
#endif
    }
}