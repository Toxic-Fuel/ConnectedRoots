using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private string sceneName = "GridScene";

    public void Play()
    {
        if (InGameGenerationMenu.IsAnyMenuOpen)
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
