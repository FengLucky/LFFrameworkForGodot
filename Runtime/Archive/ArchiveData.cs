using System;

namespace LF;
/// <summary>
/// 游戏存档基类
/// </summary>
[Serializable]
public abstract class ArchiveData
{
    public Guid GUID { get; private set; }
    public bool IsArchive { get; private set; }= true;

    protected ArchiveData()
    {
        GUID = Guid.NewGuid();
    }
}