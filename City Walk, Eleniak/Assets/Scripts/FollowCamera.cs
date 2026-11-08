using UnityEngine;

namespace CityWalk
{
    /// <summary>
    /// Behind-the-back camera. Pulls in quickly when something blocks the view
    /// and eases back out when the space clears.
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        public Transform target;
        public PlayerMover player;
        public float distance = 4.2f;
        public float height = 1.9f;
        public float lookHeight = 1.5f;
        public float pitch = 12f;
        public float yawFollowSpeed = 2.5f;
        public float positionSmoothing = 10f;

        [Header("Collision")]
        public float collisionRadius = 0.25f;
        public float minDistance = 0.9f;
        public float pullInSpeed = 25f;
        public float easeOutSpeed = 4f;
        public LayerMask collisionMask = ~(1 << 2); // everything except Ignore Raycast

        float yaw;
        float currentDistance;
        Vector3 smoothedTarget;

        public void Snap()
        {
            if (target == null) return;
            yaw = target.eulerAngles.y;
            smoothedTarget = target.position;
            currentDistance = float.MaxValue;
            ApplyPosition(0f);
        }

        void LateUpdate()
        {
            if (target == null) return;

            bool moving = player != null && player.CurrentSpeed > 0.2f;
            if (moving)
                yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, yawFollowSpeed * Time.deltaTime);

            smoothedTarget = Vector3.Lerp(smoothedTarget, target.position, positionSmoothing * Time.deltaTime);
            ApplyPosition(Time.deltaTime);
        }

        void ApplyPosition(float deltaTime)
        {
            Vector3 origin = smoothedTarget + Vector3.up * lookHeight;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desired = smoothedTarget + Vector3.up * height + rotation * new Vector3(0f, 0f, -distance);

            Vector3 offset = desired - origin;
            float fullDistance = offset.magnitude;
            Vector3 direction = offset / fullDistance;

            float allowedDistance = fullDistance;
            if (Physics.SphereCast(origin, collisionRadius, direction, out RaycastHit hit, fullDistance,
                    collisionMask, QueryTriggerInteraction.Ignore))
            {
                allowedDistance = Mathf.Max(hit.distance, minDistance);
            }

            if (deltaTime <= 0f || currentDistance > fullDistance)
            {
                currentDistance = allowedDistance;
            }
            else
            {
                float speed = allowedDistance < currentDistance ? pullInSpeed : easeOutSpeed;
                currentDistance = Mathf.Lerp(currentDistance, allowedDistance, Mathf.Clamp01(speed * deltaTime));
            }

            transform.position = origin + direction * currentDistance;
            transform.LookAt(origin);
        }
    }
}