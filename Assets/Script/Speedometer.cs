using UnityEngine;
using UnityEngine.UI;

public class Speedometer : MonoBehaviour
{
    [Header("Speedometer Settings")]
    public float maxSpeed = 200f; // Maximum speed value
    public float currentSpeed = 0f; // Current speed value
    
    [Header("Vehicle Reference")]
    public Transform vehicleTransform; // The vehicle to track
    public Rigidbody vehicleRigidbody; // Vehicle's Rigidbody component
    
    [Header("Speed Calculation")]
    public bool useRigidbody = true; // Use Rigidbody velocity or manual calculation
    public float speedMultiplier = 3.6f; // Convert m/s to km/h (3.6) or to mph (2.237)
    
    [Header("UI Components")]
    public Transform needleTransform; // The needle/pointer transform
    public Text speedText; // Optional text to display speed value
    
    [Header("Needle Rotation")]
    public float minAngle = -135f; // Starting angle (bottom left)
    public float maxAngle = 135f;  // Ending angle (bottom right)
    
    [Header("Animation")]
    public float smoothTime = 0.1f; // Smoothing for needle movement
    private float velocity = 0f;
    
    // For manual speed calculation
    private Vector3 lastPosition;
    private float lastTime;
    
    void Start()
    {
        // Auto-find vehicle components if not assigned
        if (vehicleTransform == null)
            vehicleTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (vehicleRigidbody == null && vehicleTransform != null)
            vehicleRigidbody = vehicleTransform.GetComponent<Rigidbody>();
        
        // Initialize for manual calculation
        if (vehicleTransform != null)
        {
            lastPosition = vehicleTransform.position;
            lastTime = Time.time;
        }
        
        // Initialize speedometer
        UpdateSpeedometer();
    }
    
    void Update()
    {
        // Calculate current speed
        CalculateSpeed();
        
        // Update speedometer display
        UpdateSpeedometer();
    }
    
    void CalculateSpeed()
    {
        if (vehicleTransform == null) return;
        
        float speed = 0f;
        
        if (useRigidbody && vehicleRigidbody != null)
        {
            // Use Rigidbody velocity (more accurate for physics-based vehicles)
            speed = vehicleRigidbody.linearVelocity.magnitude * speedMultiplier;
        }
        else
        {
            // Manual calculation using position change
            float deltaTime = Time.time - lastTime;
            if (deltaTime > 0f)
            {
                Vector3 deltaPosition = vehicleTransform.position - lastPosition;
                speed = (deltaPosition.magnitude / deltaTime) * speedMultiplier;
                
                lastPosition = vehicleTransform.position;
                lastTime = Time.time;
            }
        }
        
        currentSpeed = speed;
    }
    
    public void SetSpeed(float speed)
    {
        currentSpeed = Mathf.Clamp(speed, 0f, maxSpeed);
    }
    
    void UpdateSpeedometer()
    {
        if (needleTransform != null)
        {
            // Calculate the percentage of current speed relative to max speed
            float speedPercentage = currentSpeed / maxSpeed;
            
            // Calculate target angle based on speed percentage
            float targetAngle = Mathf.Lerp(minAngle, maxAngle, speedPercentage);
            
            // Smooth the needle rotation
            float currentAngle = Mathf.SmoothDampAngle(
                needleTransform.eulerAngles.z, 
                targetAngle, 
                ref velocity, 
                smoothTime
            );
            
            // Apply rotation to needle
            needleTransform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
        
        // Update speed text if available
        if (speedText != null)
        {
            speedText.text = Mathf.RoundToInt(currentSpeed).ToString();
        }
    }
    
    // Public methods for external control
    public void IncreaseSpeed(float amount)
    {
        SetSpeed(currentSpeed + amount);
    }
    
    public void DecreaseSpeed(float amount)
    {
        SetSpeed(currentSpeed - amount);
    }
    
    // Method to manually assign vehicle
    public void AssignVehicle(Transform vehicle)
    {
        vehicleTransform = vehicle;
        vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
        
        if (vehicleTransform != null)
        {
            lastPosition = vehicleTransform.position;
            lastTime = Time.time;
        }
    }
    
    // For testing purposes
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateSpeedometer();
        }
    }
}