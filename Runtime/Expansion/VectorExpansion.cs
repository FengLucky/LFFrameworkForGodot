using UnityEngine;

namespace LF.Runtime
{
    public static class VectorExpansion
    {
        public static Vector3 NoY(this Vector3 vec)
        {
            return new Vector3(vec.x, 0, vec.z);
        }
    }
}