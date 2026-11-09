using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CityWalk
{
    /// <summary>Dark-green and gold game HUD built in code.</summary>
    public class BasicHud : MonoBehaviour
    {
        public VirtualJoystick Joystick { get; private set; }
        public System.Action OnAction;

        TextMeshProUGUI counterText, instructionTitle, instructionBody, paceText, xpText, shadowLabel;
        TextMeshProUGUI completeSummary, completeXp, completeCoins, completeTime;
        Image bannerStrip;
        GameObject pauseOverlay, completePanel, settingsOverlay, mapOverlay, actionButton;
        RectTransform mapPlayer, mapTarget;
        Vector3 lastPlayerPosition, lastTargetPosition;
        float mapScale = 1f;

        public static BasicHud Create()
        {
            EnsureEventSystem();

            var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            float aspect = Screen.width / (float)Screen.height;
            scaler.matchWidthOrHeight = aspect < 16f / 9f ? 0f : 1f; // the layout always fits

            var hud = canvasGo.AddComponent<BasicHud>();
            hud.Build(canvasGo.transform);
            return hud;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        Transform BuildSafeArea(Transform root)
        {
            var rt = UiFactory.MakeRect("SafeArea", root, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Rect safe = Screen.safeArea;
            rt.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rt.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        Image CardLeft(string name, Transform parent, float x, float y, float width, float height, float radius = 20f)
        {
            var corner = new Vector2(0f, 1f);
            var card = UiFactory.Panel(name, parent, corner, corner, new Vector2(x, -y), new Vector2(width, height),
                UiFactory.PanelGreen, null, radius);
            UiFactory.AddOutline(card, radius);
            return card;
        }

        Image RoundButton(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, float size, UnityAction onClick)
        {
            var image = UiFactory.CircleImage(name, parent, anchor, pivot, pos, new Vector2(size, size), UiFactory.PanelGreen);
            UiFactory.AddCircleOutline(image);
            image.gameObject.AddComponent<Button>().onClick.AddListener(onClick);
            return image;
        }

        void Build(Transform root)
        {
            Transform safe = BuildSafeArea(root);
            var topRight = new Vector2(1f, 1f);

            // Title card
            var title = CardLeft("TitleCard", safe, 40f, 36f, 362f, 106f);
            var tileCorner = new Vector2(0f, 1f);
            var tile = UiFactory.Panel("CityTile", title.transform, tileCorner, tileCorner, new Vector2(18f, -19f),
                new Vector2(68f, 68f), UiFactory.Gold, null, 14f);
            tile.raycastTarget = false;
            UiFactory.Icon("CityIcon", tile.transform, UiFactory.CityIcon, UiFactory.DarkInk, 11f, 11f, 46f, 46f);
            UiFactory.Label("Title", title.transform, "City Walk", 32f, UiFactory.Cream, TextAlignmentOptions.Left, true, 104f, 12f, 245f, 46f);
            UiFactory.Label("Subtitle", title.transform, "Morning routine", 22f, UiFactory.Sage, TextAlignmentOptions.Left, false, 104f, 56f, 245f, 36f);

            // Lap counter
            var counter = CardLeft("LapCounter", safe, 420f, 36f, 130f, 106f);
            UiFactory.Label("LapLabel", counter.transform, "LAP", 20f, UiFactory.Sage, TextAlignmentOptions.Center, false, 0f, 10f, 130f, 28f);
            counterText = UiFactory.Label("LapValue", counter.transform, "1/3", 38f, UiFactory.Cream, TextAlignmentOptions.Center, true, 0f, 38f, 130f, 58f);

            // Instruction banner
            var banner = CardLeft("InstructionBanner", safe, 590f, 36f, 740f, 104f);
            var stripCorner = new Vector2(0f, 1f);
            bannerStrip = UiFactory.Panel("Strip", banner.transform, stripCorner, stripCorner, Vector2.zero,
                new Vector2(7f, 104f), UiFactory.Gold, null, 3f);
            bannerStrip.raycastTarget = false;
            instructionTitle = UiFactory.Label("InstructionTitle", banner.transform, "", 30f, UiFactory.Gold, TextAlignmentOptions.Left, true, 34f, 11f, 690f, 44f);
            instructionBody = UiFactory.Label("InstructionBody", banner.transform, "", 23f, UiFactory.Cream, TextAlignmentOptions.Left, false, 34f, 56f, 690f, 38f);

            // Pace pill
            var pace = CardLeft("PacePill", safe, 590f, 156f, 400f, 56f, 28f);
            UiFactory.Icon("PaceIcon", pace.transform, UiFactory.WalkerIcon, UiFactory.Gold, 18f, 12f, 32f, 32f);
            paceText = UiFactory.Label("PaceText", pace.transform, "", 22f, UiFactory.Cream, TextAlignmentOptions.Left, false, 58f, 8f, 330f, 40f);

            // XP
            var xp = UiFactory.Panel("XpDisplay", safe, topRight, topRight, new Vector2(-250f, -40f), new Vector2(222f, 82f), UiFactory.PanelGreen, null, 20f);
            UiFactory.AddOutline(xp, 20f);
            UiFactory.Icon("XpStar", xp.transform, UiFactory.StarIcon, UiFactory.Gold, 42f, 24f, 34f, 34f);
            xpText = UiFactory.Label("XpValue", xp.transform, "0 XP", 28f, UiFactory.Cream, TextAlignmentOptions.Left, false, 88f, 16f, 130f, 50f);

            // Settings and pause
            var gear = RoundButton("SettingsButton", safe, topRight, topRight, new Vector2(-144f, -40f), 90f, () => ShowOverlay(settingsOverlay, true));
            UiFactory.Icon("GearIcon", gear.transform, UiFactory.GearIcon, UiFactory.Cream, 24f, 24f, 42f, 42f);

            var pause = RoundButton("PauseButton", safe, topRight, topRight, new Vector2(-40f, -40f), 90f, () => SetPaused(true));
            UiFactory.Icon("PauseLeft", pause.transform, UiFactory.Rounded, UiFactory.Cream, 30f, 24f, 11f, 42f).preserveAspect = false;
            UiFactory.Icon("PauseRight", pause.transform, UiFactory.Rounded, UiFactory.Cream, 49f, 24f, 11f, 42f).preserveAspect = false;

            // Look at map
            var bottomRight = new Vector2(1f, 0f);
            var centre = new Vector2(0.5f, 0.5f);
            var map = RoundButton("MapButton", safe, bottomRight, centre, new Vector2(-361f, 241f), 122f, OpenMap);
            UiFactory.Label("MapLabel", map.transform, "Look at\nmap", 20f, UiFactory.Cream, TextAlignmentOptions.Center, false, 0f, 28f, 122f, 66f);

            // Contextual action
            var halo = UiFactory.CircleImage("ActionHalo", safe, bottomRight, centre, new Vector2(-161f, 255f), new Vector2(230f, 230f),
                new Color(UiFactory.Gold.r, UiFactory.Gold.g, UiFactory.Gold.b, 0.3f));
            halo.raycastTarget = false;
            var action = UiFactory.CircleImage("ActionButton", halo.transform, centre, centre, Vector2.zero, new Vector2(210f, 210f), Color.white, UiFactory.GoldCircle);
            action.gameObject.AddComponent<Button>().onClick.AddListener(() => OnAction?.Invoke());
            UiFactory.Icon("CoffeeIcon", action.transform, UiFactory.CoffeeIcon, UiFactory.DarkInk, 67f, 30f, 76f, 76f);
            UiFactory.Label("ActionLabel", action.transform, "Tap to grab\ncoffee", 26f, UiFactory.DarkInk, TextAlignmentOptions.Center, true, 15f, 108f, 180f, 72f);
            actionButton = halo.gameObject;
            actionButton.SetActive(false);

            Joystick = VirtualJoystick.Create(safe);

            // Overlays are built last so they appear on top
            pauseOverlay = BuildOverlay(root, "Paused", "Resume", () => SetPaused(false), 300f, out _);
            pauseOverlay.SetActive(false);

            settingsOverlay = BuildOverlay(root, "Settings", "Close", () => ShowOverlay(settingsOverlay, false), 400f, out RectTransform settingsBox);
            shadowLabel = AddOverlayButton(settingsBox, ShadowText(), 150f, ToggleShadows, false);
            settingsOverlay.SetActive(false);

            BuildCompletePanel(root);
        }

        void BuildCompletePanel(Transform root)
        {
            var centre = new Vector2(0.5f, 0.5f);
            var dim = UiFactory.MakeRect("CompleteOverlay", root, centre, centre, Vector2.zero, Vector2.zero);
            UiFactory.Stretch(dim);
            dim.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            var box = UiFactory.Panel("Box", dim, centre, centre, Vector2.zero, new Vector2(764f, 564f),
                new Color(0.04f, 0.17f, 0.15f, 0.97f), null, 34f);
            UiFactory.AddOutline(box, 34f);

            var eyebrow = UiFactory.Label("Eyebrow", box.transform, "ROUTE COMPLETE", 24f, UiFactory.Gold, TextAlignmentOptions.Center, false, 0f, 40f, 764f, 38f);
            eyebrow.characterSpacing = 8f;
            UiFactory.Label("Heading", box.transform, "You made it to the office", 52f, UiFactory.Cream, TextAlignmentOptions.Center, true, 0f, 80f, 764f, 72f);
            completeSummary = UiFactory.Label("Summary", box.transform, "", 24f, UiFactory.Sage, TextAlignmentOptions.Center, false, 0f, 150f, 764f, 44f);

            completeXp = BuildStatTile(box.transform, 66f, "XP");
            completeCoins = BuildStatTile(box.transform, 286f, "Coins");
            completeTime = BuildStatTile(box.transform, 506f, "Time");

            var boxRect = (RectTransform)box.transform;
            var replay = AddOverlayButton(boxRect, "Replay", 0f, Replay, false);
            PlaceButton(replay, 118f, 410f, 254f, 88f);
            var next = AddOverlayButton(boxRect, "Continue", 0f, ContinueWalking, true);
            PlaceButton(next, 396f, 412f, 250f, 84f);

            completePanel = dim.gameObject;
            completePanel.SetActive(false);
        }

        TextMeshProUGUI BuildStatTile(Transform parent, float x, string caption)
        {
            var corner = new Vector2(0f, 1f);
            var tile = UiFactory.Panel(caption + "Tile", parent, corner, corner, new Vector2(x, -230f), new Vector2(192f, 132f),
                new Color(1f, 1f, 1f, 0.07f), null, 20f);
            tile.raycastTarget = false;
            UiFactory.AddOutline(tile, 20f);
            UiFactory.Label("Caption", tile.transform, caption, 22f, UiFactory.Sage, TextAlignmentOptions.Center, false, 0f, 82f, 192f, 34f);
            return UiFactory.Label("Value", tile.transform, "", 44f, UiFactory.Gold, TextAlignmentOptions.Center, true, 0f, 18f, 192f, 62f);
        }

        static void PlaceButton(TextMeshProUGUI label, float x, float y, float width, float height)
        {
            UiFactory.Place((RectTransform)label.transform.parent, x, y, width, height);
            UiFactory.Place(label.rectTransform, 0f, 0f, width, height);
        }

        void Replay()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        void ContinueWalking()
        {
            ShowOverlay(completePanel, false);
            Joystick.gameObject.SetActive(true);
            SetInstruction("Free walk", "Explore the city at your own pace");
            SetPace("Normal pace: enjoy the walk");
        }

        GameObject BuildOverlay(Transform root, string heading, string buttonLabel, UnityAction onClick, float height, out RectTransform boxRect)
        {
            var centre = new Vector2(0.5f, 0.5f);
            var dim = UiFactory.MakeRect(heading + "Overlay", root, centre, centre, Vector2.zero, Vector2.zero);
            UiFactory.Stretch(dim);
            dim.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            var box = UiFactory.Panel("Box", dim, centre, centre, Vector2.zero, new Vector2(560f, height), UiFactory.PanelGreen, null, 32f);
            UiFactory.AddOutline(box, 32f);
            UiFactory.Label("Heading", box.transform, heading, 44f, UiFactory.Cream, TextAlignmentOptions.Center, true, 0f, 36f, 560f, 64f);

            boxRect = (RectTransform)box.transform;
            AddOverlayButton(boxRect, buttonLabel, 40f, onClick, true);
            return dim.gameObject;
        }

        TextMeshProUGUI AddOverlayButton(RectTransform box, string text, float bottom, UnityAction onClick, bool gold)
        {
            var anchor = new Vector2(0.5f, 0f);
            var button = UiFactory.Panel("Button", box, anchor, anchor, new Vector2(0f, bottom), new Vector2(300f, 88f),
                gold ? Color.white : UiFactory.PanelGreen, gold ? UiFactory.GoldPill : null, 44f);
            if (gold) button.pixelsPerUnitMultiplier = 64f / 44f;
            else UiFactory.AddOutline(button, 44f);
            button.gameObject.AddComponent<Button>().onClick.AddListener(onClick);

            var label = UiFactory.Label("ButtonLabel", button.transform, text, 28f, gold ? UiFactory.DarkInk : UiFactory.Cream,
                TextAlignmentOptions.Center, true, 0f, 0f, 300f, 88f);
            return label;
        }

        // ---- Map ----------------------------------------------------------------

        public void SetupMap(float roadPitch)
        {
            const int blocks = 5;
            var centre = new Vector2(0.5f, 0.5f);
            var root = (Transform)transform;

            var dim = UiFactory.MakeRect("MapOverlay", root, centre, centre, Vector2.zero, Vector2.zero);
            UiFactory.Stretch(dim);
            dim.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            dim.gameObject.AddComponent<Button>().onClick.AddListener(() => ShowOverlay(mapOverlay, false));

            var box = UiFactory.Panel("MapBox", dim, centre, centre, Vector2.zero, new Vector2(640f, 640f), UiFactory.PanelGreen, null, 32f);
            box.raycastTarget = false;
            UiFactory.AddOutline(box, 32f);

            mapScale = 290f / (blocks * 0.5f * roadPitch);
            float blockSize = (roadPitch - 10f) * mapScale;
            for (int x = 0; x < blocks; x++)
            {
                for (int z = 0; z < blocks; z++)
                {
                    bool park = x == blocks / 2 && z == blocks / 2;
                    var position = new Vector2((x - blocks / 2) * roadPitch, (z - blocks / 2) * roadPitch) * mapScale;
                    var block = UiFactory.Panel("Block", box.transform, centre, centre, position, new Vector2(blockSize, blockSize),
                        park ? new Color(0.33f, 0.58f, 0.33f) : new Color(0.20f, 0.33f, 0.30f), null, 8f);
                    block.raycastTarget = false;
                }
            }

            var target = UiFactory.CircleImage("MapTarget", box.transform, centre, centre, Vector2.zero, new Vector2(30f, 30f), UiFactory.Gold);
            target.raycastTarget = false;
            mapTarget = target.rectTransform;
            var player = UiFactory.CircleImage("MapPlayer", box.transform, centre, centre, Vector2.zero, new Vector2(22f, 22f), UiFactory.Cream);
            player.raycastTarget = false;
            mapPlayer = player.rectTransform;

            mapOverlay = dim.gameObject;
            mapOverlay.SetActive(false);
        }

        public void SetMapPositions(Vector3 player, Vector3 target)
        {
            lastPlayerPosition = player;
            lastTargetPosition = target;
        }

        void OpenMap()
        {
            if (mapOverlay == null) return;
            mapPlayer.anchoredPosition = new Vector2(lastPlayerPosition.x, lastPlayerPosition.z) * mapScale;
            mapTarget.anchoredPosition = new Vector2(lastTargetPosition.x, lastTargetPosition.z) * mapScale;
            ShowOverlay(mapOverlay, true);
        }

        // ---- Settings -----------------------------------------------------------

        static string ShadowText()
        {
            bool on = RenderSettings.sun == null || RenderSettings.sun.shadows != LightShadows.None;
            return on ? "Shadows: On" : "Shadows: Off";
        }

        void ToggleShadows()
        {
            var sun = RenderSettings.sun;
            if (sun == null) return;
            sun.shadows = sun.shadows == LightShadows.None ? LightShadows.Soft : LightShadows.None;
            shadowLabel.text = ShadowText();
        }

        // ---- Public API -----------------------------------------------------------

        static void ShowOverlay(GameObject overlay, bool visible)
        {
            overlay.SetActive(visible);
            Time.timeScale = visible ? 0f : 1f;
        }

        public void SetInstruction(string title, string body)
        {
            instructionTitle.text = title;
            instructionBody.text = body;
        }

        public void SetCounter(int current, int total) => counterText.text = $"{current}/{total}";

        public void SetWarning(bool on)
        {
            instructionTitle.color = on ? UiFactory.Coral : UiFactory.Gold;
            bannerStrip.color = on ? UiFactory.Coral : UiFactory.Gold;
        }

        public void SetPace(string message) => paceText.text = message;

        public void SetXp(int xp) => xpText.text = $"{xp} XP";

        public void SetActionVisible(bool visible) => actionButton.SetActive(visible);

        public void SetPaused(bool paused) => ShowOverlay(pauseOverlay, paused);

        public void ShowComplete(int laps, int wrongTurns, int xp, int coins, float seconds)
        {
            string turnWord = wrongTurns == 1 ? "wrong turn" : "wrong turns";
            int totalSeconds = Mathf.FloorToInt(seconds);

            completeSummary.text = $"{laps} laps · coffee collected · {wrongTurns} {turnWord}";
            completeXp.text = $"+{xp}";
            completeCoins.text = $"+{coins}";
            completeTime.text = $"{totalSeconds / 60}:{totalSeconds % 60:00}";

            actionButton.SetActive(false);
            Joystick.gameObject.SetActive(false);
            ShowOverlay(completePanel, true);
        }

        void OnDestroy() => Time.timeScale = 1f;
    }
}