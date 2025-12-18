using System.Collections.Generic;
using UnityEngine;

namespace SpinMotion
{
    /// <summary>
    /// Custom override for RealTimeRacePositions to fix checkpoint initialization
    /// Inherit from RealTimeRacePositions to maintain compatibility
    /// </summary>
    public class CustomRealTimeRacePositions : RealTimeRacePositions
    {
        private new void Awake()
        {
            realTimeRacePositionsRuntimeItem.Set(this);
            gameEvents.PlayersCheckpointTrackersAssignedEvent.AddListener(OnPlayersCheckpointTrackersAssigned);
            gameEvents.RaceStartedEvent.AddListener(OnCustomRaceStarted);
        }

        private void OnPlayersCheckpointTrackersAssigned(List<CheckpointTracker> playersCheckpointTrackers)
        {
            CarCheckpointTrackers.Clear();
            CarCheckpointTrackers.AddRange(playersCheckpointTrackers);
            Debug.Log($"[Custom] Assigned {CarCheckpointTrackers.Count} cars");
        }

        private void OnCustomRaceStarted()
        {
            DistanceFromCheckpointToCarTrackers.Clear();
            CheckpointScores.Clear();
            LapScores.Clear();
            RacePositionTotalScores.Clear();
            
            CarCheckpointTrackers.Sort((a, b) => a.GetCarRacePositionIndex().CompareTo(b.GetCarRacePositionIndex()));

            Debug.Log($"[Custom] Race Started! Total checkpoints: {RaceData.CheckpointsCount}");
            Debug.Log($"[Custom] Initializing {CarCheckpointTrackers.Count} cars with 0 checkpoints");
            
            for (int i = 0; i < CarCheckpointTrackers.Count; i++)
            {
                DistanceFromCheckpointToCarTrackers.Add(0);
                CheckpointScores.Add(0); // ← BẮT ĐẦU TỪ 0
                LapScores.Add(0);
                RacePositionTotalScores.Add(0);
                
                Debug.Log($"[Custom] Car {i} initialized: CheckpointScore=0, LapScore=0");
            }
        }

        private new void Update()
        {
            if (!raceManager.Item.IsRaceInProgress()) return;
            
            for (int i = 0; i < CarCheckpointTrackers.Count; i++)
            {
                RacePositionTotalScores[i] = DistanceFromCheckpointToCarTrackers[i] + (CheckpointScores[i] * 100) + (LapScores[i] * 10000);
            }
        }
    }
}