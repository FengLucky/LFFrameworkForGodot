using System;

#if NETCODE_EXPANSION
namespace LF.Runtime
{
    public struct ChangeDetail<TValue>
    {
        public ChangeType Type;
        /// <summary>
        /// 旧值，仅在Type为Remove或Modify时有值
        /// </summary>
        public TValue? OldValue;
        /// <summary>
        /// 新的值，仅在Type为Add或Modify时有值
        /// </summary>
        public TValue? NewValue;
        
        public static ChangeDetail<TValue> CreateAdd(TValue value) => new ()
        {
            Type = ChangeType.Add,
            NewValue = value
        };
        
        public static ChangeDetail<TValue> CreateRemove(TValue value) => new ()
        {
            Type = ChangeType.Remove,
            OldValue = value
        };
        
        public static ChangeDetail<TValue> CreateModify(TValue oldValue,TValue newValue) => new ()
        {
            Type = ChangeType.Modify,
            OldValue = oldValue,
            NewValue = newValue
        };
    }
}
#endif