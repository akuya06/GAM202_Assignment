using System.Linq;
using UnityEngine;

namespace SpinMotion
{
    public class AIWaypoints : MonoBehaviour
    {
        public GameEvents gameEvents;
        public AIWaypointSet aiWaypointSet;
        
        [Header("Setup")]
        [Tooltip("Thứ tự waypoints sẽ theo thứ tự trong Hierarchy (top to bottom)")]
        public bool useHierarchyOrder = true;
        
        [Tooltip("Check nếu xe chạy ngược chiều")]
        public bool reverseOrder = false;

        private void Start()
        {
            SetupWaypoints();
        }

        private void SetupWaypoints()
        {
            var waypoints = GetComponentsInChildren<Transform>().ToList();
            waypoints.RemoveAt(0); // Remove parent transform
            
            // Nếu không dùng thứ tự Hierarchy, sort theo tên
            if (!useHierarchyOrder)
            {
                waypoints = waypoints.OrderBy(t => t.name).ToList();
            }
            
            // Đảo ngược nếu cần
            if (reverseOrder)
            {
                waypoints.Reverse();
            }
            
            aiWaypointSet.Items.Clear();
            
            for (int i = 0; i < waypoints.Count; i++)
            {
                var waypoint = waypoints[i];
                if (waypoint.TryGetComponent<MeshRenderer>(out var meshRenderer))
                {
                    aiWaypointSet.Add(new AIWaypoint 
                    { 
                        aiWaypointTransform = waypoint, 
                        aiWaypointMeshRenderer = meshRenderer 
                    });
                    meshRenderer.enabled = false;
                    
                    Debug.Log($"Waypoint {i + 1}: {waypoint.name}");
                }
            }
            
            Debug.Log($"✅ Setup {aiWaypointSet.Items.Count} waypoints");
        }

        // Vẽ đường để xem thứ tự
        private void OnDrawGizmos()
        {
            if (aiWaypointSet == null || aiWaypointSet.Items == null || aiWaypointSet.Items.Count == 0)
                return;

            for (int i = 0; i < aiWaypointSet.Items.Count; i++)
            {
                var current = aiWaypointSet.Items[i].aiWaypointTransform;
                if (current == null) continue;
                
                Vector3 pos = current.position;
                
                // Vẽ hình cầu với màu theo thứ tự
                Gizmos.color = Color.Lerp(Color.green, Color.red, (float)i / aiWaypointSet.Items.Count);
                Gizmos.DrawWireSphere(pos, 2f);
                
                // Vẽ số thứ tự
                #if UNITY_EDITOR
                var style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.fontSize = 20;
                style.fontStyle = FontStyle.Bold;
                UnityEditor.Handles.Label(pos + Vector3.up * 3, $"{i + 1}", style);
                #endif
                
                // Vẽ đường đến waypoint tiếp theo
                if (i < aiWaypointSet.Items.Count - 1)
                {
                    var next = aiWaypointSet.Items[i + 1].aiWaypointTransform;
                    if (next != null)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(pos, next.position);
                        
                        // Vẽ mũi tên
                        Vector3 dir = (next.position - pos).normalized;
                        Vector3 midPoint = pos + dir * Vector3.Distance(pos, next.position) * 0.5f;
                        DrawArrow(midPoint, dir);
                    }
                }
                else
                {
                    // Vẽ đường về điểm đầu
                    var first = aiWaypointSet.Items[0].aiWaypointTransform;
                    if (first != null)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(pos, first.position);
                    }
                }
            }
        }

        private void DrawArrow(Vector3 pos, Vector3 direction)
        {
            Gizmos.color = Color.cyan;
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 30, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 30, 0) * Vector3.forward;
            Gizmos.DrawRay(pos, right * 2);
            Gizmos.DrawRay(pos, left * 2);
        }
    }

    [System.Serializable]
    public class AIWaypoint
    {
        public Transform aiWaypointTransform;
        public MeshRenderer aiWaypointMeshRenderer;
    }
}