using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Fruits.Builder.EdgeTexture.Jobs
{
    public struct RaycastHitNormalJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RaycastHit> Hits;
        [ReadOnly] public NativeArray<Vector3> dirs;

        [WriteOnly] public NativeArray<Vector3> Results;

        public void Execute(int index)
        {
            var raycastHit = Hits[index];

            if (raycastHit.distance == 0)
            {
                Results[index] = Vector3.one * float.PositiveInfinity;
            }
            else
            {
                Results[index] = dirs[index] * raycastHit.distance;
            }
        }
    }
}