using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CityWalk
{
    /// <summary>
    /// The starter HUD: title card, stop counter, instruction banner, joystick and a pause button.
    /// Deliberately plain. Restyling it to the target design is part of the task.
    /// </summary>
    public class BasicHud : MonoBehaviour
    {
        static readonly Color Card = new Color(1f, 1f, 1f, 0.92f);
        static readonly Color Ink = new Color(0.12f, 0.13f, 0.15f);
        static readonly Color SubInk = new Color(0.35f, 0.37f, 0.40f);

        public VirtualJoystick Joystick { get; private set; }

        Text counterText, instructionTitle, instructionBody, paceText;
        GameObject pauseOverlay, completePanel;

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
            scaler.matchWidthOrHeight = 0.6f;

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

        void Build(Transform root)
        {
            var topLeft = new Vector2(0f, 1f);

            // Title card
            var title = UiFactory.Panel("TitleCard", root, topLeft, topLeft, new Vector2(40f, -36f), new Vector2(380f, 110f), Card);
            var t1 = UiFactory.Label("Title", title.transform, "City Walk", 36, Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            t1.rectTransform.offsetMin = new Vector2(24f, 10f);
            t1.rectTransform.offsetMax = new Vector2(-16f, -14f);
            var t2 = UiFactory.Label("Subtitle", title.transform, "Get to the office on time", 24, SubInk, TextAnchor.LowerLeft);
            t2.rectTransform.offsetMin = new Vector2(24f, 16f);

            // Stop counter
            var counter = UiFactory.Panel("Counter", root, topLeft, topLeft, new Vector2(40f, -156f), new Vector2(130f, 110f), Card);
            UiFactory.Label("CounterLabel", counter.transform, "Lap", 22, SubInk, TextAnchor.UpperCenter); 
            counterText = UiFactory.Label("CounterValue", counter.transform, "0/3", 38, Ink, TextAnchor.LowerCenter, FontStyle.Bold);

            // Instruction banner
            var banner = UiFactory.Panel("InstructionBanner", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -36f), new Vector2(720f, 110f), Card);
            instructionTitle = UiFactory.Label("InstructionTitle", banner.transform, "", 32, Ink, TextAnchor.UpperCenter, FontStyle.Bold);
            instructionTitle.rectTransform.offsetMax = new Vector2(-16f, -14f);
            instructionBody = UiFactory.Label("InstructionBody", banner.transform, "", 24, SubInk, TextAnchor.LowerCenter);
            instructionBody.rectTransform.offsetMin = new Vector2(16f, 14f);

            // Pace pill
            var pace = UiFactory.Panel("PacePill", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -156f), new Vector2(460f, 56f), Card);
            paceText = UiFactory.Label("PaceText", pace.transform, "", 24, Ink, TextAnchor.MiddleCenter);

            // Pause button
            var pauseBtn = UiFactory.Panel("PauseButton", root, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-40f, -36f), new Vector2(96f, 96f), Card, UiFactory.Circle);
            UiFactory.Label("PauseIcon", pauseBtn.transform, "II", 34, Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            pauseBtn.gameObject.AddComponent<Button>().onClick.AddListener(() => SetPaused(true));

            // Pause overlay
            pauseOverlay = BuildOverlay(root, "Paused", "Resume", () => SetPaused(false));
            pauseOverlay.SetActive(false);

            // Completion panel
            completePanel = BuildOverlay(root, "You made it!", "Play again",
                () => UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex));
            completePanel.SetActive(false);

            Joystick = VirtualJoystick.Create(root);
        }

        GameObject BuildOverlay(Transform root, string heading, string buttonLabel, UnityEngine.Events.UnityAction onClick)
        {
            var dim = UiFactory.MakeRect(heading + "Overlay", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;
            dim.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            var box = UiFactory.Panel("Box", dim, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 300f), Card);
            var h = UiFactory.Label("Heading", box.transform, heading, 44, Ink, TextAnchor.UpperCenter, FontStyle.Bold);
            h.rectTransform.offsetMax = new Vector2(-16f, -40f);

            var btn = UiFactory.Panel("Button", box.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 40f), new Vector2(280f, 90f), Ink);
            UiFactory.Label("ButtonLabel", btn.transform, buttonLabel, 30, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            btn.gameObject.AddComponent<Button>().onClick.AddListener(onClick);
            return dim.gameObject;
        }

        public void SetInstruction(string title, string body)
        {
            instructionTitle.text = title;
            instructionBody.text = body;
        }

        public void SetCounter(int done, int total) => counterText.text = $"{done}/{total}";

        public void SetWarning(bool on) => instructionTitle.color = on ? new Color(0.85f, 0.15f, 0.12f) : Ink;

        public void SetPace(string message) => paceText.text = message;

        public void SetPaused(bool paused)
        {
            Time.timeScale = paused ? 0f : 1f;
            pauseOverlay.SetActive(paused);
        }

        public void ShowComplete()
        {
            completePanel.SetActive(true);
            Joystick.gameObject.SetActive(false);
        }

        void OnDestroy() => Time.timeScale = 1f;
    }
}
