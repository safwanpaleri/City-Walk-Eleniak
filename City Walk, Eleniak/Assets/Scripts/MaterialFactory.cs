using System.Collections.Generic;
using UnityEngine;

namespace CityWalk
{
    /// <summary>
    /// Creates simple coloured materials that work in both URP and the Built-in pipeline.
    /// It copies the default material Unity gives a primitive, so the right shader is always used.
    /// </summary>
    public static class MaterialFactory
    {
        static Material baseMaterial;
        static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

        static Material BaseMaterial
        {
            get
            {
                if (baseMaterial == null)
                {
                    var probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    baseMaterial = probe.GetComponent<Renderer>().sharedMaterial;
                    Object.DestroyImmediate(probe);
                }
                return baseMaterial;
            }
        }

        public static Material Get(Color color, float emission = 0f)
        {
            string key = ColorUtility.ToHtmlStringRGBA(color) + "_" + emission.ToString("0.00");
            if (cache.TryGetValue(key, out var existing) && existing != null)
                return existing;

            var mat = new Material(BaseMaterial) { name = "CityWalk_" + key };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.15f);

            if (emission > 0f && mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                mat.SetColor("_EmissionColor", color * emission);
            }

            cache[key] = mat;
            return mat;
        }

        /// <summary>Creates a coloured primitive. Colliders are removed unless keepCollider is true.</summary>
        public static GameObject Primitive(PrimitiveType type, string name, Transform parent,
            Vector3 localPosition, Vector3 localScale, Color color, bool keepCollider = false, float emission = 0f)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().sharedMaterial = Get(color, emission);
            if (!keepCollider)
            {
                var col = go.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);
            }
            return go;
        }
    }
}
