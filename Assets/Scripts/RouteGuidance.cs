using UnityEngine;

namespace CityWalk
{
    /// <summary>Animated ground chevrons that follow the roads to the current target.</summary>
    public class RouteGuidance : MonoBehaviour
    {
        public int chevronCount = 12;
        public float spacing = 2.4f;
        public float flowSpeed = 2.5f;
        public float startDistance = 1.5f;
        public float groundHeight = 0.04f;

        static readonly Color Green = new Color(0.55f, 0.85f, 0.65f);

        Transform player;
        Transform[] chevrons;
        readonly Vector3[] path = new Vector3[3];
        Vector3 target;
        float pitch;
        bool active;

        public void Init(Transform playerTransform, float roadPitch)
        {
            player = playerTransform;
            pitch = roadPitch;

            var mesh = ChevronMesh();
            var material = MaterialFactory.Get(Green, 0.5f);
            chevrons = new Transform[chevronCount];
            for (int i = 0; i < chevronCount; i++)
            {
                chevrons[i] = SpawnMesh("Chevron", transform, mesh, material).transform;
                chevrons[i].gameObject.SetActive(false);
            }
        }

        public void SetTarget(Vector3 position)
        {
            target = position;
            active = true;
        }

        public void Hide()
        {
            active = false;
            foreach (var c in chevrons) c.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (!active || player == null) return;

            BuildPath();

            float total = 0f;
            for (int i = 0; i < 2; i++) total += Vector3.Distance(path[i], path[i + 1]);

            float flow = Mathf.Repeat(Time.time * flowSpeed, spacing);
            float farEnd = startDistance + chevronCount * spacing;

            for (int i = 0; i < chevronCount; i++)
            {
                float s = startDistance + i * spacing + flow;
                float scale = Mathf.Min(
                    Mathf.Clamp01((s - startDistance) / 1.5f),
                    Mathf.Clamp01((total - s) / 1.5f),
                    Mathf.Clamp01((farEnd - s) / 3f));

                Vector3 position = Vector3.zero, direction = Vector3.forward;
                bool show = scale > 0.01f && PointAlongPath(s, out position, out direction);
                chevrons[i].gameObject.SetActive(show);
                if (!show) continue;

                chevrons[i].SetPositionAndRotation(position, Quaternion.LookRotation(direction, Vector3.up));
                chevrons[i].localScale = Vector3.one * scale;
            }
        }

        void BuildPath()
        {
            Vector3 p = player.position;
            float roadX = (Mathf.Floor(p.x / pitch) + 0.5f) * pitch;
            float roadZ = (Mathf.Floor(p.z / pitch) + 0.5f) * pitch;

            // Follow the road the player is closest to, then turn once onto the target's road.
            bool followingEastWest = Mathf.Abs(p.z - roadZ) <= Mathf.Abs(p.x - roadX);
            Vector3 corner = followingEastWest
                ? new Vector3(target.x, groundHeight, roadZ)
                : new Vector3(roadX, groundHeight, target.z);

            path[0] = new Vector3(p.x, groundHeight, p.z);
            path[1] = corner;
            path[2] = new Vector3(target.x, groundHeight, target.z);
        }

        bool PointAlongPath(float distance, out Vector3 position, out Vector3 direction)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector3 segment = path[i + 1] - path[i];
                float length = segment.magnitude;
                if (length < 0.01f) continue;

                if (distance <= length)
                {
                    direction = segment / length;
                    position = path[i] + direction * distance;
                    return true;
                }
                distance -= length;
            }

            position = Vector3.zero;
            direction = Vector3.forward;
            return false;
        }

        public static GameObject SpawnMesh(string name, Transform parent, Mesh mesh, Material material)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        static Mesh ChevronMesh()
        {
            const float halfWidth = 0.95f, front = 0.35f, thickness = 0.32f;
            var mesh = new Mesh { name = "Chevron" };
            mesh.vertices = new[]
            {
                new Vector3(-halfWidth, 0f, -front - thickness),
                new Vector3(-halfWidth, 0f, -front),
                new Vector3(0f, 0f, front),
                new Vector3(0f, 0f, front - thickness),
                new Vector3(halfWidth, 0f, -front),
                new Vector3(halfWidth, 0f, -front - thickness),
            };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 3, 2, 4, 3, 4, 5 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh RingMesh(float inner, float outer, int segments = 48)
        {
            var vertices = new Vector3[segments * 2];
            var triangles = new int[segments * 6];
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                var direction = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                vertices[i * 2] = direction * inner;
                vertices[i * 2 + 1] = direction * outer;

                int next = (i + 1) % segments;
                int t = i * 6;
                triangles[t] = i * 2;
                triangles[t + 1] = next * 2;
                triangles[t + 2] = next * 2 + 1;
                triangles[t + 3] = i * 2;
                triangles[t + 4] = next * 2 + 1;
                triangles[t + 5] = i * 2 + 1;
            }

            var mesh = new Mesh { name = "Ring", vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh DiamondMesh(float halfWidth, float halfHeight)
        {
            Vector3 top = Vector3.up * halfHeight, bottom = Vector3.down * halfHeight;
            Vector3[] edge =
            {
                Vector3.forward * halfWidth, Vector3.right * halfWidth,
                Vector3.back * halfWidth, Vector3.left * halfWidth,
            };

            var vertices = new Vector3[24];
            var triangles = new int[24];
            for (int i = 0; i < 4; i++)
            {
                Vector3 a = edge[i], b = edge[(i + 1) % 4];
                vertices[i * 6] = top;
                vertices[i * 6 + 1] = a;
                vertices[i * 6 + 2] = b;
                vertices[i * 6 + 3] = bottom;
                vertices[i * 6 + 4] = b;
                vertices[i * 6 + 5] = a;
            }
            for (int i = 0; i < 24; i++) triangles[i] = i;

            var mesh = new Mesh { name = "Diamond", vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}