using System.Collections.Generic;
using UnityEngine;

namespace CityWalk
{
    /// <summary>
    /// Spawns ambient pedestrians who walk loops around city blocks on the footpath.
    /// Add models to the array or raise noOfCharacters to expand the crowd.
    /// </summary>
    public class CrowdControl : MonoBehaviour
    {
        [Header("Crowd")]
        public GameObject[] characters;
        public int noOfCharacters = 15;

        [Header("Walking")]
        public float minSpeed = 1.2f;
        public float maxSpeed = 1.7f;
        public float animationBaseSpeed = 1.5f;
        public float turnSpeed = 360f;
        public int randomSeed = 11;

        [Header("Footpath")]
        public float footpathHeight = 0.15f;
        public float minEdgeInset = 1.0f; // distance in from the kerb
        public float maxEdgeInset = 1.4f;

        class Walker
        {
            public Transform transform;
            public Vector3 blockCentre;
            public float halfSize, distance, speed, direction;
        }

        readonly List<Walker> walkers = new List<Walker>();

        void Start()
        {
            var city = Object.FindFirstObjectByType<CityBuilder>();
            if (city == null)
            {
                Debug.LogError("CrowdControl: no CityBuilder found.");
                return;
            }
            if (characters == null || characters.Length == 0)
            {
                Debug.LogError("CrowdControl: add at least one model to Characters.");
                return;
            }

            var blocks = new List<Vector3>();
            for (int x = 0; x < city.blocksPerSide; x++)
                for (int z = 0; z < city.blocksPerSide; z++)
                    blocks.Add(city.BlockCentre(x, z));
            blocks.Sort((a, b) => a.sqrMagnitude.CompareTo(b.sqrMagnitude)); // busiest near the centre

            var rng = new System.Random(randomSeed);
            for (int i = 0; i < noOfCharacters; i++)
            {
                GameObject model = characters[i % characters.Length];
                if (model == null) continue;
                Spawn(model, blocks[i % blocks.Count], city.blockSize * 0.5f, rng, i);
            }
        }

        void Spawn(GameObject model, Vector3 blockCentre, float blockHalf, System.Random rng, int index)
        {
            float inset = Mathf.Lerp(minEdgeInset, maxEdgeInset, (float)rng.NextDouble());
            var walker = new Walker
            {
                blockCentre = blockCentre,
                halfSize = blockHalf - inset,
                speed = Mathf.Lerp(minSpeed, maxSpeed, (float)rng.NextDouble()),
                direction = index % 2 == 0 ? 1f : -1f,
            };
            walker.distance = (float)rng.NextDouble() * walker.halfSize * 8f;

            Vector3 position = PointOnLoop(walker, walker.distance);
            Vector3 ahead = PointOnLoop(walker, walker.distance + walker.direction);
            var instance = Instantiate(model, position, Quaternion.LookRotation(ahead - position, Vector3.up), transform);
            instance.name = model.name + "_" + index;
            walker.transform = instance.transform;

            var animator = instance.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                animator.speed = walker.speed / animationBaseSpeed;

                // Start each person at a different point in the walk cycle
                var controller = animator.runtimeAnimatorController;
                if (controller != null && controller.animationClips.Length > 0)
                    animator.Update((float)rng.NextDouble() * controller.animationClips[0].length);
            }

            walkers.Add(walker);
        }

        Vector3 PointOnLoop(Walker walker, float distance)
        {
            float h = walker.halfSize;
            float side = h * 2f;
            float d = Mathf.Repeat(distance, side * 4f);
            int edge = Mathf.Min(Mathf.FloorToInt(d / side), 3);
            float t = d - edge * side;

            Vector3 offset;
            switch (edge)
            {
                case 0: offset = new Vector3(-h + t, 0f, -h); break;
                case 1: offset = new Vector3(h, 0f, -h + t); break;
                case 2: offset = new Vector3(h - t, 0f, h); break;
                default: offset = new Vector3(-h, 0f, h - t); break;
            }

            return new Vector3(walker.blockCentre.x + offset.x, footpathHeight, walker.blockCentre.z + offset.z);
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;
            foreach (var walker in walkers)
            {
                walker.distance = Mathf.Repeat(walker.distance + walker.speed * walker.direction * deltaTime, walker.halfSize * 8f);

                Vector3 position = PointOnLoop(walker, walker.distance);
                Vector3 ahead = PointOnLoop(walker, walker.distance + walker.direction * 0.5f) - position;
                walker.transform.position = position;

                if (ahead.sqrMagnitude > 0.001f)
                {
                    walker.transform.rotation = Quaternion.RotateTowards(walker.transform.rotation,
                        Quaternion.LookRotation(ahead, Vector3.up), turnSpeed * deltaTime);
                }
            }
        }
    }
}