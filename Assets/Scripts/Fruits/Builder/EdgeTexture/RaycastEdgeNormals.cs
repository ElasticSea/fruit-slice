using System;
using System.Linq;
using Core.Extensions;
using Core.Util;
using Fruits.Builder.EdgeTexture.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Fruits.Sdf
{
    public class RaycastEdgeNormals : IDisposable
    {
        private NativeArray<int> indexes;
        private NativeArray<Vector3> origins;
        private NativeArray<Vector3> dirs;
        private NativeArray<RaycastCommand> raycasts;
        private NativeArray<RaycastHit> hits;
        private NativeArray<Vector3> distances;
        private Vector3[] rawDirections;
        private Vector3[] pointsOnSphere;
        private int[] rawIndexes;
        
        private Transform transform;
        private Bounds bounds;
        private int width;
        private int height;
        private int depth;
        private int rays;
        private BitsPerChannel channelDepth;
        
        private int chunkSize;

        private const int MaxChunkSize = 1024 * 1024 * 32;
        private const int JobInnerloopBatchCount = 1024;

        public RaycastEdgeNormals(Transform transform, Bounds bounds, int width, int height, int depth, int rays, BitsPerChannel channelDepth)
        {
            this.transform = transform;
            this.bounds = bounds;
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.rays = rays;
            this.channelDepth = channelDepth;
            
            var dataSize = (long)width * height * depth * rays;
            chunkSize = (int) Mathf.Min(dataSize, MaxChunkSize);
            
            rawIndexes = new int[chunkSize];
            rawDirections = new Vector3[chunkSize];
            pointsOnSphere = Utils.PointsOnSphere(rays);
            
            indexes = new NativeArray<int>(chunkSize, Allocator.Persistent);
            origins = new NativeArray<Vector3>(chunkSize, Allocator.Persistent);
            dirs = new NativeArray<Vector3>(chunkSize, Allocator.Persistent);
            raycasts = new NativeArray<RaycastCommand>(chunkSize, Allocator.Persistent);
            hits = new NativeArray<RaycastHit>(chunkSize, Allocator.Persistent);
            distances = new NativeArray<Vector3>(chunkSize, Allocator.Persistent);
        }

        public Texture3D Run()
        {
            var output = new Vector3[width * height * depth];
            
            var total = 0;
            while (total < output.Length)
            {
                var pointChunkSize = Mathf.Floor(chunkSize / rays);
                var currentSize = (int) Mathf.Min(output.Length - total, pointChunkSize);
                var temp = RunChunk(total, currentSize);
                Array.Copy(temp, 0, output, total, temp.Length);
                total += currentSize;
            }

            return Export(output);
        }

        private Texture3D Export(Vector3[] tOutput1)
        {
            var (bytesPerChannel, format) = GetFormat(channelDepth);
            
            var normals = new NativeArray<Vector3>(tOutput1.Length, Allocator.TempJob);
            normals.CopyFrom(tOutput1);
            var bytes = new NativeArray<byte>(tOutput1.Length * bytesPerChannel * 8, Allocator.TempJob);

            
            var job0 = new ConvertToBytesJob()
            {
                Normals = normals,
                Results = bytes,
                BytesPerChannel = bytesPerChannel
            }.Schedule(tOutput1.Length, JobInnerloopBatchCount);
            
            job0.Complete();

            var tex = new Texture3D(width, height, depth, format, false);
            tex.SetPixelData(bytes, 0);
            tex.Apply();
          
            normals.Dispose();
            bytes.Dispose();

            return tex;
        }

        private (int bytesPerChannel, TextureFormat format) GetFormat(BitsPerChannel bitsPerChannel)
        {
            switch (bitsPerChannel)
            {
                case BitsPerChannel.Ch8:
                    return (1, TextureFormat.RGB24);
                case BitsPerChannel.Ch16:
                    return (2, TextureFormat.RGB48);
                default:
                    throw new ArgumentException($"[{bitsPerChannel}] is unsupported number of bits per channel");
            }
        }

        public Vector3[] RunChunk(int offset, int points)
        {
            for (var i = 0; i < points; i++)
            {
                var index = offset + i;

                for (var r = 0; r < rays; r++)
                {
                    var rIndex = i * rays +r;
                            
                    rawIndexes[rIndex] = index;
                    rawDirections[rIndex] = pointsOnSphere[r];
                }
            }

            return RunJob(rawIndexes, rawDirections, points * rays);
        }

        private Vector3[] RunJob(int[] rawInidexes, Vector3[] rawDirections, int length)
        {
            indexes.CopyFrom(rawInidexes);
            dirs.CopyFrom(rawDirections);

            var job0 = new RaycastOriginJob
            {
                Indexes = indexes,
                Results = origins,
                Width = width,
                Height = height,
                Depth = depth,
                Bounds = bounds,
                LocalToWorld = transform.localToWorldMatrix
            }.Schedule(length, JobInnerloopBatchCount);
            
            var jobA = new CreateRaycastsJob
            {
                Directions = dirs,
                Origins = origins,
                Mask = LayerMask.GetMask("Outside", "Inside"),
                Results = raycasts
            }.Schedule(length, JobInnerloopBatchCount, job0);

            var jobB = RaycastCommand.ScheduleBatch(raycasts, hits, 1, jobA);

            var jobC = new RaycastHitNormalJob()
            {
                Hits = hits,
                dirs = dirs,
                Results = distances
            }.Schedule(length, JobInnerloopBatchCount, jobB);

            jobC.Complete();

            var output = new Vector3[length/ rays];
            for (var i = 0; i < output.Length; i++)
            {
                var shortestRay =  Vector3.one * float.PositiveInfinity;
                for (var j = 0; j < rays; j++)
                {
                    var distance = distances[i * rays + j];
                    if (distance.magnitude < shortestRay.magnitude)
                    {
                        shortestRay = distance;
                    }
                }

                output[i] = shortestRay;
            }

            return output;
        }

        public void Dispose()
        {
            indexes.Dispose();
            origins.Dispose();
            dirs.Dispose();
            raycasts.Dispose();
            hits.Dispose();
            distances.Dispose();
        }
    }
}