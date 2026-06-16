using UnityEngine;

namespace AutomaticParking
{
    public sealed class MovingObstacle : MonoBehaviour
    {
        public Transform pointA;
        public Transform pointB;
        public float speed = 1.4f;
        public float waitAtEnds = 0.8f;
        public bool waitForParkingManeuver;
        public ParkingStateMachine observedCar;
        public ParkingState triggerState = ParkingState.ReverseTurn;
        public float triggerDelay = 0.5f;
        public bool blockObservedCarUntilTarget;
        public float emergencyDistance = 4.6f;
        public bool stopAfterPointB;

        private Transform target;
        private float waitTimer;
        private bool activated;
        private float activeTimer;
        private bool reachedBlockingTarget;

        private void Start()
        {
            target = pointB;
            activated = !waitForParkingManeuver;
        }

        private void Update()
        {
            if (pointA == null || pointB == null)
            {
                return;
            }

            if (!activated)
            {
                if (observedCar == null || (int)observedCar.CurrentState < (int)triggerState)
                {
                    return;
                }

                activeTimer += Time.deltaTime;
                if (activeTimer < triggerDelay)
                {
                    return;
                }

                activated = true;
            }

            if (waitTimer > 0.0f)
            {
                waitTimer -= Time.deltaTime;
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            Vector3 direction = target.position - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }

            if (Vector3.Distance(transform.position, target.position) < 0.05f)
            {
                if (blockObservedCarUntilTarget && target == pointB)
                {
                    reachedBlockingTarget = true;
                    if (stopAfterPointB)
                    {
                        enabled = false;
                        return;
                    }
                }

                target = target == pointA ? pointB : pointA;
                waitTimer = waitAtEnds;
            }

            if (observedCar == null)
            {
                return;
            }

            if (blockObservedCarUntilTarget && !reachedBlockingTarget)
            {
                observedCar.RequestEmergencyStop(0.75f);
                return;
            }

            if (Vector3.Distance(transform.position, observedCar.transform.position) < emergencyDistance)
            {
                observedCar.RequestEmergencyStop(0.65f);
            }
        }
    }
}
