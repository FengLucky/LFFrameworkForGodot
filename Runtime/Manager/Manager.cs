namespace LF;

public abstract class Manager<T> where T : Manager<T>, new()
{
    private static T _instance;
    private static readonly object Lock = new();

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
            }

            return _instance;
        }
    }
}