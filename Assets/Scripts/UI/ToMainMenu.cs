using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ToMainMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool _isTransitioning;
    private InputAction _returnToMenuAction;

    private void OnEnable()
    {
        if (_returnToMenuAction == null)
        {
            _returnToMenuAction = new InputAction("ReturnToMenu", InputActionType.Button, "<Keyboard>/m");
        }

        _returnToMenuAction.performed += OnReturnToMenuPerformed;
        _returnToMenuAction.Enable();
    }

    private void OnDisable()
    {
        if (_returnToMenuAction == null)
        {
            return;
        }

        _returnToMenuAction.performed -= OnReturnToMenuPerformed;
        _returnToMenuAction.Disable();
    }

    private void OnDestroy()
    {
        _returnToMenuAction?.Dispose();
        _returnToMenuAction = null;
    }

    private void OnReturnToMenuPerformed(InputAction.CallbackContext context)
    {
        if (_isTransitioning)
        {
            return;
        }

        StartCoroutine(ReturnToMainMenuCoroutine());
    }

    private System.Collections.IEnumerator ReturnToMainMenuCoroutine()
    {
        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError($"ToMainMenu: Scene '{mainMenuSceneName}' is not in Build Settings.", this);
            yield break;
        }

        _isTransitioning = true;

        // Ensure main menu is loaded, then unload every other loaded scene.
        if (!SceneManager.GetSceneByName(mainMenuSceneName).isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Additive);
        }

        int loadedSceneCount = SceneManager.sceneCount;
        for (int i = loadedSceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.IsValid() || !scene.isLoaded || scene.name == mainMenuSceneName)
            {
                continue;
            }

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        Scene mainMenuScene = SceneManager.GetSceneByName(mainMenuSceneName);
        if (mainMenuScene.IsValid() && mainMenuScene.isLoaded)
        {
            SceneManager.SetActiveScene(mainMenuScene);
        }

        _isTransitioning = false;
    }
}
