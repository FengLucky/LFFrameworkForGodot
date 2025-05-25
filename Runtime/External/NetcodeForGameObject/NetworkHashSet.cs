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
    public class NetworkHashSet<TValue> : NetworkVariableBase, INetworkSerializable,IReadOnlySet<TValue>, ICollection<TValue>
        where TValue : IEquatable<TValue>
    {
        private enum Function
        {
            Add,
            AddRange,
            Remove,
            Clear,
            Full
        }

        private struct FunctionOperation
        {
            public Function Function;
            public TValue Item;

            public FunctionOperation(Function function, TValue item)
            {
                Function = function;
                Item = item;
            }
        }

        public event Action OnValueChanged;
        public event Action<List<ChangeDetail<TValue>>> OnValueChangedWithDetails;

        public int Count => _set.Count;
        public bool IsReadOnly => false;

        private readonly HashSet<TValue> _set = new();
        private readonly List<FunctionOperation> _functionOperations = new();

        public NetworkHashSet() { }
        internal NetworkHashSet(IEnumerable<TValue> items)
        {
            foreach (var item in items)
            {
                _set.Add(item);
            }
        }
        public NetworkHashSet(NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm) : base(readPerm, writePerm)
        {
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        public void Add(TValue item)
        {
            if (!CheckPermission())
            {
                return;
            }

            _set.Add(item);
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
                _set.Add(item);
                _functionOperations.Add(new(Function.AddRange, item));
                details.Add(ChangeDetail<TValue>.CreateAdd(item));
            }

            SetDirty(true);
            OnValueChanged?.Invoke();
            OnValueChangedWithDetails?.Invoke(details);
            ListPool<ChangeDetail<TValue>>.Release(details);
        }

        public bool Contains(TValue item)
        {
            return _set.Contains(item);
        }

        public bool Remove(TValue item)
        {
            if (!CheckPermission())
            {
                return false;
            }

            if (_set.Remove(item))
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

        public void Clear()
        {
            if (!CheckPermission())
            {
                return;
            }

            if (_set.Count == 0)
            {
                return;
            }

            var details = ListPool<ChangeDetail<TValue>>.Get();
            foreach (var item in _set)
            {
                details.Add(ChangeDetail<TValue>.CreateRemove(item));
            }

            _set.Clear();
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
                        var value = operation.Item;
                        NetworkVariableSerialization<TValue>.Write(writer, ref value);
                        break;
                    }
                    case Function.AddRange:
                    {
                        var count = 1;
                        for (var j = i; j + 1 < _functionOperations.Count; j++, count++) ;
                        NetworkVariableSerialization<int>.Write(writer, ref count);
                        for (var j = i; j < i + count; j++)
                        {
                            var value = _functionOperations[j].Item;
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
            writer.WriteValueSafe(_set.Count);
            foreach (var value in _set)
            {
                var v = value;
                NetworkVariableSerialization<TValue>.Write(writer, ref v);
            }
        }

        public override void ReadField(FastBufferReader reader)
        {
            _set.Clear();
            reader.ReadValueSafe(out int count);
            for (int i = 0; i < count; i++)
            {
                var value = default(TValue);
                NetworkVariableSerialization<TValue>.Read(reader, ref value);
                _set.Add(value);
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
                        var value = default(TValue);
                        NetworkVariableSerialization<TValue>.Read(reader, ref value);
                        details.Add(ChangeDetail<TValue>.CreateAdd(value));
                        _set.Add(value);
                        break;
                    }
                    case Function.AddRange:
                    {
                        var count = 0;
                        NetworkVariableSerialization<int>.Read(reader, ref count);
                        for (int j = 0; j < count; j++)
                        {
                            var value = default(TValue);
                            NetworkVariableSerialization<TValue>.Read(reader, ref value);
                            details.Add(ChangeDetail<TValue>.CreateAdd(value));
                            _set.Add(value);
                        }

                        break;
                    }
                    case Function.Remove:
                    {
                        var value = default(TValue);
                        NetworkVariableSerialization<TValue>.Read(reader, ref value);
                        details.Add(ChangeDetail<TValue>.CreateRemove(value));
                        _set.Remove(value);
                        break;
                    }
                    case Function.Clear:
                    {
                        foreach (var item in _set)
                        {
                            details.Add(ChangeDetail<TValue>.CreateRemove(item));
                        }

                        _set.Clear();
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
            _set.CopyTo(array,index);
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