using UnityEngine;

namespace CityWalk
{
    /// <summary>
    /// Each lap visits every stop in order. Only the current stop has a beacon.
    /// Warns the player if they keep walking away from it.
    /// </summary>
    public class LapRoute : MonoBehaviour
    {
        [System.Serializable]
        public class Stop
        {
            public string title;
            public string hint;
            public Vector3 position;
        }

        public int totalLaps = 3;
        public float reachRadius = 2.5f;
        public float wrongWayDelay = 1f;       // seconds walking away before warning
        public float wrongWayClearTime = 0.4f; // seconds walking towards it to clear
        public float minClosingSpeed = 0.1f;   // metres per second

        public int LapsDone { get; private set; }
        public int WrongTurns { get; private set; }
        public bool WrongWay { get; private set; }
        public bool Finished { get; private set; }

        Transform player;
        BasicHud hud;
        Stop[] stops;
        Transform marker, diamond;
        RouteGuidance guidance;
        int stopIndex;
        float lastDistance, awayTime, towardsTime;

        static readonly Color MarkerColor = new Color(0.98f, 0.82f, 0.25f);

        Stop Current => stops[stopIndex];

        public void Init(Transform playerTransform, BasicHud hudRef, float roadPitch, Stop[] route)
        {
            player = playerTransform;
            hud = hudRef;
            stops = route;

            marker = new GameObject("StopMarker").transform;
            marker.SetParent(transform, false);
            var yellow = MaterialFactory.Get(MarkerColor, 1.2f);
            RouteGuidance.SpawnMesh("Ring", marker, RouteGuidance.RingMesh(reachRadius - 0.3f, reachRadius), yellow)
                .transform.localPosition = new Vector3(0f, 0.05f, 0f);
            diamond = RouteGuidance.SpawnMesh("Diamond", marker, RouteGuidance.DiamondMesh(0.55f, 1f), yellow).transform;

            var beam = MaterialFactory.Primitive(PrimitiveType.Cylinder, "Beam", marker, new Vector3(0f, 12f, 0f),
                new Vector3(2.4f, 12f, 2.4f), MarkerColor);
            var beamRenderer = beam.GetComponent<Renderer>();
            beamRenderer.sharedMaterial = Resources.Load<Material>("BeamMaterial");
            beamRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            guidance = gameObject.AddComponent<RouteGuidance>();
            guidance.Init(player, roadPitch);

            BeginStop();
        }

        float DistanceToStop()
        {
            Vector3 d = player.position - Current.position;
            d.y = 0f;
            return d.magnitude;
        }

        void BeginStop()
        {
            marker.position = Current.position;
            guidance.SetTarget(Current.position);
            lastDistance = DistanceToStop();
            awayTime = towardsTime = 0f;
            WrongWay = false;
            RefreshHud();
        }

        void Update()
        {
            if (Finished || player == null || Time.deltaTime <= 0f) return;

            diamond.localPosition = new Vector3(0f, 4.2f + Mathf.Sin(Time.time * 2f) * 0.25f, 0f);
            diamond.localRotation = Quaternion.Euler(0f, Time.time * 70f, 0f);

            float distance = DistanceToStop();
            if (distance <= reachRadius)
            {
                ReachStop();
                return;
            }

            UpdateWrongWay((lastDistance - distance) / Time.deltaTime);
            lastDistance = distance;
        }

        void ReachStop()
        {
            stopIndex++;
            if (stopIndex < stops.Length)
            {
                BeginStop();
                return;
            }

            stopIndex = 0;
            LapsDone++;
            if (LapsDone < totalLaps)
            {
                BeginStop();
                return;
            }

            Finished = true;
            marker.gameObject.SetActive(false);
            guidance.Hide();
            hud.SetWarning(false);
            hud.SetCounter(totalLaps, totalLaps);
            hud.SetInstruction("Arrived", "You reached the office.");
            hud.ShowComplete();
        }

        void UpdateWrongWay(float closingSpeed)
        {
            bool changed = false;

            if (closingSpeed < -minClosingSpeed)
            {
                awayTime += Time.deltaTime;
                towardsTime = 0f;
                if (!WrongWay && awayTime >= wrongWayDelay)
                {
                    WrongWay = true;
                    WrongTurns++;
                    changed = true;
                }
            }
            else if (closingSpeed > minClosingSpeed)
            {
                towardsTime += Time.deltaTime;
                awayTime = 0f;
                if (WrongWay && towardsTime >= wrongWayClearTime)
                {
                    WrongWay = false;
                    changed = true;
                }
            }

            if (changed) RefreshHud();
        }

        void RefreshHud()
        {
            hud.SetCounter(LapsDone + 1, totalLaps);
            hud.SetWarning(WrongWay);
            hud.SetPace(LapsDone < 2 ? "Rush hour: walk briskly" : "Normal pace: enjoy the walk");
            if (WrongWay)
            {
                hud.SetInstruction("Wrong way!", "Turn around and follow the beacon");
                return;
            }

            bool returning = stopIndex == 0 && LapsDone > 0;
            hud.SetInstruction(Current.title, returning ? "Head back to the coffee kiosk" : Current.hint);
        }
    }
}