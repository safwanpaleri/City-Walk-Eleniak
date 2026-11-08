using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CityWalk
{
    /// <summary>
    /// Camera-relative movement using a CharacterController.
    /// Input comes from the on-screen joystick, or WASD / arrow keys in the editor.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMover : MonoBehaviour
    {
        public float walkSpeed = 3.2f;
        public float runSpeed = 5.5f;
        [Range(0f, 1f)] public float runThreshold = 0.9f;
        public float acceleration = 10f;
        public float turnSpeed = 540f;
        public float gravity = -20f;

        public VirtualJoystick joystick;
        public Transform cameraTransform;
        public SuitedManAvatar avatar;
        public Animator animator;
        static readonly int SpeedParam = Animator.StringToHash("Speed");

        public bool MovementLocked { get; set; }
        public float CurrentSpeed { get; private set; }

        CharacterController controller;
        Vector3 horizontalVelocity;
        float verticalVelocity;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            Vector2 input = MovementLocked ? Vector2.zero : ReadInput();
            float strength = Mathf.Clamp01(input.magnitude);
            if (strength >= 0.05f && RunHeld()) strength = 1f;

            Vector3 forward = Vector3.forward, right = Vector3.right;
            if (cameraTransform != null)
            {
                forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            }

            Vector3 moveDir = (forward * input.y + right * input.x);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            float targetSpeed = strength < 0.05f ? 0f : strength <= 0.5f ? Mathf.Lerp(0f, walkSpeed, strength / 0.5f) : Mathf.Lerp(walkSpeed, runSpeed, (strength - 0.5f) / 0.5f);
            Vector3 targetVelocity = moveDir.sqrMagnitude > 0.0001f ? moveDir.normalized * targetSpeed : Vector3.zero;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;

            controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);

            Vector3 flat = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
            CurrentSpeed = flat.magnitude;
            if (CurrentSpeed > 0.1f)
            {
                var targetRot = Quaternion.LookRotation(flat.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }

            if (avatar != null) avatar.Speed = CurrentSpeed;
            if (animator != null)
                animator.SetFloat(SpeedParam, strength < 0.05f ? 0f : strength, 0.1f, Time.deltaTime);
        }

        Vector2 ReadInput()
        {
            Vector2 v = joystick != null ? joystick.Value : Vector2.zero;
            if (v.sqrMagnitude > 0.0001f) return v;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                float x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
                float y = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
                v = Vector2.ClampMagnitude(new Vector2(x, y), 1f) * 0.6f;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            v = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f) * 0.6f;
#endif
            return v;
        }

        bool RunHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.LeftShift);
#else
            return false;
#endif
        }

        public void Teleport(Vector3 position, float yaw)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            controller.enabled = true;
            horizontalVelocity = Vector3.zero;
        }
    }
}
