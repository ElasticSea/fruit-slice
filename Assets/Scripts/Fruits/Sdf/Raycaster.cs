using System;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Fruits.Sdf
{
    public class Raycaster : IDisposable
    {
        private NativeArray<RaycastHit> hits1;
        private NativeArray<RaycastHit> hits2;
        private NativeArray<Vector3> dirs;
        private NativeArray<RaycastCommand> commands1;
        private NativeArray<RaycastCommand> commands2;
        private NativeArray<float> distances;

        public Raycaster(int size)
        {
            hits1 = new NativeArray<RaycastHit>(size, Allocator.Persistent);
            hits2 = new NativeArray<RaycastHit>(size, Allocator.Persistent);
            dirs = new NativeArray<Vector3>(size, Allocator.Persistent);
            commands1 = new NativeArray<RaycastCommand>(size, Allocator.Persistent);
            commands2 = new NativeArray<RaycastCommand>(size, Allocator.Persistent);
            distances = new NativeArray<float>(size, Allocator.Persistent);
        }

        public float Run(Vector3 origin, Vector3[] directions)
        {
            var size = directions.Length;

            dirs.CopyFrom(directions);

            var jobA1 = new CreateRaycastCommand
            {
                Directions = dirs,
                Origin = origin,
                Mask = LayerMask.GetMask("Outside"),
                Results = commands1
            }.Schedule(size, 64);

            var jobB1 = RaycastCommand.ScheduleBatch(commands1, hits1, 1, jobA1);


            var jobA2 = new CreateRaycastCommand
            {
                Directions = dirs,
                Origin = origin,
                Mask = LayerMask.GetMask("Inside"),
                Results = commands2
            }.Schedule(size, 64, jobB1);

            var jobB2 = RaycastCommand.ScheduleBatch(commands2, hits2, 1, jobA2);

            var jobC = new Distance()
            {
                InsideHits = hits2,
                OutsideHits = hits1,
                Results = distances
            }.Schedule(size, 64, jobB2);


            jobC.Complete();


            var shortestRay = float.PositiveInfinity;

            for (var l = 0; l < distances.Length; l++)
            {
                var distance = distances[l];
                if (Mathf.Abs(distance) < Mathf.Abs(shortestRay))
                {
                    shortestRay = distance;
                }
            }

            return shortestRay;
        }

        public void Dispose()
        {
            hits1.Dispose();
            hits2.Dispose();
            dirs.Dispose();
            commands1.Dispose();
            commands2.Dispose();
            distances.Dispose();
        }
    }
}