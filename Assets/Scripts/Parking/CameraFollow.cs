using UnityEngine;

namespace AutomaticParking
{
    public sealed class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(-7.5f, 8.5f, -9.0f);
        public float followSharpness = 6.0f;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1.0f - Mathf.Exp(-followSharpness * Time.deltaTime));
            transform.LookAt(target.position + Vector3.up * 0.8f);
        }
    }
}
