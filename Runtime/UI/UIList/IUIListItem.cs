namespace LF;
public interface IUIListItem<TData>
{
    void Refresh(TData data);
}