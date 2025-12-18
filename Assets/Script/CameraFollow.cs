using UnityEngine;

public class RacingCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // xe đua cần theo

    [Header("Third Person Settings")]
    public Vector3 offset = new Vector3(0f, 2.5f, -6f); // vị trí camera so với xe (local space)
    public float followSpeed = 3f;       // tốc độ follow vị trí (giảm để chậm hơn)
    public float rotationSpeed = 2f;     // tốc độ quay camera (giảm để mượt hơn)

    [Header("Smoothing")]
    public float positionSmoothing = 0.15f;  // độ mượt vị trí (nhỏ = mượt hơn)
    public float rotationSmoothing = 0.12f;  // độ mượt rotation (nhỏ = mượt hơn)
    public float targetFollowLag = 0.8f;     // độ trễ theo target (1 = không trễ, 0 = rất trễ)

    [Header("Manual Control")]
    public float mouseSensitivity = 2f;  // giảm độ nhạy chuột
    public float minPitch = -20f;        
    public float maxPitch = 45f;
    public float returnDelay = 1.5f;     // tăng thời gian chờ
    public float returnSpeed = 1.5f;     // giảm tốc độ quay về

    [Header("Zoom")]
    public float zoomSpeed = 1.5f;       // giảm tốc độ zoom
    public float minDistance = 3f;
    public float maxDistance = 12f;

    private float currentYaw;
    private float currentPitch = 15f;
    private float currentDistance;
    private float lastManualTime = -999f;
    private Vector3 velocity;
    private Vector3 rotationVelocity;
    
    // smooth target following
    private Vector3 smoothTargetPosition;
    private Vector3 targetVelocity;

    void Start()
    {
        if (target == null) return;
        
        currentYaw = target.eulerAngles.y;
        currentDistance = offset.magnitude;
        currentPitch = Mathf.Atan2(offset.y, -offset.z) * Mathf.Rad2Deg;
        
        // khởi tạo smooth target
        smoothTargetPosition = target.position;
    }

    void Update()
    {
        if (target == null) return;

        UpdateSmoothTarget();
        HandleZoom();
        HandleManualRotation();
        HandleAutoReturn();
    }

    void UpdateSmoothTarget()
    {
        // tạo target position mượt với độ trễ
        smoothTargetPosition = Vector3.SmoothDamp(
            smoothTargetPosition, 
            target.position, 
            ref targetVelocity, 
            (1f - targetFollowLag) * 0.5f
        );
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
    }

    void HandleManualRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            currentYaw += mouseX * mouseSensitivity * rotationSpeed;
            currentPitch -= mouseY * mouseSensitivity * rotationSpeed;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

            lastManualTime = Time.time;
        }
    }

    void HandleAutoReturn()
    {
        if (Time.time - lastManualTime > returnDelay)
        {
            float targetYaw = target.eulerAngles.y;
            float targetPitch = 15f;

            // smooth return với exponential decay
            float returnBlend = 1f - Mathf.Exp(-returnSpeed * Time.deltaTime);
            currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, returnBlend);
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, returnBlend);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // tính vị trí camera dựa trên smooth target position
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 direction = rotation * Vector3.back * currentDistance;
        Vector3 desiredPosition = smoothTargetPosition + Vector3.up * offset.y + direction;

        // smooth position với SmoothDamp cho chuyển động tự nhiên
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            desiredPosition, 
            ref velocity, 
            positionSmoothing
        );

        // smooth rotation khi nhìn về xe
        Vector3 lookTarget = smoothTargetPosition + Vector3.up * (offset.y * 0.5f);
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            1f - Mathf.Exp(-rotationSpeed / rotationSmoothing * Time.deltaTime)
        );
    }
}
