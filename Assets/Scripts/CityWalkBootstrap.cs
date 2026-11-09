using UnityEngine;

namespace CityWalk
{
    /// <summary>
    /// Entry point. Put this on one empty GameObject in an empty scene and press Play.
    /// It builds the lighting, city, player, camera, HUD and route in code.
    /// </summary>
    public class CityWalkBootstrap : MonoBehaviour
    {
        public int citySeed = 7;

        void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;

            SetupLighting();

            var city = new GameObject("City").AddComponent<CityBuilder>();
            city.randomSeed = citySeed;
            city.Build();
            float p = city.Pitch;

            // Player
            var playerGo = new GameObject("Player");
            playerGo.layer = 2;
            var cc = playerGo.AddComponent<CharacterController>();
            cc.height = 1.6f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.35f;
            var mover = playerGo.AddComponent<PlayerMover>();
            var characterPrefab = Resources.Load<GameObject>("PlayerCharacter");
            var character = Instantiate(characterPrefab, playerGo.transform);
            character.transform.localPosition = Vector3.zero;
            character.transform.localRotation = Quaternion.identity;
            mover.animator = character.GetComponentInChildren<Animator>();

            // Camera
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.62f, 0.76f, 0.90f);
            cam.fieldOfView = 55f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 400f;
            var follow = camGo.AddComponent<FollowCamera>();
            follow.target = playerGo.transform;
            follow.player = mover;
            mover.cameraTransform = camGo.transform;

            // HUD
            var hud = BasicHud.Create();
            mover.joystick = hud.Joystick;

            // Spawn on the road just south of the park, facing east
            mover.Teleport(new Vector3(-p * 0.5f + 4f, 0.05f, -p * 0.5f), 90f);
            follow.Snap();

            // Route
            var route = new GameObject("Route").AddComponent<LapRoute>();   // here
            route.Init(playerGo.transform, hud, p, new[]
            {
                new LapRoute.Stop { title = "Grab a coffee", hint = "The coffee kiosk is ahead. Tap it as you pass.", position = new Vector3(p * 0.5f, 0f, -p * 0.5f), requiresTap = true },
                new LapRoute.Stop { title = "Stop by the bank", hint = "Head north up the avenue", position = new Vector3(p * 0.5f, 0f, p * 1.5f) },            
                new LapRoute.Stop { title = "Get to the office", hint = "Your office is to the west", position = new Vector3(-p * 1.5f, 0f, p * 1.5f) },
            });

        }

        static void SetupLighting()
        {
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.2f;
            sun.color = new Color(1f, 0.96f, 0.88f);
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            RenderSettings.sun = sun;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.70f, 0.82f);
            RenderSettings.ambientEquatorColor = new Color(0.50f, 0.52f, 0.55f);
            RenderSettings.ambientGroundColor = new Color(0.28f, 0.27f, 0.26f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.62f, 0.76f, 0.90f);
            RenderSettings.fogStartDistance = 60f;
            RenderSettings.fogEndDistance = 220f;
        }
    }
}
