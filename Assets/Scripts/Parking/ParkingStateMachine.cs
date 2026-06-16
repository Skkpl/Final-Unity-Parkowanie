using UnityEngine;

namespace AutomaticParking
{
    [RequireComponent(typeof(ParkingCarController))]
    [RequireComponent(typeof(ParkingSensors))]
    [RequireComponent(typeof(ParkingManeuverPlanner))]
    public sealed class ParkingStateMachine : MonoBehaviour
    {
        public ParkingState CurrentState { get; private set; } = ParkingState.Scan;
        public ParkingSpotCandidate CurrentSpot { get; private set; }
        public float StateElapsed { get; private set; }

        [Header("Scan")]
        public float scanThrottle = 0.35f;
        public bool restartWhenParked;

        private ParkingCarController car;
        private ParkingSensors sensors;
        private ParkingManeuverPlanner planner;
        private ParkingScenario scenario;
        private ParkingState interruptedState = ParkingState.Scan;
        private float interruptedElapsed;
        private float parkedElapsed;
        private int scenarioStepIndex;
        private float externalEmergencyTimer;
        private bool scenarioStepStarted;
        private Vector3 stepStartPosition;
        private float stepStartYaw;
        private float stepYawDelta;
        private float stepProgress;
        private float stepPathLength;

        private void Awake()
        {
            car = GetComponent<ParkingCarController>();
            sensors = GetComponent<ParkingSensors>();
            planner = GetComponent<ParkingManeuverPlanner>();
            scenario = GetComponent<ParkingScenario>();
            car.UseExternalPoseControl = scenario != null;
        }

        private void Start()
        {
            ChangeState(ParkingState.Scan);
        }

        private void FixedUpdate()
        {
            StateElapsed += Time.fixedDeltaTime;
            externalEmergencyTimer = Mathf.Max(0.0f, externalEmergencyTimer - Time.fixedDeltaTime);

            bool allowRaycastEmergency = scenario == null || scenario.useRaycastEmergencyStop;
            bool raycastEmergency = allowRaycastEmergency && sensors.HasEmergencyObstacle();
            bool scriptedEmergency = externalEmergencyTimer > 0.0f;

            if (CurrentState != ParkingState.EmergencyStop
                && CurrentState != ParkingState.Scan
                && CurrentState != ParkingState.Parked
                && (raycastEmergency || scriptedEmergency))
            {
                interruptedState = CurrentState;
                interruptedElapsed = StateElapsed;
                ChangeState(ParkingState.EmergencyStop);
                return;
            }

            switch (CurrentState)
            {
                case ParkingState.Scan:
                    if (scenario != null)
                    {
                        UpdateScenarioScan();
                    }
                    else
                    {
                        UpdateScan();
                    }
                    break;
                case ParkingState.ValidateSpot:
                    if (scenario != null)
                    {
                        UpdateScenarioValidation();
                    }
                    else
                    {
                        ChangeState(ParkingState.Positioning);
                    }
                    break;
                case ParkingState.Positioning:
                case ParkingState.ReverseTurn:
                case ParkingState.CounterTurn:
                case ParkingState.Straighten:
                    if (scenario != null)
                    {
                        UpdateScenarioManeuverState();
                    }
                    else
                    {
                        UpdateManeuverState();
                    }
                    break;
                case ParkingState.EmergencyStop:
                    UpdateEmergencyStop();
                    break;
                case ParkingState.Parked:
                    UpdateParked();
                    break;
            }
        }

        public void ChangeState(ParkingState nextState)
        {
            CurrentState = nextState;
            StateElapsed = 0.0f;

            if (nextState == ParkingState.Parked)
            {
                parkedElapsed = 0.0f;
                car.StopImmediately();
            }
        }

        public void RequestEmergencyStop(float seconds)
        {
            externalEmergencyTimer = Mathf.Max(externalEmergencyTimer, seconds);
        }

        private void UpdateScan()
        {
            car.SetControls(scanThrottle, 0.0f, 0.0f);

            if (sensors.TryFindSpot(out ParkingSpotCandidate spot))
            {
                CurrentSpot = spot;
                ChangeState(ParkingState.ValidateSpot);
            }
        }

        private void UpdateScenarioScan()
        {
            sensors.GetReadings();

            if (scenario.scanAcceptPoint == null)
            {
                CurrentSpot = scenario.BuildCandidate();
                sensors.PublishCandidate(CurrentSpot);
                ChangeState(ParkingState.ValidateSpot);
                return;
            }

            bool arrived = car.MoveToPose(
                scenario.scanAcceptPoint.position,
                scenario.scanAcceptPoint.rotation,
                false,
                scenario.scanSpeed,
                scenario.scanTurnSpeed,
                scenario.scanArriveDistance,
                8.0f);

            if (arrived)
            {
                CurrentSpot = scenario.BuildCandidate();
                sensors.PublishCandidate(CurrentSpot);
                ChangeState(ParkingState.ValidateSpot);
            }
        }

