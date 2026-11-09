using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CityWalk
{
    /// <summary>Builds the dark-green and gold UI in code, including its icons.</summary>
    public static class UiFactory
    {
        public static readonly Color PanelGreen = new Color(0.05f, 0.19f, 0.17f, 0.90f);
        public static readonly Color Gold = new Color(0.91f, 0.76f, 0.42f);
        public static readonly Color Cream = new Color(0.98f, 0.95f, 0.86f);
        public static readonly Color Sage = new Color(0.78f, 0.84f, 0.79f);
        public static readonly Color Coral = new Color(1f, 0.45f, 0.36f);
        public static readonly Color DarkInk = new Color(0.08f, 0.12f, 0.10f);

        const int RoundedRadius = 48;
        static Sprite rounded, roundedRing, circle, circleRing, goldCircle, goldPill;
        static readonly Dictionary<string, Sprite> shapes = new Dictionary<string, Sprite>();

        static readonly Color32 GoldTop = new Color32(242, 212, 132, 255);
        static readonly Color32 GoldBottom = new Color32(201, 155, 72, 255);
        static readonly Color32 White = new Color32(255, 255, 255, 255);

        public static Sprite Rounded => rounded != null ? rounded : rounded = MakeShape(128, RoundedRadius, 0f, White, White);
        public static Sprite RoundedRing => roundedRing != null ? roundedRing : roundedRing = MakeShape(128, RoundedRadius, 4f, White, White);
        public static Sprite Circle => circle != null ? circle : circle = MakeShape(256, 128, 0f, White, White);
        public static Sprite CircleRing => circleRing != null ? circleRing : circleRing = MakeShape(256, 128, 4f, White, White);
        public static Sprite GoldCircle => goldCircle != null ? goldCircle : goldCircle = MakeShape(256, 128, 0f, GoldTop, GoldBottom);
        public static Sprite GoldPill => goldPill != null ? goldPill : goldPill = MakeShape(128, 64, 0f, GoldTop, GoldBottom);

        static Sprite MakeShape(int size, int radius, float ring, Color32 top, Color32 bottom)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                Color32 rowColor = Color32.Lerp(bottom, top, (y + 0.5f) / size);
                for (int x = 0; x < size; x++)
                {
                    float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                    float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                    float signed = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy)) - radius;
                    float alpha = Mathf.Clamp01(0.5f - signed);
                    if (ring > 0f) alpha *= Mathf.Clamp01(0.5f + signed + ring);
                    pixels[y * size + x] = new Color32(rowColor.r, rowColor.g, rowColor.b, (byte)(alpha * 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            float border = Mathf.Min(radius, size / 2 - 1);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
        }

        // ---- Icons -------------------------------------------------------------

        static Sprite Shape(string key, System.Func<Vector2, float> distance)
        {
            if (shapes.TryGetValue(key, out var existing) && existing != null) return existing;

            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = distance(new Vector2((x + 0.5f) / size, (y + 0.5f) / size));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(0.5f - d * size) * 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            shapes[key] = sprite;
            return sprite;
        }

        static float Disc(Vector2 p, float x, float y, float r) => (p - new Vector2(x, y)).magnitude - r;

        static float Segment(Vector2 p, float ax, float ay, float bx, float by, float r)
        {
            var a = new Vector2(ax, ay);
            Vector2 ba = new Vector2(bx, by) - a;
            float h = Mathf.Clamp01(Vector2.Dot(p - a, ba) / Vector2.Dot(ba, ba));
            return (p - a - ba * h).magnitude - r;
        }

        static float Box(Vector2 p, float cx, float cy, float hx, float hy)
        {
            float qx = Mathf.Abs(p.x - cx) - hx;
            float qy = Mathf.Abs(p.y - cy) - hy;
            return new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0f);
        }

        public static Sprite StarIcon => Shape("star", p =>
        {
            Vector2 c = p - new Vector2(0.5f, 0.5f);
            float phi = Mathf.Repeat(Mathf.Atan2(c.y, c.x) * Mathf.Rad2Deg - 90f, 72f);
            if (phi > 36f) phi = 72f - phi;
            float rad = phi * Mathf.Deg2Rad;
            var q = new Vector2(c.magnitude * Mathf.Cos(rad), c.magnitude * Mathf.Sin(rad));
            var a = new Vector2(0.48f, 0f);
            Vector2 edge = new Vector2(0.154f, 0.112f) - a;
            return Vector2.Dot(q - a, new Vector2(edge.y, -edge.x).normalized);
        });

        public static Sprite GearIcon => Shape("gear", p =>
        {
            Vector2 c = p - new Vector2(0.5f, 0.5f);
            float phi = Mathf.Repeat(Mathf.Atan2(c.y, c.x) * Mathf.Rad2Deg, 45f);
            if (phi > 22.5f) phi = 45f - phi;
            float outer = Mathf.Lerp(0.46f, 0.35f, Mathf.Clamp01((phi - 9f) / 6f));
            return Mathf.Max(c.magnitude - outer, 0.16f - c.magnitude);
        });

        public static Sprite WalkerIcon => Shape("walker", p => Mathf.Min(
            Disc(p, 0.55f, 0.84f, 0.10f),
            Segment(p, 0.52f, 0.70f, 0.46f, 0.42f, 0.07f),
            Segment(p, 0.52f, 0.66f, 0.70f, 0.52f, 0.05f),
            Segment(p, 0.50f, 0.66f, 0.32f, 0.48f, 0.05f),
            Mathf.Min(Segment(p, 0.46f, 0.42f, 0.62f, 0.10f, 0.06f), Segment(p, 0.46f, 0.42f, 0.28f, 0.12f, 0.06f))));

        public static Sprite CoffeeIcon => Shape("coffee", p => Mathf.Min(
            Box(p, 0.44f, 0.40f, 0.25f, 0.24f) - 0.02f,
            Mathf.Abs(Disc(p, 0.72f, 0.42f, 0.12f)) - 0.035f,
            Segment(p, 0.34f, 0.74f, 0.38f, 0.92f, 0.03f),
            Segment(p, 0.52f, 0.74f, 0.56f, 0.92f, 0.03f),
            Segment(p, 0.14f, 0.08f, 0.74f, 0.08f, 0.03f)));

        public static Sprite CityIcon => Shape("city", p => Mathf.Min(
            Box(p, 0.22f, 0.32f, 0.12f, 0.26f),
            Box(p, 0.50f, 0.46f, 0.13f, 0.40f),
            Box(p, 0.78f, 0.27f, 0.12f, 0.21f)));

        // ---- Elements ----------------------------------------------------------

        public static RectTransform MakeRect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return rt;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        /// <summary>Positions an element using top-left pixels inside its parent.</summary>
        public static void Place(RectTransform rt, float x, float y, float width, float height)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(width, height);
        }

        public static Image Panel(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, Color color, Sprite sprite = null, float radius = 24f)
        {
            var rt = MakeRect(name, parent, anchor, pivot, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite != null ? sprite : Rounded;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = sprite != null ? 1f : RoundedRadius / radius;
            img.color = color;
            return img;
        }

        public static Image CircleImage(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, Color color, Sprite sprite = null)
        {
            var rt = MakeRect(name, parent, anchor, pivot, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite != null ? sprite : Circle;
            img.type = Image.Type.Simple;
            img.color = color;
            return img;
        }

        public static void AddOutline(Image target, float radius)
        {
            var rt = MakeRect("Outline", target.transform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Stretch(rt);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = RoundedRing;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = RoundedRadius / radius;
            img.color = new Color(Gold.r, Gold.g, Gold.b, 0.55f);
            img.raycastTarget = false;
        }

        public static void AddCircleOutline(Image target)
        {
            var rt = MakeRect("Outline", target.transform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Stretch(rt);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = CircleRing;
            img.color = new Color(Gold.r, Gold.g, Gold.b, 0.8f);
            img.raycastTarget = false;
        }

        public static Image Icon(string name, Transform parent, Sprite sprite, Color color, float x, float y, float width, float height)
        {
            var rt = MakeRect(name, parent, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Place(rt, x, y, width, height);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }

        public static TextMeshProUGUI Label(string name, Transform parent, string text, float fontSize, Color color,
            TextAlignmentOptions align, bool bold, float x, float y, float width, float height)
        {
            var rt = MakeRect(name, parent, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Place(rt, x, y, width, height);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = align;
            t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            t.raycastTarget = false;
            return t;
        }
    }
}