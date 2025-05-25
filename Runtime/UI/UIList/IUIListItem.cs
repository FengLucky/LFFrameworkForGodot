namespace LF.Runtime
{
    public interface IUIListItem<TData>
    {
        void Refresh(TData data);
    }
}