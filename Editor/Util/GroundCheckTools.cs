using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace LF.Editor
{
    public static class GroundCheckTools
    {
        [MenuItem("GameObject/贴地/通过物理")]
        private static void GroundCheckByPhysics()
        {
            foreach (var obj in Selection.gameObjects)
            {
                if (AssetDatabase.Contains(obj))
                {
                    continue;
                }
                
                var startPos = obj.transform.position;
                if (Physics.Raycast(startPos, Vector3.down, out var hit, 1000, Physics.AllLayers,
                        QueryTriggerInteraction.Ignore))
                {
                    obj.transform.position = hit.point;
                }
            }
        }
        [MenuItem("GameObject/贴地/通过导航")]
        private static void GroundCheckByNavmesh()
        {
            foreach (var obj in Selection.gameObjects)
            {
                if (AssetDatabase.Contains(obj))
                {
                    continue;
                }
                
                var startPos = obj.transform.position;
                if (NavMesh.SamplePosition(startPos, out var hit, 1000, NavMesh.AllAreas))
                {
                    obj.transform.position = hit.position;
                }
            }
        }
    }
}