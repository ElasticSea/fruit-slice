using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct Distance : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RaycastHit> InsideHits;
        [ReadOnly] public NativeArray<RaycastHit> OutsideHits;

        [WriteOnly] public NativeArray<float> Results;

        public void Execute(int index)
        {
            var insideHit = InsideHits[index].distance;
            var outsideHit = OutsideHits[index].distance;

            if (insideHit + outsideHit == 0)
            {
                Results[index] = float.PositiveInfinity;
            }
            else
            {
                if (insideHit == 0)
                {
                    Results[index] = outsideHit;
                }else if (outsideHit == 0)
                {
                    Results[index] = -insideHit;
                }
                else if (outsideHit < insideHit)
                {
                    Results[index] = outsideHit;
                }
                else
                {
                    Results[index] = -insideHit;
                }
            }
        }
    }
}