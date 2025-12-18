using UnityEngine;
using SpinMotion;

public class CompleteAICarSetup : MonoBehaviour
{
    [Header("Required References")]
    public GameEvents gameEvents;
    public AIWaypointSet aiWaypointSet;
    
    [Header("AI Car Prefab")]
    public GameObject aiCarPrefab;
    
    [Header("Waypoint Tracker")]
    public AIWaypointTracker waypointTrackerPrefab;
    
    void Start()
    {
        SetupAIRace();
    }
    
    void SetupAIRace()
    {
        // Tạo waypoint tracker
        AIWaypointTracker tracker = Instantiate(waypointTrackerPrefab);
        tracker.gameEvents = gameEvents;
        tracker.aiWaypointSet = aiWaypointSet;
        
        // Spawn AI cars
        for (int i = 0; i < 3; i++) // Spawn 3 xe AI
        {
            GameObject aiCar = Instantiate(aiCarPrefab);
            
            // Setup AI components
            CarAIControl aiControl = aiCar.GetComponent<CarAIControl>();
            if (aiControl != null)
            {
                aiControl.SetTarget(tracker.transform);
            }
            
            // Setup tracker reference
            tracker.SetupAICarCollider(aiCar.GetComponent<Collider>());
        }
    }
}