using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Fruits.Builder.EdgeTexture.Jobs
{
    public struct CreateRaycastsJob : IJobParallelFor
    {
        public LayerMask Mask;

        [ReadOnly] public NativeArray<Vector3> Origins;
        [ReadOnly] public NativeArray<Vector3> Directions;

        [WriteOnly] public NativeArray<RaycastCommand> Results;

        public void Execute(int index)
        {
            Results[index] = new RaycastCommand(Origins[index], Directions[index], layerMask: Mask);
        }
    }
}