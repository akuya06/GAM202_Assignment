using UnityEngine;
using SpinMotion;

public class AICarSetup : MonoBehaviour
{
    [Header("AI Components")]
    public CarAIControl carAIControl;
    public AIWaypointTracker waypointTracker;
    
    void Start()
    {
        // Setup AI car to follow waypoint tracker
        if (carAIControl != null && waypointTracker != null)
        {
            carAIControl.SetTarget(waypointTracker.transform);
            waypointTracker.SetupAICarCollider(GetComponent<Collider>());
        }
    }
}