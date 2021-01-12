using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct CreateRaycastCommand : IJobParallelFor
    {
        public Vector3 Origin;
        public LayerMask Mask;

        [ReadOnly] public NativeArray<Vector3> Directions;

        [WriteOnly] public NativeArray<RaycastCommand> Results;

        public void Execute(int index)
        {
            Results[index] = new RaycastCommand(Origin, Directions[index], layerMask: Mask);
        }
    }
}