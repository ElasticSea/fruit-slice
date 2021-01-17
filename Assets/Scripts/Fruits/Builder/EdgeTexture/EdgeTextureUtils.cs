using Core.Extensions;
using UnityEngine;

namespace Fruits.Builder.EdgeTexture
{
    public class EdgeTextureUtils
    {
        public static Vector3 IndexToPoint(int index, Bounds bounds, Matrix4x4 localToWorld, int width, int height, int depth)
        {
            var z = index / (height * depth);
            index -= z * (height * depth);
            var y = index / depth;
            index -= y * depth;
            var x = index;
                
            var proc = new Vector3(x, y, z).Divide(new Vector3(width - 1, height - 1, depth - 1));
            var position = bounds.min + (bounds.size.Multiply(proc));
            return localToWorld.MultiplyPoint(position);
        }
    }
}