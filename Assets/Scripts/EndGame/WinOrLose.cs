using UnityEngine;
using UnityEngine.SceneManagement;

public class WinOrLose : MonoBehaviour
{
    [SerializeField] private string victorySceneName = "VictoryScene";
    [SerializeField] private string defeatSceneName = "DefeatScene";

    public void CheckWinOrLose(Turns.TurnState state)
    {
        Debug.Log("Checking win or lose condition: " + state);

        if (state == Turns.TurnState.Win)
        {
            LoadSceneSafe(victorySceneName);
        }
        else if (state == Turns.TurnState.Lose)
        {
            LoadSceneSafe(defeatSceneName);
        }
    }

    private void LoadSceneSafe(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"WinOrLose: Scene '{sceneName}' is not in Build Profiles / shared scene list.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
