using UnityEngine;

namespace SpinMotion
{
    public class RaceManager : MonoBehaviour
    {
        public RaceManagerItem raceManagerRuntimeItem;
        public GameEvents gameEvents;
        [Header("Race Timer Settings:")]
        public bool isTimedRace;
        public int raceTimerSeconds;
        
        [Header("Auto Start Settings:")]
        public bool autoStartRace = true;
        public float autoStartDelay = 2f;
        
        private bool isRaceInProgress;

        private void Awake()
        {
            raceManagerRuntimeItem.Set(this);
            gameEvents.RaceStartedEvent.AddListener(OnRaceStarted);
            gameEvents.RaceFinishedEvent.AddListener(OnRaceFinished);
            gameEvents.OnClickPlayRaceEvent.AddListener(OnPlayRace);
            gameEvents.OnClickRestartRaceEvent.AddListener(OnRestartRace);
        }

        private void Start()
        {
            Debug.Log("[RaceManager] Start");
            
            // Set số vòng mặc định nếu chưa có
            if (RaceData.LapsSelected == 0)
            {
                RaceData.LapsSelected = 3; // Mặc định 3 vòng
                Debug.Log("[RaceManager] Set default laps: 3");
            }
            
            Debug.Log($"[RaceManager] Race will be {RaceData.LapsSelected} laps with {RaceData.CheckpointsCount} checkpoints per lap");
            
            // Force start race
            Invoke(nameof(ForceStartRace), 1f);
        }

        private void ForceStartRace()
        {
            isRaceInProgress = true;
            Debug.Log("[RaceManager] ★★★ FORCE START - isRaceInProgress = TRUE ★★★");
        }

        private void OnPlayRace()
        {
            gameEvents.SpawnPlayersEvent.Invoke();
            gameEvents.PreRaceUpdateGuiEvent.Invoke();
            gameEvents.PlayPreRaceCountdownEvent.Invoke();
            gameEvents.ChangeToRaceCamerasEvent.Invoke();
        }

        private void OnRaceStarted()
        {
            isRaceInProgress = true;
            Debug.Log($"[RaceManager] Race started event - isRaceInProgress = TRUE");
        }

        private void OnRaceFinished(RaceFinishType raceFinishType)
        {
            isRaceInProgress = false;
        }

        private void OnRestartRace()
        {
            isRaceInProgress = false;
            gameEvents.RestartRaceEvent.Invoke();
            gameEvents.PreRaceUpdateGuiEvent.Invoke();
            gameEvents.PlayPreRaceCountdownEvent.Invoke();
        }
        
        public bool IsRaceInProgress()
        {
            return isRaceInProgress;
        }
    }

    public class RaceData
    {
        public static int CheckpointsCount;
        public static int AiBotsSelected;
        public static int LapsSelected;
    }
}