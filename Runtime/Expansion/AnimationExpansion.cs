using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LF.Runtime
{
    public static class AnimationExpansion
    {
        public static void ImmediatelyCompletion(this Animation obj)
        {
            if (!obj.clip)
            {
                return;
            }
            obj[obj.clip.name].time = obj.clip.length;
            obj.Sample();
        }

        public static UniTask PlayToEnd(this Animation obj,string animationName)
        {
            obj.Play(animationName);
            if (!obj.clip)
            {
                return UniTask.CompletedTask;
            }

            return UniTask.WaitForSeconds(obj.clip.length);
        }
    }
}