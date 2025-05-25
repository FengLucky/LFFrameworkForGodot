#if NETCODE_EXPANSION
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace LF.Runtime
{
    [Serializable]
    [GenerateSerializationForGenericParameter(0)]
    public class NetworkList<TValue> : NetworkVariableBase, INetworkSerializable,IReadOnlyList<TValue>, IList<TValue>
        where TValue : IEquatable<TValue>
    {
        private enum Function
        {
            Add,
            Insert,
            AddRange,
            Modify,
            Remove,
            RemoveAt,
            Clear,
            Full
        }

        private struct FunctionOperation
        {
            public Function Function;
            public TValue Item;
            public int Index;

            public FunctionOperation(Function function, TValue item, int index = 0)
            {
                Function = function;
                Item = item;
                Index = index;
            }
        }

        public event Action OnValueChanged;
        public event Action<List<ChangeDetail<TValue>>> OnValueChangedWithDetails;

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public TValue this[int index]
        {
            get => _list[index];
            set
            {
                if (!CheckPermission())
                {
                    return;
                }

                var oldValue = _list[index];
                if (oldValue.Equals(value))
                {
                    return;
                }

                _list[index] = value;
                var details = ListPool<ChangeDetail<TValue>>.Get();
                details.Add(ChangeDetail<TValue>.CreateModify(oldValue, value));
                _functionOperations.Add(new(Function.Modify, value,index));
                SetDirty(true);
                OnValueChanged?.Invoke();
                OnValueChangedWithDetails?.Invoke(details);
                ListPool<ChangeDetail<TValue>>.Release(details);
            }
        }

        private readonly List<TValue> _list = new();
        private readonly List<FunctionOperation> _functionOperations = new();

        public NetworkList(){ }

        internal NetworkList(IEnumerable<TValue> items)
        {
            _list.AddRange(items);
        }
        public NetworkList(NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm) : base(readPerm, writePerm)
        {
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        
        public override bool IsDirty()
        {
            return base.IsDirty() || _functionOperations.Count > 0;
        }

        public void Add(TValue item)
        {
            if (!CheckPermission())
            {
                return;
            }

            _list.Add(item);
            _functionOperations.Add(new(Function.Add, item));
            var details = ListPool<ChangeDetail<TValue>>.Get();
            details.Add(ChangeDetail<TValue>.CreateAdd(item));
            SetDirty(true);
            OnValueChanged?.Invoke();
            OnValueChangedWithDetails?.Invoke(details);
            ListPool<ChangeDetail<TValue>>.Release(details);
        }

        public void AddRange(IEnumerable<TValue> items)
        {
            if (!CheckPermission())
            {
                return;
            }

            var details = ListPool<ChangeDetail<TValue>>.Get();
            foreach (var item in items)
            {
                _list.Add(item);
                _functionOperations.Add(new(Function.AddRange, item));
                details.Add(ChangeDetail<TValue>.CreateAdd(item));
            }

            SetDirty(true);
            OnValueChanged?.Invoke();
            OnValueChangedWithDetails?.Invoke(details);
            ListPool<ChangeDetail<TValue>>.Release(details);
        }

        public void Insert(int index, TValue item)
        {
            if (!CheckPermission())
            {
                return;
            }

            _list.Insert(index, item);
            _functionOperations.Add(new(Function.Insert, item, index));
            var details = ListPool<ChangeDetail<TValue>>.Get();
            details.Add(ChangeDetail<TValue>.CreateAdd(item));
            SetDirty(true);
            OnValueChanged?.Invoke();
            OnValueChangedWithDetails?.Invoke(details);
            ListPool<ChangeDetail<TValue>>.Release(details);
        }

        public bool Contains(TValue item)
        {
            return _list.Contains(item);
        }

        public int IndexOf(TValue item)
        {
            return _list.IndexOf(item);
        }

        public bool Remove(TValue item)
        {
            if (!CheckPermission())
            {
                return false;
            }

            if (_list.Remove(item))
            {
                _functionOperations.Add(new(Function.Remove, item));
                var details = ListPool<ChangeDetail<TValue>>.Get();
                details.Add(ChangeDetail<TValue>.CreateRemove(item));
                SetDirty(true);
                OnValueChanged?.Invoke();
                OnValueChangedWithDetails?.Invoke(details);
                ListPool<ChangeDetail<TValue>>.Release(details);
            }

            return true;
        }

        public void RemoveAt(int index)
        {
            if (!CheckPermission())
            {
                return;
            }

            if (index >= _list.Count)
            {
                return;
            }

            var item = _list[index];
            _list.RemoveAt(index);
            _functionOperations.Add(new(Function.RemoveAt, item));
            var details = ListPool<ChangeDetail<TValue>>.Get();
            details.Add(ChangeDetail<TValue>.CreateRemove(item));
            SetDirty(true);
            OnValueChanged?.Invoke();
            OnValueChangedWithDetails?.Invoke(details);
            ListPool<ChangeDetail<TValue>>.Release(details);
        }

        public void Clear()
        {
            if (!CheckPermission())
            {
                return;
            }

            if (_list.Count == 0)
            {
                return;
            }

            var details = ListPool<ChangeDetail<TValue>>.Get();
            foreach (var item in _list)
            {
                details.Add(ChangeDetail<TValue>.CreateRemove(item));
            }

            _list.Clear();
            _functionOperations.Add(new(Function.Clear, default));
            SetDirty(true);
            OnValueChanged?.Invoke();
            OnValueChangedWithDetails?.Invoke(details);
            ListPool<ChangeDetail<TValue>>.Release(details);
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
                    case Function.Remove:
                    {
                        NetworkVariableSerialization<TValue>.Write(writer, ref operation.Item);
                        break;
                    }
                    case Function.AddRange:
                    {
                        var count = 1;
                        for (var j = i; j + 1 < _functionOperations.Count; j++, count++) ;
                        writer.WriteValueSafe(count);
                        for (var j = i; j < i + count; j++)
                        {
                            var value = _functionOperations[j].Item;
                            NetworkVariableSerialization<TValue>.Write(writer, ref value);
                        }

                        i += count - 1;
                        break;
                    }
                    case Function.Modify:
                    case Function.Insert:
                    {
                        writer.WriteValueSafe(operation.Index);
                        NetworkVariableSerialization<TValue>.Write(writer, ref operation.Item);
                        break;
                    }
                    case Function.RemoveAt:
                    {
                        writer.WriteValueSafe(operation.Index);
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
            writer.WriteValueSafe(_list.Count);
            foreach (var value in _list)
            {
                var v = value;
                NetworkVariableSerialization<TValue>.Write(writer, ref v);
            }
        }

        public override void ReadField(FastBufferReader reader)
        {
            _list.Clear();
            reader.ReadValueSafe(out int count);
            for (int i = 0; i < count; i++)
            {
                TValue value = default;
                NetworkVariableSerialization<TValue>.Read(reader, ref value);
                _list.Add(value);
            }
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            var details = ListPool<ChangeDetail<TValue>>.Get();
            reader.ReadValueSafe(out int operationCount);
            for (int i = 0; i < operationCount; i++)
            {
                reader.ReadValueSafe(out Function function);
                switch (function)
                {
                    case Function.Add:
                    {
                        TValue value = default;
                        NetworkVariableSerialization<TValue>.Read(reader, ref value);
                        details.Add(ChangeDetail<TValue>.CreateAdd(value));
                        _list.Add(value);
                        break;
                    }
                    case Function.Insert:
                    {
                        TValue value = default;
                        reader.ReadValueSafe(out int index);
                        NetworkVariableSerialization<TValue>.Read(reader, ref value);
                        details.Add(ChangeDetail<TValue>.CreateAdd(value));
                        _list.Insert(index,value);
                        break;
                    }
                    case Function.AddRange:
                    {
                        reader.ReadValueSafe(out int count);
                        for (int j = 0; j < count; j++)
                        {
                            TValue value = default;
                            NetworkVariableSerialization<TValue>.Read(reader, ref value);
                            details.Add(ChangeDetail<TValue>.CreateAdd(value));
                            _list.Add(value);
                        }

                        break;
                    }
                    case Function.Modify:
                    {
                        TValue value = default;
                        reader.ReadValueSafe(out int index);
                        NetworkVariableSerialization<TValue>.Read(reader, ref value);
                        details.Add(ChangeDetail<TValue>.CreateModify(_list[index],value));
                        _list[index] = value;
                        break;
                    }
                    case Function.Remove:
                    {
                        TValue value = default;
                        NetworkVariableSerialization<TValue>.Read(reader, ref value);
                        details.Add(ChangeDetail<TValue>.CreateRemove(value));
                        _list.Remove(value);
                        break;
                    }
                    case Function.RemoveAt:
                    {
                        reader.ReadValueSafe(out int index);
                        details.Add(ChangeDetail<TValue>.CreateRemove(_list[index]));
                        _list.RemoveAt(index);
                        break;
                    }
                    case Function.Clear:
                    {
                        foreach (var item in _list)
                        {
                            details.Add(ChangeDetail<TValue>.CreateRemove(item));
                        }
                        _list.Clear();
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
            ListPool<ChangeDetail<TValue>>.Release(details);
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

        void ICollection<TValue>.CopyTo(TValue[] array, int index)
        {
            _list.CopyTo(array,index);
        }

        private bool CheckPermission()
        {
            if (!NetworkManager.Singleton)
            {
                return false;
            }

            if (!CanClientWrite(NetworkManager.Singleton.LocalClientId))
            {
                Debug.LogError(
                    $"|Client-{NetworkManager.Singleton.LocalClientId}|{Name}| Write permissions ({WritePerm}) for this client instance is not allowed!");
            }

            return true;
        }
    }
}
#endif