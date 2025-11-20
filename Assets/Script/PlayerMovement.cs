// ...existing code...
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple arcade car movement with drift
public class PlayerMovement : MonoBehaviour
{
    [Header("Drive")]
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float maxForwardSpeed = 25f;
    [SerializeField] private float brakeForce = 40f;
    [SerializeField] private float naturalDrag = 0.1f;

    [Header("Steering")]
    [SerializeField] private float turnSpeed = 80f; // base turning (deg/s)
    [SerializeField] private float minSpeedForSteer = 0.5f; // only steer when forward speed above this

    [Header("Drift")]
    [SerializeField] private KeyCode driftKey = KeyCode.LeftShift;
    [SerializeField] private float driftTurnMultiplier = 1.6f; // turn multiplier while drifting (sharp look)
    [SerializeField] private float driftLateralForce = 6f; // how much sideways velocity is added while drifting
    [SerializeField, Range(0f, 10f)] private float driftRecovery = 4f; // how fast lateral velocity is damped when not drifting
    [SerializeField, Range(0f, 10f)] private float grip = 5f; // higher = faster lateral damping (non-drift)

    [Header("Input")]
    [SerializeField, Range(0f, 0.2f)] private float inputDeadzone = 0.05f;

    Rigidbody rb;
    float moveInput;
    float steerInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // prevent flipping over for simple arcade behavior
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void FixedUpdate()
    {
        ReadInput();

        if (rb == null) return;

        HandleDrive();
        HandleSteerAndDrift();
        ApplyNaturalDrag();
    }

    void ReadInput()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(moveInput) < inputDeadzone) moveInput = 0f;
        if (Mathf.Abs(steerInput) < inputDeadzone) steerInput = 0f;
    }

    void HandleDrive()
    {
        // forward acceleration
        if (Mathf.Abs(moveInput) > 0f)
        {
            rb.AddForce(transform.forward * moveInput * acceleration, ForceMode.Acceleration);
        }
        else
        {
            // small passive braking when no input
            rb.AddForce(-rb.linearVelocity * naturalDrag, ForceMode.Acceleration);
        }

        // clamp forward speed (only affect forward component)
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        float forward = localVel.z;
        forward = Mathf.Clamp(forward, -maxForwardSpeed * 0.5f, maxForwardSpeed);
        localVel.z = forward;
        rb.linearVelocity = transform.TransformDirection(new Vector3(localVel.x, rb.linearVelocity.y, localVel.z));
    }

    void HandleSteerAndDrift()
    {
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        float forwardSpeed = Mathf.Abs(localVel.z);

        bool drifting = Input.GetKey(driftKey) && forwardSpeed > 0.5f;

        // steering: only when moving
        if (forwardSpeed > minSpeedForSteer)
        {
            float forwardFactor = Mathf.Clamp01(forwardSpeed / maxForwardSpeed);
            float turnMultiplier = drifting ? driftTurnMultiplier : 1f;
            float yaw = steerInput * turnSpeed * forwardFactor * turnMultiplier * Time.fixedDeltaTime;

            // if moving backward, invert steering
            if (localVel.z < 0f) yaw = -yaw;

            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, yaw, 0f));
        }

        // lateral (sideways) handling for drift effect
        float lateral = localVel.x;

        if (drifting)
        {
            // push sideways in direction of steering input for stylish slide
            lateral += steerInput * driftLateralForce * Time.fixedDeltaTime;
            // optionally reduce forward grip slightly so drift feels wobbly
            localVel.z *= 0.995f;
        }
        else
        {
            // recover lateral velocity toward zero (grip)
            lateral = Mathf.Lerp(lateral, 0f, Mathf.Clamp01(grip * Time.fixedDeltaTime * 0.5f));
        }

        // apply lateral and keep world Y velocity (gravity)
        Vector3 newLocal = new Vector3(lateral, 0f, localVel.z);
        Vector3 newWorld = transform.TransformDirection(newLocal);
        rb.linearVelocity = new Vector3(newWorld.x, rb.linearVelocity.y, newWorld.z);
    }

    void ApplyNaturalDrag()
    {
        // keep overall speed under absolute cap
        Vector3 v = rb.linearVelocity;
        float flatSpeed = new Vector3(v.x, 0f, v.z).magnitude;
        if (flatSpeed > maxForwardSpeed * 1.2f)
        {
            Vector3 flatDir = new Vector3(v.x, 0f, v.z).normalized;
            Vector3 clamped = flatDir * (maxForwardSpeed * 1.2f);
            rb.linearVelocity = new Vector3(clamped.x, v.y, clamped.z);
        }
    }
}
// ...existing code...