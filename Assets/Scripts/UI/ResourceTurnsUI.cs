using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceTurnsUI : MonoBehaviour
{
    [Header("Resource Icons")]
    [SerializeField] private Sprite[] sprites;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject CurrentResourcePrefab;
    [SerializeField] private GameObject ResourcesPerTurnPrefab;

    [Header("UI References")]
    [SerializeField] private TMP_Text turnsText;
    [SerializeField] private GameObject CurrentResourcesParent;
    [SerializeField] private GameObject ResourcesPerTurnParent;

    private TMP_Text[] currentResourcesTexts;
    private TMP_Text[] resourcesPerTurnTexts;
    
    // private void Awake()
    // {
    //     CreateResourceTexts();
    // }

    private void CreateResourceTexts()
    {
        int resourceCount = sprites.Length;
        currentResourcesTexts = new TMP_Text[resourceCount];
        resourcesPerTurnTexts = new TMP_Text[resourceCount];

        for (int i = 0; i < resourceCount; i++)
        {
            // Create Current Resource Text
            GameObject currentResourceGO = Instantiate(CurrentResourcePrefab, CurrentResourcesParent.transform);
            currentResourceGO.GetComponentInChildren<TMP_Text>().text = "0";
            currentResourceGO.GetComponentInChildren<TMP_Text>().color = Color.black;
            currentResourceGO.GetComponentInChildren<Image>().sprite = sprites[i];
            currentResourcesTexts[i] = currentResourceGO.GetComponentInChildren<TMP_Text>();
            

            // Create Resources Per Turn Text
            GameObject resourcesPerTurnGO = Instantiate(ResourcesPerTurnPrefab, ResourcesPerTurnParent.transform);
            resourcesPerTurnGO.GetComponentInChildren<TMP_Text>().text = "+0/t";
            resourcesPerTurnGO.GetComponentInChildren<TMP_Text>().color = Color.black;
            resourcesPerTurnGO.GetComponentInChildren<Image>().sprite = sprites[i];
            resourcesPerTurnTexts[i] = resourcesPerTurnGO.GetComponentInChildren<TMP_Text>();
        }
        RectTransform resourcesPerTurnRect = ResourcesPerTurnParent.GetComponent<RectTransform>();
        // if (resourcesPerTurnRect != null)
        // {
        //     Vector2 size = resourcesPerTurnRect.sizeDelta;
        //     size.y = resourceCount * 150f; // Adjust height based on number of resources
        //     resourcesPerTurnRect.sizeDelta = size;
        //
        //     // Align to top-left corner
        //     // Align to top-right corner
        //     // resourcesPerTurnRect.anchorMin = new Vector2(1, 1);
        //     // resourcesPerTurnRect.anchorMax = new Vector2(1, 1);
        //     // resourcesPerTurnRect.pivot = new Vector2(1, 1);
        //     //resourcesPerTurnRect.anchoredPosition = Vector2.zero;
        // }
    }
    public void UpdateTexts(int[] currentResources, int[] resourcesPerTurn, int remainingTurns)
    {
        if(currentResourcesTexts == null || resourcesPerTurnTexts == null)
        {
            CreateResourceTexts();
            if(currentResourcesTexts == null || resourcesPerTurnTexts == null)
            {
                Debug.LogWarning("ResourceTurnsUI: Text arrays not initialized.", this);
                return;
            }
        }
        for (int resourceIndex = 0; resourceIndex < currentResources.Length-1; resourceIndex++)
        {
            currentResourcesTexts[resourceIndex].text = currentResources[resourceIndex+1].ToString();
            resourcesPerTurnTexts[resourceIndex].text = $"+{resourcesPerTurn[resourceIndex+1]}/t";
        }

        turnsText.text = remainingTurns.ToString();

      
    }
}
