using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace LF.Runtime
{
    public static class NavMeshAgentExpansion
    {
        public static async UniTask MoveTo(this NavMeshAgent agent, Vector3 target,CancellationToken cancellationToken = default)
        {
            var stopDistance = agent.stoppingDistance * agent.stoppingDistance;
            var transform = agent.transform;
            agent.SetDestination(target);
            while (transform && (target - transform.position).NoY().sqrMagnitude > stopDistance)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UniTask.Yield();
            }
        }
    }
}