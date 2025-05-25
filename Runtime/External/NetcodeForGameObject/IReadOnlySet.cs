using System.Collections.Generic;

namespace LF.Runtime
{
    public interface IReadOnlySet<TValue> : IReadOnlyCollection<TValue>
    {
        bool Contains(TValue item);
    }
}