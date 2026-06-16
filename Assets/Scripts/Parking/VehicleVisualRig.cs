using UnityEngine;

namespace AutomaticParking
{
    public sealed class VehicleVisualRig : MonoBehaviour
    {
        public float wheelRadius = 0.24f;
        public float maxVisualSteer = 32.0f;

        private Transform[] wheels;
        private Transform[] frontWheels;
        private ParkingCarController controller;
        private Vector3 lastPosition;
        private float rollDegrees;

        private void Awake()
        {
            controller = GetComponent<ParkingCarController>();
            Transform[] allChildren = GetComponentsInChildren<Transform>();
            int wheelCount = 0;
            int frontCount = 0;

            foreach (Transform child in allChildren)
            {
                if (child.name.StartsWith("Wheel"))
                {
                    wheelCount++;
                    if (child.localPosition.z > 0.0f)
                    {
                        frontCount++;
                    }
                }
            }

            wheels = new Transform[wheelCount];
            frontWheels = new Transform[frontCount];
            int wi = 0;
            int fi = 0;
            foreach (Transform child in allChildren)
            {
                if (!child.name.StartsWith("Wheel"))
                {
                    continue;
                }

                wheels[wi++] = child;
                if (child.localPosition.z > 0.0f)
                {
                    frontWheels[fi++] = child;
                }
            }

            lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (controller == null)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;
            if (wheelRadius > 0.01f)
            {
                float direction = Mathf.Sign(controller.CurrentSpeed);
                if (Mathf.Abs(controller.CurrentSpeed) < 0.01f)
                {
                    direction = 0.0f;
                }

                rollDegrees = Mathf.Repeat(
                    rollDegrees + direction * distance / (2.0f * Mathf.PI * wheelRadius) * 360.0f,
                    360.0f);
            }

            float steer = Mathf.Clamp(controller.CurrentSteeringAngle, -maxVisualSteer, maxVisualSteer);

            foreach (Transform wheel in wheels)
            {
                float steerYaw = IsFrontWheel(wheel) ? steer : 0.0f;
                wheel.localRotation = Quaternion.Euler(0.0f, steerYaw, 0.0f)
                    * Quaternion.Euler(0.0f, 0.0f, 90.0f)
                    * Quaternion.Euler(0.0f, rollDegrees, 0.0f);
            }
        }

        private bool IsFrontWheel(Transform wheel)
        {
            for (int i = 0; i < frontWheels.Length; i++)
            {
                if (frontWheels[i] == wheel)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
