using UnityEngine;

namespace Fruits.Builder
{
    public class PreviewQuad : MonoBehaviour
    {
        [Range(0, 1)]
        [SerializeField] private float offset;
        [SerializeField] private Vector3 size = Vector3.one;

        private void OnValidate()
        {
            GetComponent<MeshFilter>().mesh = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(-size.x / 2, offset * size.y - size.y / 2, -size.z / 2),
                    new Vector3(-size.x / 2, offset * size.y - size.y / 2, size.z / 2),
                    new Vector3(size.x / 2, offset * size.y - size.y / 2, size.z / 2),
                    new Vector3(size.x / 2, offset * size.y - size.y / 2, -size.z / 2)
                },
                triangles = new[] {0, 1, 2, 0, 2, 3},
                normals = new[]
                {
                    Vector3.up,
                    Vector3.up,
                    Vector3.up,
                    Vector3.up
                },
                uv = new[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0)
                }
            };
        }
    }
}