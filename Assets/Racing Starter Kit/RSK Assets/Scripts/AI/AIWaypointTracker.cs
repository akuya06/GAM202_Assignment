using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpinMotion
{
    public class AIWaypointTracker : MonoBehaviour
    {
        public GameEvents gameEvents;
        public AIWaypointSet aiWaypointSet;
        public Vector3 trackerScale;
        
        [Header("AI Car")]
        public GameObject aiCar;

        private List<AIWaypoint> aiWaypoints = new();
        private Transform currentWaypoint;
        private int currentIndex;
        private Collider aiCarCollider;
        private Collider waypointTrackerCollider;
        private CarAIControl aiControl;
        private float checkDistanceTimer = 0f;
        
        private void Awake()
        {
            if (gameEvents != null)
                gameEvents.RestartRaceEvent.AddListener(OnRestartRace);
                
            waypointTrackerCollider = GetComponent<BoxCollider>();
            if (waypointTrackerCollider != null)
            {
                waypointTrackerCollider.isTrigger = true;
            }
            
            transform.localScale = trackerScale;
        }

        private void Start()
        {
            StartCoroutine(SetupDelayed());
        }

        private IEnumerator SetupDelayed()
        {
            yield return new WaitForEndOfFrame();
            
            ResetTracker();
            
            yield return new WaitForEndOfFrame();
            
            if (aiCar != null)
            {
                aiCarCollider = aiCar.GetComponent<Collider>();
                
                aiControl = aiCar.GetComponent<CarAIControl>();
                if (aiControl != null)
                {
                    aiControl.SetTarget(this.transform);
                }
            }
        }

        private void Update()
        {
            checkDistanceTimer += Time.deltaTime;
            if (checkDistanceTimer > 0.5f)
            {
                checkDistanceTimer = 0f;
                CheckIfReachedWaypoint();
            }
        }

        private void CheckIfReachedWaypoint()
        {
            if (aiCar == null || aiWaypoints == null || aiWaypoints.Count == 0) return;

            float distance = Vector3.Distance(aiCar.transform.position, transform.position);
            
            if (distance < 10f)
            {
                MoveToNextWaypoint();
            }
        }

        private void OnRestartRace()
        {
            ResetTracker();
        }

        private void ResetTracker()
        {
            if (aiWaypointSet == null || aiWaypointSet.Items == null || aiWaypointSet.Items.Count == 0)
                return;

            this.aiWaypoints = new List<AIWaypoint>(aiWaypointSet.Items);
            currentIndex = 0;
            UpdateTrackerPosition();
        }

        public void SetupAICarCollider(Collider aiCarCollider)
        {
            this.aiCarCollider = aiCarCollider;
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (aiCarCollider != null && collider.GetInstanceID() == aiCarCollider.GetInstanceID())
            {
                MoveToNextWaypoint();
            }
        }

        private void MoveToNextWaypoint()
        {
            if (aiWaypoints == null || aiWaypoints.Count == 0)
                return;

            currentIndex++;
            if (currentIndex >= aiWaypoints.Count)
                currentIndex = 0;

            UpdateTrackerPosition();
        }

        private void UpdateTrackerPosition()
        {
            if (aiWaypoints == null || aiWaypoints.Count == 0 || currentIndex >= aiWaypoints.Count)
                return;

            var waypoint = aiWaypoints[currentIndex];
            if (waypoint?.aiWaypointTransform != null)
            {
                currentWaypoint = waypoint.aiWaypointTransform;
                this.transform.position = currentWaypoint.position;
            }
        }

        private void OnDestroy()
        {
            if (gameEvents != null)
                gameEvents.RestartRaceEvent.RemoveListener(OnRestartRace);
        }
    }
}