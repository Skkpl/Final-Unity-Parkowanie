using System;
using UnityEngine;

namespace AutomaticParking
{
    public enum ParkingState
    {
        Scan,
        ValidateSpot,
        Positioning,
        ReverseTurn,
        CounterTurn,
        Straighten,
        Parked,
        EmergencyStop
    }

    public enum ParkingType
    {
        Perpendicular,
        Parallel
    }

    [Serializable]
    public struct SensorReadings
    {
        public float front;
        public float rear;
        public float rightFront;
        public float rightMiddle;
        public float rightRear;
        public float leftFront;
        public float leftMiddle;
        public float leftRear;
    }

    [Serializable]
    public struct ParkingSpotCandidate
    {
        public Vector3 startPosition;
        public Vector3 endPosition;
        public float length;
        public float width;
        public bool isValid;
        public ParkingType parkingType;

        public Vector3 Center => (startPosition + endPosition) * 0.5f;
    }
}
