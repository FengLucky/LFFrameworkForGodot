namespace LF.Runtime
{
    public class UIPageT<T>:UIPage where T : PageHolder
    {
        public T Holder => RawHolder as T;
    }
}