        private void UpdateScenarioValidation()
        {
            car.StopImmediately();
            sensors.PublishCandidate(CurrentSpot);

            if (StateElapsed < scenario.validatePause)
            {
                return;
            }

            scenarioStepIndex = 0;
            scenarioStepStarted = false;
            if (scenario.steps == null || scenario.steps.Length == 0)
            {
                ChangeState(ParkingState.Parked);
                return;
            }

            ChangeState(scenario.steps[scenarioStepIndex].state);
        }

        private void UpdateScenarioManeuverState()
        {
            if (scenario.steps == null || scenarioStepIndex >= scenario.steps.Length)
            {
                ChangeState(ParkingState.Parked);
                return;
            }

            ParkingScenarioStep step = scenario.steps[scenarioStepIndex];
            if (step.target == null)
            {
                AdvanceScenarioStep();
                return;
            }

            bool arrived = step.smoothArc
                ? UpdateSmoothScenarioStep(step)
                : car.MoveToPose(
                    step.target.position,
                    step.target.rotation,
                    step.reverse,
                    step.speed,
                    step.turnSpeed,
                    step.arriveDistance,
                    step.angleTolerance);

            if (arrived || StateElapsed > step.timeout)
            {
                AdvanceScenarioStep();
            }
        }

        private void AdvanceScenarioStep()
        {
            scenarioStepIndex++;
            scenarioStepStarted = false;
            if (scenario.steps == null || scenarioStepIndex >= scenario.steps.Length)
            {
                ChangeState(ParkingState.Parked);
                return;
            }

            ChangeState(scenario.steps[scenarioStepIndex].state);
        }

        private bool UpdateSmoothScenarioStep(ParkingScenarioStep step)
        {
            if (!scenarioStepStarted)
            {
                stepStartPosition = transform.position;
                stepStartYaw = transform.eulerAngles.y;
                stepYawDelta = Mathf.DeltaAngle(stepStartYaw, step.target.rotation.eulerAngles.y);
                stepProgress = 0.0f;
                stepPathLength = Mathf.Max(0.1f, Vector3.Distance(stepStartPosition, step.target.position));
                scenarioStepStarted = true;
            }

            float speed = Mathf.Max(0.1f, step.speed);
            stepProgress = Mathf.Clamp01(stepProgress + speed * Time.fixedDeltaTime / stepPathLength);

            Vector3 position = Vector3.Lerp(stepStartPosition, step.target.position, stepProgress);
            float easedProgress = Mathf.SmoothStep(0.0f, 1.0f, stepProgress);
            float yaw = stepStartYaw + stepYawDelta * easedProgress;
            Quaternion finalRotation = Quaternion.Euler(0.0f, yaw, 0.0f);
            float steeringHint = Mathf.Clamp(stepYawDelta * 0.65f, -car.maxSteeringAngle, car.maxSteeringAngle);
            car.SetPoseAlongPath(position, finalRotation, step.reverse ? -speed : speed, steeringHint);

            return stepProgress >= 0.999f;
        }

        private void UpdateManeuverState()
        {
            planner.ApplyControls(car, CurrentSpot.parkingType, CurrentState);

            if (StateElapsed < planner.GetDuration(CurrentSpot.parkingType, CurrentState))
            {
                return;
            }

            ParkingState next = CurrentState switch
            {
                ParkingState.Positioning => ParkingState.ReverseTurn,
                ParkingState.ReverseTurn => ParkingState.CounterTurn,
                ParkingState.CounterTurn => ParkingState.Straighten,
                ParkingState.Straighten => ParkingState.Parked,
                _ => ParkingState.Parked
            };

            ChangeState(next);
        }

        private void UpdateEmergencyStop()
        {
            car.StopImmediately();
            SensorReadings readings = sensors.GetReadings();
            bool clearAgain = readings.front > 2.0f && readings.rear > 1.8f && externalEmergencyTimer <= 0.0f;
            if (clearAgain && StateElapsed > 0.8f)
            {
                CurrentState = interruptedState;
                StateElapsed = interruptedElapsed;
            }
        }

        private void UpdateParked()
        {
            parkedElapsed += Time.fixedDeltaTime;
            car.SetControls(0.0f, 0.0f, 1.0f);

            if (restartWhenParked && parkedElapsed > 5.0f)
            {
                sensors.ResetScan();
                ChangeState(ParkingState.Scan);
            }
        }
    }
}
