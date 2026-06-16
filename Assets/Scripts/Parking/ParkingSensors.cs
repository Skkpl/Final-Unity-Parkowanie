using UnityEngine;

namespace AutomaticParking
{
    public sealed class ParkingSensors : MonoBehaviour
    {
        [Header("Ray sensors")]
        public float rayHeight = 0.55f;
        public float forwardRange = 5.0f;
        public float sideRange = 7.0f;
        public LayerMask obstacleMask = ~0;

        [Header("Spot validation")]
        public ParkingType parkingType = ParkingType.Parallel;
        public float parallelRequiredLength = 6.3f;
        public float perpendicularRequiredWidth = 2.8f;
        public float freeSideThreshold = 4.2f;
        public float scanStartDistance = 2.5f;
        public float boxValidationDepth = 5.5f;
        public float boxValidationHeight = 1.8f;

        public SensorReadings LastReadings { get; private set; }
        public ParkingSpotCandidate LastCandidate { get; private set; }
        public bool IsTrackingGap { get; private set; }

        private bool gapOpen;
        private Vector3 gapStart;
        private Vector3 lastGapEnd;

        public SensorReadings GetReadings()
        {
            Transform t = transform;
            Vector3 basePosition = t.position + Vector3.up * rayHeight;

            SensorReadings readings = new SensorReadings
            {
                front = CastDistance(basePosition + t.forward * 1.9f, t.forward, forwardRange),
                rear = CastDistance(basePosition - t.forward * 1.9f, -t.forward, forwardRange),
                rightFront = CastDistance(basePosition + t.forward * 1.5f, t.right, sideRange),
                rightMiddle = CastDistance(basePosition, t.right, sideRange),
                rightRear = CastDistance(basePosition - t.forward * 1.5f, t.right, sideRange),
                leftFront = CastDistance(basePosition + t.forward * 1.5f, -t.right, sideRange),
                leftMiddle = CastDistance(basePosition, -t.right, sideRange),
                leftRear = CastDistance(basePosition - t.forward * 1.5f, -t.right, sideRange)
            };

            LastReadings = readings;
            return readings;
        }

        public bool TryFindSpot(out ParkingSpotCandidate spot)
        {
            SensorReadings readings = GetReadings();
            bool rightSideLooksFree = readings.rightFront > freeSideThreshold
                && readings.rightMiddle > freeSideThreshold
                && readings.rightRear > freeSideThreshold;

            if (rightSideLooksFree && !gapOpen)
            {
                gapOpen = true;
                gapStart = transform.position;
            }

            if (gapOpen)
            {
                lastGapEnd = transform.position;
            }

            float currentGapLength = gapOpen ? Vector3.Distance(gapStart, lastGapEnd) : 0.0f;
            float requiredLength = parkingType == ParkingType.Parallel
                ? parallelRequiredLength
                : perpendicularRequiredWidth;

            bool longEnough = currentGapLength >= requiredLength;
            bool volumeClear = longEnough && ValidateGapVolume(gapStart, lastGapEnd);
            IsTrackingGap = gapOpen;

            if (longEnough && volumeClear)
            {
                spot = new ParkingSpotCandidate
                {
                    startPosition = gapStart,
                    endPosition = lastGapEnd,
                    length = currentGapLength,
                    width = freeSideThreshold,
                    isValid = true,
                    parkingType = parkingType
                };

                LastCandidate = spot;
                return true;
            }

            if (!rightSideLooksFree && gapOpen)
            {
                gapOpen = false;
            }

            spot = default;
            return false;
        }

        public bool HasEmergencyObstacle()
        {
            SensorReadings readings = GetReadings();
            return readings.front < 1.55f || readings.rear < 1.25f;
        }

        public void PublishCandidate(ParkingSpotCandidate candidate)
        {
            LastCandidate = candidate;
        }

        public void ResetScan()
        {
            gapOpen = false;
            IsTrackingGap = false;
            LastCandidate = default;
        }

        private float CastDistance(Vector3 origin, Vector3 direction, float range)
        {
            bool hit = Physics.Raycast(origin, direction, out RaycastHit hitInfo, range, obstacleMask, QueryTriggerInteraction.Ignore);
            Debug.DrawRay(origin, direction.normalized * (hit ? hitInfo.distance : range), hit ? Color.red : Color.green);
            return hit ? hitInfo.distance : range;
        }

        private bool ValidateGapVolume(Vector3 start, Vector3 end)
        {
            Vector3 center = (start + end) * 0.5f + transform.right * (freeSideThreshold * 0.72f) + Vector3.up * (boxValidationHeight * 0.5f);
            Vector3 halfExtents = new Vector3(1.05f, boxValidationHeight * 0.5f, Mathf.Max(1.2f, Vector3.Distance(start, end) * 0.42f));
            Quaternion rotation = transform.rotation;
            bool blocked = Physics.CheckBox(center, halfExtents, rotation, obstacleMask, QueryTriggerInteraction.Ignore);
            DebugDrawBox(center, halfExtents, rotation, blocked ? Color.yellow : Color.cyan);
            return !blocked;
        }

        private static void DebugDrawBox(Vector3 center, Vector3 halfExtents, Quaternion rotation, Color color)
        {
            Vector3[] local =
            {
                new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(1, -1, 1), new Vector3(-1, -1, 1),
                new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(-1, 1, 1)
            };

            Vector3[] points = new Vector3[8];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = center + rotation * Vector3.Scale(local[i], halfExtents);
            }

            DrawEdge(points, 0, 1, color);
            DrawEdge(points, 1, 2, color);
            DrawEdge(points, 2, 3, color);
            DrawEdge(points, 3, 0, color);
            DrawEdge(points, 4, 5, color);
            DrawEdge(points, 5, 6, color);
            DrawEdge(points, 6, 7, color);
            DrawEdge(points, 7, 4, color);
            DrawEdge(points, 0, 4, color);
            DrawEdge(points, 1, 5, color);
            DrawEdge(points, 2, 6, color);
            DrawEdge(points, 3, 7, color);
        }

        private static void DrawEdge(Vector3[] points, int a, int b, Color color)
        {
            Debug.DrawLine(points[a], points[b], color);
        }
    }
}
