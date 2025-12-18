using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SpinMotion
{
    [RequireComponent(typeof (CarController))]
    public class CarAIControl : MonoBehaviour
    {
        public enum BrakeCondition
        {
            NeverBrake,
            TargetDirectionDifference,
            TargetDistance,
        }

        [SerializeField] [Range(0, 1)] private float m_CautiousSpeedFactor = 0.05f;
        [SerializeField] [Range(0, 180)] private float m_CautiousMaxAngle = 50f;
        [SerializeField] private float m_CautiousMaxDistance = 100f;
        [SerializeField] private float m_CautiousAngularVelocityFactor = 30f;
        [SerializeField] private float m_SteerSensitivity = 0.05f;
        [SerializeField] private float m_AccelSensitivity = 0.04f;
        [SerializeField] private float m_BrakeSensitivity = 1f;
        [SerializeField] private float m_LateralWanderDistance = 3f;
        [SerializeField] private float m_LateralWanderSpeed = 0.1f;
        [SerializeField] [Range(0, 1)] private float m_AccelWanderAmount = 0.1f;
        [SerializeField] private float m_AccelWanderSpeed = 0.1f;
        [SerializeField] private BrakeCondition m_BrakeCondition = BrakeCondition.TargetDistance;
        [SerializeField] private bool m_Driving;
        [SerializeField] private Transform m_Target;
        [SerializeField] private bool m_StopWhenTargetReached;
        [SerializeField] private float m_ReachTargetThreshold = 2;

        [Header("Stuck Recovery")]
        [SerializeField] private float m_StuckCheckInterval = 2f;        // Kiểm tra mỗi 2 giây
        [SerializeField] private float m_StuckSpeedThreshold = 2f;       // Tốc độ dưới 2m/s = kẹt
        [SerializeField] private float m_StuckReverseTime = 1.5f;        // Lùi 1.5 giây
        [SerializeField] private float m_MaxTiltAngle = 45f;             // Góc nghiêng tối đa

        private float m_RandomPerlin;
        private CarController m_CarController;
        private float m_AvoidOtherCarTime;
        private float m_AvoidOtherCarSlowdown;
        private float m_AvoidPathOffset;
        private Rigidbody m_Rigidbody;
        
        // Stuck detection variables
        private float m_StuckCheckTimer;
        private Vector3 m_LastPosition;
        private float m_StuckTimer;
        private bool m_IsReversing;
        private float m_ReverseTimer;

        private void Awake()
        {
            m_CarController = GetComponent<CarController>();
            m_RandomPerlin = Random.value*100;
            m_Rigidbody = GetComponent<Rigidbody>();
            m_LastPosition = transform.position;
        }

        public void SetTarget(Transform target)
        {
            m_Target = target;
            m_Driving = true;
        }

        private void FixedUpdate()
        {
            // Check if car is stuck or flipped
            CheckIfStuck();
            
            // If reversing to get unstuck
            if (m_IsReversing)
            {
                m_ReverseTimer -= Time.fixedDeltaTime;
                
                if (m_ReverseTimer <= 0)
                {
                    m_IsReversing = false;
                    m_StuckTimer = 0;
                }
                else
                {
                    // Reverse with steering
                    m_CarController.Move(Random.Range(-0.5f, 0.5f), -1f, -1f, 0f);
                    return;
                }
            }

            if (m_Target == null || !m_Driving)
            {
                m_CarController.Move(0, 0, -1f, 1f);
            }
            else
            {
                Vector3 fwd = transform.forward;
                if (m_Rigidbody.linearVelocity.magnitude > m_CarController.MaxSpeed*0.1f)
                {
                    fwd = m_Rigidbody.linearVelocity;
                }

                float desiredSpeed = m_CarController.MaxSpeed;

                switch (m_BrakeCondition)
                {
                    case BrakeCondition.TargetDirectionDifference:
                        {
                            float approachingCornerAngle = Vector3.Angle(m_Target.forward, fwd);
                            float spinningAngle = m_Rigidbody.angularVelocity.magnitude*m_CautiousAngularVelocityFactor;
                            float cautiousnessRequired = Mathf.InverseLerp(0, m_CautiousMaxAngle,
                                                                           Mathf.Max(spinningAngle, approachingCornerAngle));
                            desiredSpeed = Mathf.Lerp(m_CarController.MaxSpeed, m_CarController.MaxSpeed*m_CautiousSpeedFactor,
                                                      cautiousnessRequired);
                            break;
                        }

                    case BrakeCondition.TargetDistance:
                        {
                            Vector3 delta = m_Target.position - transform.position;
                            float distanceCautiousFactor = Mathf.InverseLerp(m_CautiousMaxDistance, 0, delta.magnitude);
                            float spinningAngle = m_Rigidbody.angularVelocity.magnitude*m_CautiousAngularVelocityFactor;
                            float cautiousnessRequired = Mathf.Max(
                                Mathf.InverseLerp(0, m_CautiousMaxAngle, spinningAngle), distanceCautiousFactor);
                            desiredSpeed = Mathf.Lerp(m_CarController.MaxSpeed, m_CarController.MaxSpeed*m_CautiousSpeedFactor,
                                                      cautiousnessRequired);
                            break;
                        }

                    case BrakeCondition.NeverBrake:
                        break;
                }

                Vector3 offsetTargetPos = m_Target.position;

                if (Time.time < m_AvoidOtherCarTime)
                {
                    desiredSpeed *= m_AvoidOtherCarSlowdown;
                    offsetTargetPos += m_Target.right*m_AvoidPathOffset;
                }
                else
                {
                    offsetTargetPos += m_Target.right*
                                       (Mathf.PerlinNoise(Time.time*m_LateralWanderSpeed, m_RandomPerlin)*2 - 1)*
                                       m_LateralWanderDistance;
                }

                float accelBrakeSensitivity = (desiredSpeed < m_CarController.CurrentSpeed)
                                                  ? m_BrakeSensitivity
                                                  : m_AccelSensitivity;

                float accel = Mathf.Clamp((desiredSpeed - m_CarController.CurrentSpeed)*accelBrakeSensitivity, -1, 1);

                accel *= (1 - m_AccelWanderAmount) +
                         (Mathf.PerlinNoise(Time.time*m_AccelWanderSpeed, m_RandomPerlin)*m_AccelWanderAmount);

                Vector3 localTarget = transform.InverseTransformPoint(offsetTargetPos);
                float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z)*Mathf.Rad2Deg;
                float steer = Mathf.Clamp(targetAngle*m_SteerSensitivity, -1, 1)*Mathf.Sign(m_CarController.CurrentSpeed);

                m_CarController.Move(steer, accel, accel, 0f);

                if (m_StopWhenTargetReached && localTarget.magnitude < m_ReachTargetThreshold)
                {
                    m_Driving = false;
                }
            }
        }

        private void CheckIfStuck()
        {
            m_StuckCheckTimer += Time.fixedDeltaTime;
            
            if (m_StuckCheckTimer >= m_StuckCheckInterval)
            {
                m_StuckCheckTimer = 0;
                
                // Check speed
                float speed = m_Rigidbody.linearVelocity.magnitude;
                bool isMovingSlow = speed < m_StuckSpeedThreshold;
                
                // Check if car is flipped or tilted
                float tiltAngle = Vector3.Angle(transform.up, Vector3.up);
                bool isFlipped = tiltAngle > m_MaxTiltAngle;
                
                // Check if car moved very little
                float distanceMoved = Vector3.Distance(transform.position, m_LastPosition);
                bool notMoving = distanceMoved < 1f;
                
                if ((isMovingSlow && notMoving) || isFlipped)
                {
                    m_StuckTimer += m_StuckCheckInterval;
                    
                    // If stuck for more than 1 check, start reversing
                    if (m_StuckTimer >= m_StuckCheckInterval)
                    {
                        StartReversing();
                    }
                }
                else
                {
                    m_StuckTimer = 0;
                }
                
                m_LastPosition = transform.position;
            }
        }

        private void StartReversing()
        {
            m_IsReversing = true;
            m_ReverseTimer = m_StuckReverseTime;
            m_StuckTimer = 0;
        }

        private void OnCollisionStay(Collision col)
        {
            if (col.rigidbody != null)
            {
                var otherAI = col.rigidbody.GetComponent<CarAIControl>();
                if (otherAI != null)
                {
                    m_AvoidOtherCarTime = Time.time + 1;

                    if (Vector3.Angle(transform.forward, otherAI.transform.position - transform.position) < 90)
                    {
                        m_AvoidOtherCarSlowdown = 0.5f;
                    }
                    else
                    {
                        m_AvoidOtherCarSlowdown = 1;
                    }

                    var otherCarLocalDelta = transform.InverseTransformPoint(otherAI.transform.position);
                    float otherCarAngle = Mathf.Atan2(otherCarLocalDelta.x, otherCarLocalDelta.z);
                    m_AvoidPathOffset = m_LateralWanderDistance*-Mathf.Sign(otherCarAngle);
                }
            }
        }
    }
}
