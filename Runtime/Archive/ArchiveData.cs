using System;
using UnityEngine;

namespace LF.Runtime
{
    /// <summary>
    /// 简易存档，用于加载存档时显示预览图
    /// </summary>
    /// <typeparam name="T">完整存档类型</typeparam>
    /// <typeparam name="TV">简略存档类型</typeparam>
    [Serializable]
    public abstract class ArchiveSimpleData<T, TV> where TV : ArchiveSimpleData<T, TV>, new() where T : ArchiveData
    {
        [SerializeField] protected SerializableGuid guid;
        public SerializableGuid Guid => guid;

        public virtual void ConvertFrom(T fullArchive)
        {
            guid = fullArchive.Guid;
        }
    }

    /// <summary>
    /// 游戏存档基类
    /// </summary>
    [Serializable]
    public abstract class ArchiveData
    {
        [SerializeField] protected SerializableGuid guid;
        [SerializeField] protected bool isArchive = true;
        
        public SerializableGuid Guid => guid;
        public bool IsArchive => isArchive;

        protected ArchiveData()
        {
            guid = SerializableGuid.Generate();
        }
    }
}