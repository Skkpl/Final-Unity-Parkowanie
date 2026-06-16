using UnityEngine;

namespace AutomaticParking
{
    public sealed class ParkingCarController : MonoBehaviour
    {
        [Header("Vehicle dimensions")]
        public float wheelBase = 2.7f;
        public float carLength = 4.5f;
        public float carWidth = 2.0f;

        [Header("Motion")]
        public float maxForwardSpeed = 4.0f;
        public float maxReverseSpeed = 2.8f;
        public float acceleration = 4.0f;
        public float brakeAcceleration = 9.0f;
        public float maxSteeringAngle = 35.0f;
        public float steeringRate = 90.0f;

        public float CurrentSpeed { get; private set; }
        public float CurrentSteeringAngle { get; private set; }
        public bool UseExternalPoseControl { get; set; }

        private float targetThrottle;
        private float targetSteering;
        private float targetBrake;

        public void SetControls(float throttle, float steering, float brake)
        {
            targetThrottle = Mathf.Clamp(throttle, -1.0f, 1.0f);
            targetSteering = Mathf.Clamp(steering, -1.0f, 1.0f);
            targetBrake = Mathf.Clamp01(brake);
        }

        public void StopImmediately()
        {
            CurrentSpeed = 0.0f;
            SetControls(0.0f, 0.0f, 1.0f);
        }

        private void FixedUpdate()
        {
            if (UseExternalPoseControl)
            {
                return;
            }

            float dt = Time.fixedDeltaTime;
            float desiredSpeed = targetThrottle >= 0.0f
                ? targetThrottle * maxForwardSpeed
                : targetThrottle * maxReverseSpeed;

            float speedStep = targetBrake > 0.01f ? brakeAcceleration * dt : acceleration * dt;
            CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, desiredSpeed, speedStep);
            if (targetBrake > 0.01f)
            {
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0.0f, brakeAcceleration * targetBrake * dt);
            }

            float desiredSteeringAngle = targetSteering * maxSteeringAngle;
            CurrentSteeringAngle = Mathf.MoveTowards(
                CurrentSteeringAngle,
                desiredSteeringAngle,
                steeringRate * dt);

            if (Mathf.Abs(CurrentSpeed) > 0.01f)
            {
                float steeringRadians = CurrentSteeringAngle * Mathf.Deg2Rad;
                float yawRate = CurrentSpeed / Mathf.Max(0.1f, wheelBase) * Mathf.Tan(steeringRadians);
                transform.Rotate(Vector3.up, yawRate * Mathf.Rad2Deg * dt, Space.World);
            }

            transform.position += transform.forward * (CurrentSpeed * dt);
        }

        public bool MoveToPose(
            Vector3 targetPosition,
            Quaternion targetRotation,
            bool reverse,
            float speed,
            float turnSpeed,
            float positionTolerance,
            float angleTolerance)
        {
            float dt = Time.fixedDeltaTime;
            Vector3 current = transform.position;
            Vector3 planarTarget = new Vector3(targetPosition.x, current.y, targetPosition.z);
            Quaternion planarTargetRotation = Quaternion.Euler(0.0f, targetRotation.eulerAngles.y, 0.0f);
            Vector3 toTarget = planarTarget - current;
            float distance = toTarget.magnitude;

            if (distance > Mathf.Max(0.03f, positionTolerance))
            {
                float headingError = Vector3.SignedAngle(transform.forward, planarTargetRotation * Vector3.forward, Vector3.up);

                CurrentSteeringAngle = Mathf.MoveTowards(
                    CurrentSteeringAngle,
                    Mathf.Clamp(headingError, -maxSteeringAngle, maxSteeringAngle),
                    Mathf.Max(steeringRate, turnSpeed) * dt);

                float signedSpeed = (reverse ? -1.0f : 1.0f) * Mathf.Max(0.15f, speed);
                transform.position = Vector3.MoveTowards(current, planarTarget, Mathf.Abs(signedSpeed) * dt);
                transform.rotation = Quaternion.RotateTowards(FlattenYaw(transform.rotation), planarTargetRotation, turnSpeed * dt);
                CurrentSpeed = signedSpeed;
                return false;
            }

            transform.position = planarTarget;
            transform.rotation = planarTargetRotation;
            CurrentSpeed = 0.0f;
            CurrentSteeringAngle = 0.0f;
            return true;
        }

        public void SetPoseAlongPath(Vector3 position, Quaternion rotation, float signedSpeed, float steeringHint)
        {
            transform.position = new Vector3(position.x, transform.position.y, position.z);
            transform.rotation = FlattenYaw(rotation);
            CurrentSpeed = signedSpeed;
            CurrentSteeringAngle = Mathf.Clamp(steeringHint, -maxSteeringAngle, maxSteeringAngle);
        }

        private static Quaternion FlattenYaw(Quaternion rotation)
        {
            return Quaternion.Euler(0.0f, rotation.eulerAngles.y, 0.0f);
        }
    }
}
