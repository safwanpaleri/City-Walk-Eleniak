using System.Collections.Generic;
using UnityEngine;

namespace CityWalk
{
    /// <summary>
    /// Old Scrpit, Use LapRoute
    /// </summary>
    public class CheckpointRoute : MonoBehaviour
    {
        [System.Serializable]
        public class Stop
        {
            public string title;
            public string hint;
            public Vector3 position;
        }

        public float reachRadius = 2.5f;

        readonly List<Stop> stops = new List<Stop>();
        readonly List<GameObject> markers = new List<GameObject>();
        Transform player;
        BasicHud hud;
        int index;

        static readonly Color MarkerColor = new Color(0.98f, 0.82f, 0.25f);

        public void Init(Transform playerTransform, BasicHud hudRef, IEnumerable<Stop> route)
        {
            player = playerTransform;
            hud = hudRef;
            stops.AddRange(route);

            foreach (var s in stops)
            {
                var m = new GameObject("Marker_" + s.title);
                m.transform.SetParent(transform, false);
                m.transform.position = s.position;
                MaterialFactory.Primitive(PrimitiveType.Cylinder, "Ring", m.transform, new Vector3(0f, 0.05f, 0f),
                    new Vector3(reachRadius * 2f, 0.02f, reachRadius * 2f), MarkerColor, false, 0.6f);
                MaterialFactory.Primitive(PrimitiveType.Cylinder, "Beacon", m.transform, new Vector3(0f, 3f, 0f),
                    new Vector3(0.25f, 3f, 0.25f), MarkerColor, false, 1.2f);
                m.SetActive(false);
                markers.Add(m);
            }

            index = 0;
            hud.SetCounter(0, stops.Count);
            ShowCurrent();
        }

        void ShowCurrent()
        {
            for (int i = 0; i < markers.Count; i++) markers[i].SetActive(i == index);
            if (index < stops.Count) hud.SetInstruction(stops[index].title, stops[index].hint);
        }

        void Update()
        {
            if (player == null || index >= stops.Count) return;

            var m = markers[index].transform;
            m.GetChild(1).localScale = new Vector3(0.25f, 3f + Mathf.Sin(Time.time * 3f) * 0.2f, 0.25f);

            Vector3 d = player.position - stops[index].position;
            d.y = 0f;
            if (d.sqrMagnitude <= reachRadius * reachRadius)
            {
                markers[index].SetActive(false);
                index++;
                hud.SetCounter(index, stops.Count);
                if (index >= stops.Count)
                {
                    hud.SetInstruction("Arrived", "You reached the office.");
                    //hud.ShowComplete();
                }
                else
                {
                    ShowCurrent();
                }
            }
        }
    }
}
