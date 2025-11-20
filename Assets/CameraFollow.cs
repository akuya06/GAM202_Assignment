using UnityEngine;

public class RobloxCameraController : MonoBehaviour
{
    public Transform target; // Nhân vật cần theo
    public float distance = 5.0f;
    public float zoomSpeed = 2.0f;
    public float minDistance = 2.0f;
    public float maxDistance = 10.0f;
    public float rotationSpeed = 5.0f;

    private float currentX = 0f;
    private float currentY = 0f;

    void Update()
    {
        // Zoom bằng con lăn chuột
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Xoay camera khi giữ chuột phải
        if (Input.GetMouseButton(1))
        {
            currentX += Input.GetAxis("Mouse X") * rotationSpeed;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
            currentY = Mathf.Clamp(currentY, -30, 60); // Giới hạn góc nhìn
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = new Vector3(0, 0, -distance);
        transform.position = target.position + rotation * direction;
        transform.LookAt(target.position);
    }
}
