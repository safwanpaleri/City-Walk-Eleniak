using UnityEngine;

namespace CityWalk
{
    /// <summary>
    /// Placeholder avatar: a man in a dark suit built from primitives,
    /// with a simple procedural walk (swinging arms and legs).
    /// Replace with a rigged, animated character as part of the task.
    /// </summary>
    public class SuitedManAvatar : MonoBehaviour
    {
        public float strideDegrees = 32f;
        public float stepsPerMetre = 1.3f;

        static readonly Color Suit = new Color(0.16f, 0.18f, 0.22f);
        static readonly Color Trousers = new Color(0.13f, 0.14f, 0.17f);
        static readonly Color Shirt = new Color(0.95f, 0.95f, 0.93f);
        static readonly Color Tie = new Color(0.55f, 0.12f, 0.14f);
        static readonly Color Skin = new Color(0.78f, 0.60f, 0.47f);
        static readonly Color Hair = new Color(0.10f, 0.08f, 0.07f);
        static readonly Color Shoes = new Color(0.07f, 0.06f, 0.06f);

        Transform body, leftLeg, rightLeg, leftArm, rightArm;
        float phase;
        float bodyBaseY;

        /// <summary>Speed in metres per second, set by the player controller each frame.</summary>
        public float Speed { get; set; }

        public static SuitedManAvatar Create(Transform parent)
        {
            var root = new GameObject("SuitedMan");
            root.transform.SetParent(parent, false);
            var avatar = root.AddComponent<SuitedManAvatar>();
            avatar.Build();
            return avatar;
        }

        void Build()
        {
            body = new GameObject("Body").transform;
            body.SetParent(transform, false);
            bodyBaseY = 0f;

            // Legs pivot at the hips
            leftLeg = Pivot("LeftHip", new Vector3(-0.11f, 0.95f, 0f));
            rightLeg = Pivot("RightHip", new Vector3(0.11f, 0.95f, 0f));
            foreach (var hip in new[] { leftLeg, rightLeg })
            {
                MaterialFactory.Primitive(PrimitiveType.Cube, "Leg", hip, new Vector3(0f, -0.45f, 0f), new Vector3(0.18f, 0.9f, 0.2f), Trousers);
                MaterialFactory.Primitive(PrimitiveType.Cube, "Shoe", hip, new Vector3(0f, -0.9f, 0.05f), new Vector3(0.19f, 0.1f, 0.3f), Shoes);
            }

            // Torso (jacket) with shirt and tie on the front (+Z is forward)
            MaterialFactory.Primitive(PrimitiveType.Cube, "Jacket", body, new Vector3(0f, 1.27f, 0f), new Vector3(0.5f, 0.66f, 0.28f), Suit);
            MaterialFactory.Primitive(PrimitiveType.Cube, "Shirt", body, new Vector3(0f, 1.42f, 0.141f), new Vector3(0.16f, 0.34f, 0.01f), Shirt);
            MaterialFactory.Primitive(PrimitiveType.Cube, "Tie", body, new Vector3(0f, 1.38f, 0.148f), new Vector3(0.06f, 0.36f, 0.01f), Tie);
            MaterialFactory.Primitive(PrimitiveType.Cylinder, "Neck", body, new Vector3(0f, 1.63f, 0f), new Vector3(0.12f, 0.05f, 0.12f), Skin);
            MaterialFactory.Primitive(PrimitiveType.Sphere, "Head", body, new Vector3(0f, 1.77f, 0f), new Vector3(0.24f, 0.27f, 0.25f), Skin);
            MaterialFactory.Primitive(PrimitiveType.Sphere, "Hair", body, new Vector3(0f, 1.83f, -0.02f), new Vector3(0.25f, 0.18f, 0.25f), Hair);

            // Arms pivot at the shoulders
            leftArm = Pivot("LeftShoulder", new Vector3(-0.31f, 1.55f, 0f));
            rightArm = Pivot("RightShoulder", new Vector3(0.31f, 1.55f, 0f));
            foreach (var shoulder in new[] { leftArm, rightArm })
            {
                MaterialFactory.Primitive(PrimitiveType.Cube, "Sleeve", shoulder, new Vector3(0f, -0.3f, 0f), new Vector3(0.13f, 0.6f, 0.16f), Suit);
                MaterialFactory.Primitive(PrimitiveType.Sphere, "Hand", shoulder, new Vector3(0f, -0.66f, 0f), Vector3.one * 0.11f, Skin);
            }
        }

        Transform Pivot(string name, Vector3 localPosition)
        {
            var t = new GameObject(name).transform;
            t.SetParent(body, false);
            t.localPosition = localPosition;
            return t;
        }

        void Update()
        {
            float speed01 = Mathf.Clamp01(Speed / 5f);
            if (Speed > 0.05f)
                phase += Speed * stepsPerMetre * Mathf.PI * Time.deltaTime;
            else
                phase = Mathf.MoveTowards(phase, Mathf.Round(phase / Mathf.PI) * Mathf.PI, Time.deltaTime * 6f);

            float swing = Mathf.Sin(phase) * strideDegrees * Mathf.Lerp(0.6f, 1.2f, speed01) * (Speed > 0.05f ? 1f : 0f);
            leftLeg.localRotation = Quaternion.Euler(swing, 0f, 0f);
            rightLeg.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            leftArm.localRotation = Quaternion.Euler(-swing * 0.8f, 0f, 0f);
            rightArm.localRotation = Quaternion.Euler(swing * 0.8f, 0f, 0f);

            float bob = Speed > 0.05f ? Mathf.Abs(Mathf.Cos(phase)) * 0.04f : 0f;
            var p = body.localPosition;
            p.y = Mathf.Lerp(p.y, bodyBaseY + bob, Time.deltaTime * 12f);
            body.localPosition = p;
        }
    }
}
