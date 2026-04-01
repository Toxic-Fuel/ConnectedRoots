using UnityEngine;

namespace QuestSystem
{
    public class Quest : MonoBehaviour
    {
        public string questName;
        public string description;
        [SerializeField] private Turns turns;
        [SerializeField] private AudioSource completionAudioSource;
        [SerializeField] private int[] rewardResources;
        [SerializeField] private int[] resourceCosts;

        private bool _isCompleted;
        public bool IsCompleted => _isCompleted;

        public void SetTurns(Turns turnsReference)
        {
            if (turnsReference == null)
            {
                return;
            }

            turns = turnsReference;
        }

        public bool PayResources()
        {
            if (_isCompleted)
            {
                return false;
            }

            if (turns == null)
            {
                Debug.LogError("Quest: Turns reference is missing.", this);
                return false;
            }

            if (!turns.CanAffordResources(resourceCosts) || !turns.CanAffordTurns(resourceCosts[(int)ResourceType.Turn]))
            {
                return false;
            }

            turns.TrySpendResources(resourceCosts);
            turns.TrySpendTurns(resourceCosts[0]);

            OnQuestComplete();
            return true;
        }

        private void OnQuestComplete()
        {
            if (_isCompleted)
            {
                return;
            }

            if (turns == null)
            {
                Debug.LogError("Quest: Turns reference is missing.", this);
                return;
            }

            turns.AddPerTurnResources(rewardResources);
            _isCompleted = true;

            if (completionAudioSource != null)
            {
                completionAudioSource.Play();
            }
        }
    }
}
