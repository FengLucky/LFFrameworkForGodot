namespace LF;

public class SimpleArchiveBase 
{
    public int Index { get;private init; }
    public bool IsQuickArchive => Index < 0;

    public static T CreateNew<T>(int index)where T:SimpleArchiveBase,new()
    {
        return new T
        {
            Index = index
        };
    }
}