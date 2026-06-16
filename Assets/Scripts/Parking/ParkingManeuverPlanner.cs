using UnityEngine;

namespace AutomaticParking
{
    public sealed class ParkingManeuverPlanner : MonoBehaviour
    {
        [Header("Parallel timing")]
        public float parallelPositionTime = 1.1f;
        public float parallelReverseTurnTime = 2.4f;
        public float parallelCounterTurnTime = 2.2f;
        public float parallelStraightenTime = 1.2f;

        [Header("Perpendicular timing")]
        public float perpendicularPositionTime = 0.9f;
        public float perpendicularTurnTime = 2.7f;
        public float perpendicularStraightenTime = 1.2f;

        public float GetDuration(ParkingType type, ParkingState state)
        {
            if (type == ParkingType.Perpendicular)
            {
                return state switch
                {
                    ParkingState.Positioning => perpendicularPositionTime,
                    ParkingState.ReverseTurn => perpendicularTurnTime,
                    ParkingState.CounterTurn => perpendicularStraightenTime,
                    ParkingState.Straighten => 0.7f,
                    _ => 0.0f
                };
            }

            return state switch
            {
                ParkingState.Positioning => parallelPositionTime,
                ParkingState.ReverseTurn => parallelReverseTurnTime,
                ParkingState.CounterTurn => parallelCounterTurnTime,
                ParkingState.Straighten => parallelStraightenTime,
                _ => 0.0f
            };
        }

        public void ApplyControls(ParkingCarController car, ParkingType type, ParkingState state)
        {
            if (type == ParkingType.Perpendicular)
            {
                ApplyPerpendicular(car, state);
                return;
            }

            ApplyParallel(car, state);
        }

        private static void ApplyParallel(ParkingCarController car, ParkingState state)
        {
            switch (state)
            {
                case ParkingState.Positioning:
                    car.SetControls(0.38f, 0.0f, 0.0f);
                    break;
                case ParkingState.ReverseTurn:
                    car.SetControls(-0.55f, 1.0f, 0.0f);
                    break;
                case ParkingState.CounterTurn:
                    car.SetControls(-0.48f, -1.0f, 0.0f);
                    break;
                case ParkingState.Straighten:
                    car.SetControls(0.25f, 0.0f, 0.0f);
                    break;
                default:
                    car.SetControls(0.0f, 0.0f, 1.0f);
                    break;
            }
        }

        private static void ApplyPerpendicular(ParkingCarController car, ParkingState state)
        {
            switch (state)
            {
                case ParkingState.Positioning:
                    car.SetControls(0.35f, 0.0f, 0.0f);
                    break;
                case ParkingState.ReverseTurn:
                    car.SetControls(0.50f, 0.95f, 0.0f);
                    break;
                case ParkingState.CounterTurn:
                    car.SetControls(0.28f, -0.50f, 0.0f);
                    break;
                case ParkingState.Straighten:
                    car.SetControls(0.18f, 0.0f, 0.0f);
                    break;
                default:
                    car.SetControls(0.0f, 0.0f, 1.0f);
                    break;
            }
        }
    }
}
