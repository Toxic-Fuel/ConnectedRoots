using UnityEngine;

public class LoseCondition : MonoBehaviour
{
    public Turns turns;

    void Update()
    {
        if (turns.RemainingTurns <= 0)
        {
            turns.SetLoseState();
        }
    }
}
