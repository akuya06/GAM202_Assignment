using UnityEngine;

namespace SpinMotion
{
    public class CheckpointTracker : MonoBehaviour
    {
        public GameEvents gameEvents;
        public RaceManagerItem raceManager;
        public RealTimeRacePositionsItem realTimeRacePositions;

        private int carRacePositionIndex = -1;
        
        public void SetCarRacePositionIndex(int carRacePositionIndex)
        {
            this.carRacePositionIndex = carRacePositionIndex;
            Debug.Log($"[CheckpointTracker] {gameObject.name} index set to {carRacePositionIndex}");
        }
        
        public int GetCarRacePositionIndex() { return carRacePositionIndex; }

        private int currentCheckpoint;
        private int nextCheckpoint;

        private void Awake()
        {
            gameEvents.RaceStartedEvent.AddListener(OnRaceStarted);
        }

        private void Start()
        {
            if (carRacePositionIndex == -1)
            {
                AutoDetectIndexByTag();
            }
        }

        private void AutoDetectIndexByTag()
        {
            // Tìm tag trong object này hoặc parent
            Transform current = transform;
            
            while (current != null)
            {
                string tag = current.gameObject.tag;
                
                switch (tag)
                {
                    case "Player":
                        carRacePositionIndex = 0;
                        Debug.Log($"[CheckpointTracker] Found tag 'Player' on '{current.gameObject.name}' → index 0");
                        return;
                        
                    case "Bot1":
                        carRacePositionIndex = 1;
                        Debug.Log($"[CheckpointTracker] Found tag 'Bot1' on '{current.gameObject.name}' → index 1");
                        return;
                        
                    case "Bot2":
                        carRacePositionIndex = 2;
                        Debug.Log($"[CheckpointTracker] Found tag 'Bot2' on '{current.gameObject.name}' → index 2");
                        return;
                        
                    case "Bot3":
                        carRacePositionIndex = 3;
                        Debug.Log($"[CheckpointTracker] Found tag 'Bot3' on '{current.gameObject.name}' → index 3");
                        return;
                }
                
                current = current.parent;
            }
            
            // Fallback nếu không tìm được tag
            Debug.LogError($"[CheckpointTracker] No valid tag found for '{gameObject.name}'! Please set tag to Player/Bot1/Bot2/Bot3");
            carRacePositionIndex = 0; // Default to Player
        }

        private void OnRaceStarted()
        {
            currentCheckpoint = 0;
            nextCheckpoint = 1;
            Debug.Log($"[CheckpointTracker] Car {carRacePositionIndex} ({gameObject.name}) ready, waiting for checkpoint 1");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (carRacePositionIndex < 0 || carRacePositionIndex >= realTimeRacePositions.Item.CheckpointScores.Count)
            {
                Debug.LogError($"[CheckpointTracker] {gameObject.name}: Invalid index {carRacePositionIndex}!");
                return;
            }
            
            var checkpoint = other.GetComponent<Checkpoint>();
            if (checkpoint == null) 
            {
                checkpoint = other.GetComponentInParent<Checkpoint>();
                if (checkpoint == null) return;
            }
            
            var checkpointNumber = checkpoint.GetNumber();
            
            realTimeRacePositions.Item.DistanceFromCheckpointToCarTrackers[carRacePositionIndex] = 0;
            
            if (currentCheckpoint + 1 == checkpointNumber)
            {
                realTimeRacePositions.Item.CheckpointScores[carRacePositionIndex] = checkpointNumber;
                gameEvents.CheckpointPassedEvent.Invoke(carRacePositionIndex, checkpointNumber);
                
                Debug.Log($"[CheckpointTracker] ★ Car {carRacePositionIndex}: Checkpoint {checkpointNumber} PASSED!");
                
                currentCheckpoint++;
                nextCheckpoint++;

                if (currentCheckpoint >= RaceData.CheckpointsCount)
                {
                    realTimeRacePositions.Item.LapScores[carRacePositionIndex]++;
                    
                    int currentLap = realTimeRacePositions.Item.LapScores[carRacePositionIndex];
                    Debug.Log($"[CheckpointTracker] 🏁 Car {carRacePositionIndex}: LAP {currentLap}/{RaceData.LapsSelected} COMPLETED!");
                    
                    if (carRacePositionIndex == 0)
                    {
                        gameEvents.LapCompletedEvent.Invoke();
                    }
                    
                    currentCheckpoint = 0;
                    nextCheckpoint = 1;
                    realTimeRacePositions.Item.CheckpointScores[carRacePositionIndex] = 0;
                }
            }
            else if (nextCheckpoint > RaceData.CheckpointsCount)
            {
                currentCheckpoint = 0;
                nextCheckpoint = 1;
                realTimeRacePositions.Item.CheckpointScores[carRacePositionIndex] = 0;
            }
        }
    }
}