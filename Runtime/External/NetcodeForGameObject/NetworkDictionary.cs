#if NETCODE_EXPANSION
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace LF.Runtime
{
    [Serializable]
    [GenerateSerializationForGenericParameter(0)]
    [GenerateSerializationForGenericParameter(1)]
    public class NetworkDictionary<TKey,TValue>:NetworkVariableBase,INetworkSerializable,IDictionary<TKey,TValue>,IReadOnlyDictionary<TKey,TValue> where TKey:unmanaged,IEquatable<TKey>
    {
        private enum Function
        {
            Add,
            AddRange,
            Modify,
            Remove,
            Clear,
            Full
        }
        
        private struct FunctionOperation
        {
            public Function Function;
            public KeyValuePair<TKey,TValue> Item;
            public FunctionOperation(Function function, KeyValuePair<TKey,TValue> item)
            {
                Function = function;
                Item = item;
            }
        }
        
        public event Action OnValueChanged;
        public event Action<List<ChangeDetail<KeyValuePair<TKey,TValue>>>> OnValueChangedWithDetails;
        
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                if (!CheckPermission())
                {
                    return;
                }

                var oldValue = _dictionary[key];
                if (oldValue.Equals(value))
                {
                    return;
                }
                _dictionary[key] = value;
                var details = ListPool<ChangeDetail<KeyValuePair<TKey,TValue>>>.Get();
                details.Add(ChangeDetail<KeyValuePair<TKey,TValue>>.CreateModify(
                    new KeyValuePair<TKey, TValue>(key,oldValue),
                    new KeyValuePair<TKey, TValue>(key,value)));
                _functionOperations.Add(new (Function.Modify, new KeyValuePair<TKey, TValue>(key,value)));
                SetDirty(true);
                OnValueChanged?.Invoke();
                OnValueChangedWithDetails?.Invoke(details);
                ListPool<ChangeDetail<KeyValuePair<TKey,TValue>>>.Release(details);
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;

        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;
        
        private readonly Dictionary<TKey, TValue> _dictionary = new();
        private readonly List<FunctionOperation> _functionOperations = new();
        
        public NetworkDictionary() { }
        
        internal NetworkDictionary(IEnumerable<KeyValuePair<TKey,TValue>> items)
        {
            foreach (var item in items)
            {
                _dictionary.TryAdd(item.Key, item.Value);
            }
        }

        public NetworkDictionary(NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm) : base(readPerm, writePerm)
        {
            
        }
        
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        { 
            Add(item.Key,item.Value);
        }
        
        public void Add(TKey key, TValue value)
        {
            if (!CheckPermission())
            {
                return;
            }
            
            _dictionary.Add(key,value);
            var item = new KeyValuePair<TKey, TValue>(key, value);
            _functionOperations.Add(new (Function.Add, item));
            var details = ListPool<ChangeDetail<KeyValuePair<TKey,TValue>>>.Get();
            details.Add(ChangeDetail<KeyValuePair<TKey,TValue>>.CreateAdd(item));
            SetDirty(true);
            OnValueChanged?.Invoke();
            OnValueChangedWithDetails?.Invoke(details);
            ListPool<ChangeDetail<KeyValuePair<TKey,TValue>>>.Release(details);
        }
        
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            if (!CheckPermission())
            {
                return;
            }

            var details = ListPool<ChangeDetail<KeyValuePair<TKey,TValue>>>.Get();
            foreach (var item in items)
            {
                _dictionary.Add(item.Key,item.Value);
                _functionOperations.Add(new (Function.AddRange, item));
                details.Add(ChangeDetail<KeyValuePair<TKey,TValue>>.CreateAdd(item));
            }
            SetDirty(true);
            OnValueChanged?.Invoke();
            OnValueChangedWithDetails?.Invoke(details);
            ListPool<ChangeDetail<KeyValuePair<TKey,TValue>>>.Release(details);
        }
        
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }
        
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }
        
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public TValue GetValueOrDefault(TKey key)
        {
            return _dictionary.GetValueOrDefault(key);
        }
        
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }
        
        public bool Remove(TKey key)
        {
            if (!CheckPermission())
            {
                return false;
            }

            if (_dictionary.ContainsKey(key))
            {
                var item = new KeyValuePair<TKey, TValue>(key, _dictionary[key]);
                _dictionary.Remove(key);
                _functionOperations.Add(new (Function.Remove,item));
                var details = ListPool<ChangeDetail<KeyValuePair<TKey,TValue>>>.Get();
                details.Add(ChangeDetail<KeyValuePair<TKey,TValue>>.CreateRemove(item));
                SetDirty(true);
                OnValueChanged?.Invoke();
                OnValueChangedWithDetails?.Invoke(details);
                ListPool<ChangeDetail<KeyValuePair<TKey,TValue>>>.Release(details);
            }

            return true;
        }
        
        public void Clear()
        {
            if (!CheckPermission())
            {
                return;
            }

            if (_dictionary.Count == 0)
            {
                return;
            }
            
            var details = ListPool<ChangeDetail<KeyValuePair<TKey,TValue>>>.Get();
            foreach (var item in _dictionary)
            {
                details.Add(ChangeDetail<KeyValuePair<TKey,TValue>>.CreateRemove(item));
            }
            _dictionary.Clear();
            _functionOperations.Add(new (Function.Clear,default));
            SetDirty(true);
            OnValueChanged?.Invoke();
            OnValueChangedWithDetails?.Invoke(details);
            ListPool<ChangeDetail<KeyValuePair<TKey,TValue>>>.Release(details);
        }
        
        public override void WriteDelta(FastBufferWriter writer)
        {
            if (base.IsDirty())
            {
                writer.WriteValueSafe(1);
                writer.WriteValueSafe(Function.Full);
                WriteField(writer);
                return;
            }
            
            var operationCount = 0;
            for (int i = 0; i < _functionOperations.Count; i++)
            {
                if (i > 0 && _functionOperations[i - 1].Function == Function.AddRange &&
                    _functionOperations[i].Function == Function.AddRange)
                {
                    continue;
                }
                operationCount++;
            }
            writer.WriteValueSafe(operationCount);
            
            for (int i = 0; i < _functionOperations.Count; i++)
            {
                var operation = _functionOperations[i];
                writer.WriteValueSafe(operation.Function);
                switch (operation.Function)
                {
                    case Function.Add:
                    case Function.Modify:
                    {
                        var key = operation.Item.Key;
                        var value = operation.Item.Value;
                        NetworkVariableSerialization<TKey>.Write(writer, ref key);
                        NetworkVariableSerialization<TValue>.Write(writer, ref value);
                        break;
                    }
                    case Function.Remove:
                    {
                        var key = operation.Item.Key;
                        NetworkVariableSerialization<TKey>.Write(writer, ref key);
                        break;
                    }
                    case Function.AddRange:
                    {
                        var count = 1;
                        for (var j = i; j + 1 < _functionOperations.Count; j++, count++) ;
                        writer.WriteValueSafe(count);
                        for (var j = i; j < i + count; j++)
                        {
                            var key = _functionOperations[j].Item.Key;
                            var value = _functionOperations[j].Item.Value;
                            NetworkVariableSerialization<TKey>.Write(writer, ref key);
                            NetworkVariableSerialization<TValue>.Write(writer, ref value);
                        }
                        i += count - 1;
                        break;                        
                    }
                    case Function.Clear:
                    {
                        break;
                    }
                }
            }
            
            _functionOperations.Clear();
        }

        public override void WriteField(FastBufferWriter writer)
        {
            writer.WriteValueSafe(_dictionary.Count);
            foreach (var kv in _dictionary)
            {
                var key = kv.Key;
                var value = kv.Value;
                NetworkVariableSerialization<TKey>.Write(writer,ref key);
                NetworkVariableSerialization<TValue>.Write(writer,ref value);
            }
        }

        public override void ReadField(FastBufferReader reader)
        {
            _dictionary.Clear();
            reader.ReadValueSafe(out int count);
            for (int i = 0; i < count; i++)
            {
                var key = default(TKey);
                var value = default(TValue);
                NetworkVariableSerialization<TKey>.Read(reader,ref key);
                NetworkVariableSerialization<TValue>.Read(reader,ref value);
                _dictionary.TryAdd(key,value);
            }
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            var details = ListPool<ChangeDetail<KeyValuePair<TKey,TValue>>>.Get();
            reader.ReadValueSafe(out int operationCount);
            for (int i = 0; i < operationCount; i++)
            {
                reader.ReadValueSafe(out Function function);
                switch (function)
                {
                    case Function.Add:
                    {
                        var key = default(TKey);
                        var value = default(TValue);
                        NetworkVariableSerialization<TKey>.Read(reader, ref key);
                        NetworkVariableSerialization<TValue>.Read(reader, ref value);
                        details.Add(ChangeDetail<KeyValuePair<TKey,TValue>>.CreateAdd(new KeyValuePair<TKey,TValue>(key,value)));
                        _dictionary.Add(key,value);
                        break;
                    }
                    case Function.AddRange:
                    {
                        reader.ReadValueSafe(out int count);
                        for (int j = 0; j < count; j++)
                        {
                            var key = default(TKey);
                            var value = default(TValue);
                            NetworkVariableSerialization<TKey>.Read(reader, ref key);
                            NetworkVariableSerialization<TValue>.Read(reader, ref value);
                            details.Add(ChangeDetail<KeyValuePair<TKey,TValue>>.CreateAdd(new KeyValuePair<TKey,TValue>(key,value)));
                           _dictionary.Add(key,value);
                        }
                        break;
                    }
                    case Function.Modify:
                    {
                        var key = default(TKey);
                        var value = default(TValue);
                        NetworkVariableSerialization<TKey>.Read(reader, ref key);
                        NetworkVariableSerialization<TValue>.Read(reader, ref value);
                        details.Add(ChangeDetail<KeyValuePair<TKey,TValue>>.CreateModify(new KeyValuePair<TKey,TValue>(
                            key,_dictionary.GetValueOrDefault(key)),new KeyValuePair<TKey,TValue>(key,value)));
                        _dictionary[key] = value;
                        break;
                    }
                    case Function.Remove:
                    {
                        var key = default(TKey);
                        NetworkVariableSerialization<TKey>.Read(reader, ref key);
                        details.Add(ChangeDetail<KeyValuePair<TKey,TValue>>.CreateRemove(new KeyValuePair<TKey,TValue>(key,_dictionary.GetValueOrDefault(key))));
                        _dictionary.Remove(key);
                        break;
                    }
                    case Function.Clear:
                    {
                        foreach (var item in _dictionary)
                        {
                            details.Add(ChangeDetail<KeyValuePair<TKey,TValue>>.CreateRemove(item));
                        }
                        _dictionary.Clear();
                        break;
                    }
                    case Function.Full:
                    {
                        ReadField(reader);
                        ResetDirty();
                        break;
                    }
                    default:
                    {
                        Debug.LogError($"Name: Unknown function {function}");
                        break;
                    }
                }
            }
            
            if (keepDirtyDelta)
            {
                SetDirty(true);
            }
            
            OnValueChanged?.Invoke();
            OnValueChangedWithDetails?.Invoke(details);
            ListPool<ChangeDetail<KeyValuePair<TKey,TValue>>>.Release(details);
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                ReadField(serializer.GetFastBufferReader());
            }
            else if (serializer.IsWriter)
            {
                WriteField(serializer.GetFastBufferWriter());
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (index > 0 && index < _dictionary.Count)
            {
                array[index] = _dictionary.ElementAt(index);
            }
        }

        private bool CheckPermission()
        {
            if (!NetworkManager.Singleton)
            {
                return false;
            }
            if (!CanClientWrite(NetworkManager.Singleton.LocalClientId))
            {
                Debug.LogError($"|Client-{NetworkManager.Singleton.LocalClientId}|{Name}| Write permissions ({WritePerm}) for this client instance is not allowed!");
            }

            return true;
        }
    }
}
#endif