using UnityEngine;

namespace Fruits.Sdf
{
    public class Interpolation
    {
        private static float Blerp(
            float c000, float c100, float c010, float c110,
            float c001, float c101, float c011, float c111,
            float tx, float ty, float tz)
        {
            var c00 = Mathf.Lerp(c000, c100, tx);
            var c10 = Mathf.Lerp(c010, c110, tx);
            var c01 = Mathf.Lerp(c001, c101, tx);
            var c11 = Mathf.Lerp(c011, c111, tx);
            
            var c0 = Mathf.Lerp(c00, c10, ty);
            var c1 = Mathf.Lerp(c01, c11, ty);

            var c = Mathf.Lerp(c0, c1, tz);
            
            return c;
        }

        public static float[,,] Scale(float[,,] source, int width, int height, int depth)
        {
            var dest = new float[width, height, depth];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        float gx = (float) x / width * (source.GetLength(0) - 1);
                        float gy = (float) y / height * (source.GetLength(1) - 1);
                        float gz = (float) z / depth * (source.GetLength(2) - 1);
                        int gxi = (int) gx;
                        int gyi = (int) gy;
                        int gzi = (int) gz;
                        float c000 = source[gxi + 0, gyi + 0, gzi + 0];
                        float c100 = source[gxi + 1, gyi + 0, gzi + 0];
                        float c010 = source[gxi + 0, gyi + 1, gzi + 0];
                        float c110 = source[gxi + 1, gyi + 1, gzi + 0];
                        float c001 = source[gxi + 0, gyi + 0, gzi + 1];
                        float c101 = source[gxi + 1, gyi + 0, gzi + 1];
                        float c011 = source[gxi + 0, gyi + 1, gzi + 1];
                        float c111 = source[gxi + 1, gyi + 1, gzi + 1];

                        float gx2 = (float) x / (width - 1) * (source.GetLength(0) - 1);
                        float gy2 = (float) y / (height - 1) * (source.GetLength(1) - 1);
                        float gz2 = (float) z / (depth - 1) * (source.GetLength(2) - 1);

                        var value = Blerp(c000, c100, c010, c110, c001, c101, c011, c111, gx2 - gxi, gy2 - gyi, gz2 - gzi);

                        dest[x, y, z] = value;
                    }
                }
            }

            return dest;
        }
    }
}