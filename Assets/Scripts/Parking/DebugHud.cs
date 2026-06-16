using UnityEngine;
using UnityEngine.UI;

namespace AutomaticParking
{
    public sealed class DebugHud : MonoBehaviour
    {
        public ParkingStateMachine stateMachine;
        public ParkingCarController car;
        public ParkingSensors sensors;
        public Text label;

        private void Update()
        {
            if (label == null || stateMachine == null || car == null || sensors == null)
            {
                return;
            }

            SensorReadings readings = sensors.LastReadings;
            ParkingSpotCandidate spot = sensors.LastCandidate;

            label.text =
                $"FSM: {stateMachine.CurrentState}\n" +
                $"Speed: {car.CurrentSpeed:0.00} m/s | Steering: {car.CurrentSteeringAngle:0.0} deg\n" +
                $"Front: {readings.front:0.0}  Rear: {readings.rear:0.0}\n" +
                $"Right F/M/R: {readings.rightFront:0.0} / {readings.rightMiddle:0.0} / {readings.rightRear:0.0}\n" +
                $"Gap tracking: {sensors.IsTrackingGap}\n" +
                $"Candidate: valid={spot.isValid}, length={spot.length:0.0}, type={spot.parkingType}";
        }
    }
}
