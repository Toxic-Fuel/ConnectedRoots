using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace UI
{
    public static class EventSystemRuntimeGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureEventSystemReady();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureEventSystemReady();
        }

        private static void EnsureEventSystemReady()
        {
            EventSystem[] allEventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            EventSystem primary = SelectPrimaryEventSystem(allEventSystems);

            if (primary == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem");
                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene.IsValid())
                {
                    SceneManager.MoveGameObjectToScene(eventSystemObject, activeScene);
                }

                primary = eventSystemObject.AddComponent<EventSystem>();
            }

            if (!primary.enabled)
            {
                primary.enabled = true;
            }

            InputSystemUIInputModule inputSystemModule = primary.GetComponent<InputSystemUIInputModule>();
            if (inputSystemModule == null)
            {
                inputSystemModule = primary.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            if (!inputSystemModule.enabled)
            {
                inputSystemModule.enabled = true;
            }

            StandaloneInputModule standaloneModule = primary.GetComponent<StandaloneInputModule>();
            if (standaloneModule != null)
            {
                Object.Destroy(standaloneModule);
            }

            for (int i = 0; i < allEventSystems.Length; i++)
            {
                EventSystem other = allEventSystems[i];
                if (other == null || other == primary)
                {
                    continue;
                }

                other.enabled = false;
            }
        }

        private static EventSystem SelectPrimaryEventSystem(EventSystem[] allEventSystems)
        {
            if (allEventSystems == null || allEventSystems.Length == 0)
            {
                return null;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            for (int i = 0; i < allEventSystems.Length; i++)
            {
                EventSystem eventSystem = allEventSystems[i];
                if (eventSystem != null && eventSystem.gameObject.scene == activeScene)
                {
                    return eventSystem;
                }
            }

            return allEventSystems[0];
        }
    }
}
