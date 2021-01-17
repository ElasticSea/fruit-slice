using Core.Extensions;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Fruits.Builder.EdgeTexture.Jobs
{
    public struct RaycastOriginJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> Indexes;
        [WriteOnly] public NativeArray<Vector3> Results;

        public int Width;
        public int Height;
        public int Depth;
        public Bounds Bounds;
        public Matrix4x4 LocalToWorld;
        
        public void Execute(int index)
        {
            Results[index] = EdgeTextureUtils.IndexToPoint(Indexes[index], Bounds, LocalToWorld, Width, Height, Depth);
        }
    }
}