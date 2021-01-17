using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Fruits.Builder.EdgeTexture.Jobs
{
    public struct ConvertToBytesJob : IJobParallelFor
    {
        public int BytesPerChannel;
        [ReadOnly] public NativeArray<Vector3> Normals;

        [NativeDisableParallelForRestriction] 
        [WriteOnly] public NativeArray<byte> Results;

        public void Execute(int index)
        {
            var normal = Normals[index];

            WriteVecPart(BytesPerChannel * (index * 3 + 0), normal.x);
            WriteVecPart(BytesPerChannel * (index * 3 + 1), normal.y);
            WriteVecPart(BytesPerChannel * (index * 3 + 2), normal.z);
        }

        private byte[] GetBytes(float value)
        {
            switch (BytesPerChannel)
            {
                case 1:
                    return new[] {(byte) (value * byte.MaxValue)};
                case 2:
                    var ushortValue = (ushort) (value * ushort.MaxValue);
                    return BitConverter.GetBytes(ushortValue);
                case 4:
                    var uintValue = (uint) (value * uint.MaxValue);
                    return BitConverter.GetBytes(uintValue);
                default:
                    throw new ArgumentException($"[{BytesPerChannel}] is unsupported number of bytes");
            }
        }
        
        void WriteFloat(int index, float vFloat)
        {
            var bytes = GetBytes(vFloat);
            for (var i = 0; i < bytes.Length; i++)
            {
                Results[index + i] = bytes[i];
            }
        }
        
        void WriteVecPart(int index, float vPart)
        {
            WriteFloat(index, vPart / 2 + 0.5f);
        }
    }
}