using System.Collections.Generic;
using UnityEngine;

namespace SpinMotion
{
    public class RealTimeRacePositions : MonoBehaviour
    {
        public RealTimeRacePositionsItem realTimeRacePositionsRuntimeItem;
        public GameEvents gameEvents;
        public RaceManagerItem raceManager;
        
        public List<CheckpointTracker> CarCheckpointTrackers = new();
        public List<double> DistanceFromCheckpointToCarTrackers = new();
        public List<int> CheckpointScores = new();
        public List<int> LapScores = new();
        public List<double> RacePositionTotalScores = new();

        private void Awake()
        {
            Debug.Log("[RealTime] Awake called!");
            
            realTimeRacePositionsRuntimeItem.Set(this);
            gameEvents.PlayersCheckpointTrackersAssignedEvent.AddListener(OnPlayersCheckpointTrackersAssigned);
            gameEvents.RaceStartedEvent.AddListener(OnRaceStarted);
            
            Debug.Log("[RealTime] Event listeners registered!");
        }

        private void Start()
        {
            Debug.Log("[RealTime] Start called!");
            
            // Khởi tạo ngay cho 4 xe (Player + 3 AI)
            Invoke(nameof(InitializeListsDelayed), 1f);
        }

        private void InitializeListsDelayed()
        {
            Debug.Log($"[RealTime] InitializeListsDelayed - Initializing for 4 cars");
            
            DistanceFromCheckpointToCarTrackers.Clear();
            CheckpointScores.Clear();
            LapScores.Clear();
            RacePositionTotalScores.Clear();
            
            // Khởi tạo cho 4 xe (có thể thay đổi số lượng)
            int totalCars = 4;
            
            for (int i = 0; i < totalCars; i++)
            {
                DistanceFromCheckpointToCarTrackers.Add(0);
                CheckpointScores.Add(0);
                LapScores.Add(0);
                RacePositionTotalScores.Add(0);
            }
            
            Debug.Log($"[RealTime] Lists initialized for {totalCars} cars! CheckpointScores.Count={CheckpointScores.Count}");
        }

        private void OnPlayersCheckpointTrackersAssigned(List<CheckpointTracker> playersCheckpointTrackers)
        {
            Debug.Log($"[RealTime] *** OnPlayersCheckpointTrackersAssigned CALLED! ***");
            CarCheckpointTrackers.Clear();
            CarCheckpointTrackers.AddRange(playersCheckpointTrackers);
            Debug.Log($"[RealTime] Assigned {CarCheckpointTrackers.Count} cars");
        }

        private void OnRaceStarted()
        {
            Debug.Log($"[RealTime] *** OnRaceStarted CALLED! ***");
            Debug.Log($"[RealTime] CheckpointScores.Count = {CheckpointScores.Count}");
            Debug.Log($"[RealTime] RaceData.CheckpointsCount = {RaceData.CheckpointsCount}");
        }

        public void Update()
        {
            if (raceManager.Item == null || !raceManager.Item.IsRaceInProgress())
            {
                return;
            }
            
            if (CheckpointScores.Count == 0)
            {
                return;
            }
            
            // Tính tổng điểm
            for (int i = 0; i < CheckpointScores.Count; i++)
            {
                RacePositionTotalScores[i] = DistanceFromCheckpointToCarTrackers[i] + (CheckpointScores[i] * 100) + (LapScores[i] * 10000);
            }
            
            // Debug mỗi 300 frames (5 giây)
            if (Time.frameCount % 300 == 0)
            {
                Debug.Log($"[RealTime] === UPDATE (Frame {Time.frameCount}) ===");
                for (int i = 0; i < CheckpointScores.Count; i++)
                {
                    Debug.Log($"[RealTime] Car {i}: Checkpoint={CheckpointScores[i]}, Lap={LapScores[i]}, Total={RacePositionTotalScores[i]:F2}");
                }
            }
        }

        public int GetPlayerRacePosition(int nPlayer)
        {
            if (nPlayer < 0 || nPlayer >= RacePositionTotalScores.Count)
                return 1;
                
            var maxPos = 1;

            for (int i = 0; i < RacePositionTotalScores.Count; i++)
            {
                if (RacePositionTotalScores[nPlayer] < RacePositionTotalScores[i])
                    maxPos = maxPos + 1;
            }

            return maxPos;
        }
    }
}