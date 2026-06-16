using System;
using UnityEngine;

namespace AutomaticParking
{
    [Serializable]
    public sealed class ParkingScenarioStep
    {
        public ParkingState state = ParkingState.Positioning;
        public Transform target;
        public bool reverse;
        public float speed = 2.0f;
        public float turnSpeed = 100.0f;
        public float arriveDistance = 0.25f;
        public float angleTolerance = 4.0f;
        public float timeout = 12.0f;
        public bool smoothArc = true;
        public float curveHandle = 2.2f;
    }

    public sealed class ParkingScenario : MonoBehaviour
    {
        public ParkingType parkingType = ParkingType.Parallel;
        public Transform scanAcceptPoint;
        public float scanSpeed = 2.2f;
        public float scanTurnSpeed = 80.0f;
        public float scanArriveDistance = 0.35f;
        public float validatePause = 0.35f;
        public bool useRaycastEmergencyStop;
        public ParkingScenarioStep[] steps = new ParkingScenarioStep[0];

        [Header("Reported detected spot")]
        public Vector3 spotStart;
        public Vector3 spotEnd;
        public float spotWidth = 2.8f;

        public ParkingSpotCandidate BuildCandidate()
        {
            return new ParkingSpotCandidate
            {
                startPosition = spotStart,
                endPosition = spotEnd,
                length = Vector3.Distance(spotStart, spotEnd),
                width = spotWidth,
                isValid = true,
                parkingType = parkingType
            };
        }
    }
}
