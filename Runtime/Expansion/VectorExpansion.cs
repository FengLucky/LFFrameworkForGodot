using Godot;

namespace LF.Runtime
{
    public static class VectorExpansion
    {
        public static Vector3 NoY(this Vector3 vec)
        {
            return new Vector3(vec.X, 0, vec.Z);
        }
    }
}