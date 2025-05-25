using UnityEngine;
using UnityEngine.UI;

namespace LF.Runtime
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class EmptyGraphics:MaskableGraphic
    {
        public EmptyGraphics()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
