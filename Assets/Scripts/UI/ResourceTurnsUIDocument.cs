using UnityEngine;
using UnityEngine.UIElements;

public class ResourceTurnsUIDocument : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Turns turns;

    [Header("Panel Scale")]
    [SerializeField] private string hudRootName = "hud-root";
    [SerializeField, Min(0.1f)] private float panelScale = 1f;

    [Header("Label Names")]
    [SerializeField] private string turnsLabelName = "turns-value";
    [SerializeField] private string[] currentResourceLabelNames;
    [SerializeField] private string[] perTurnResourceLabelNames;

    [Header("Control Names")]
    [SerializeField] private string skipTurnButtonName = "skip-turn-button";

    private Label turnsLabel;
    private Label[] currentResourceLabels;
    private Label[] perTurnResourceLabels;
    private Button skipTurnButton;
    private VisualElement hudRoot;

    private void Awake()
    {
        CacheElements();
    }

    private void OnEnable()
    {
        CacheElements();
        ApplyPanelScale();
        BindControls();
    }

    private void OnDisable()
    {
        UnbindControls();
    }

    public void UpdateTexts(int[] currentResources, int[] resourcesPerTurn, int remainingTurns)
    {
        if (uiDocument == null)
        {
            return;
        }

        if (turnsLabel == null)
        {
            CacheElements();
        }

        if (turnsLabel != null)
        {
            turnsLabel.text = remainingTurns.ToString();
        }

        if (currentResources != null && currentResourceLabels != null)
        {
            for (int i = 0; i < currentResourceLabels.Length; i++)
            {
                if (currentResourceLabels[i] == null || i >= currentResources.Length)
                {
                    continue;
                }

                currentResourceLabels[i].text = currentResources[i].ToString();
            }
        }

        if (resourcesPerTurn != null && perTurnResourceLabels != null)
        {
            for (int i = 0; i < perTurnResourceLabels.Length; i++)
            {
                if (perTurnResourceLabels[i] == null || i >= resourcesPerTurn.Length)
                {
                    continue;
                }

                perTurnResourceLabels[i].text = $"+{resourcesPerTurn[i]}/t";
            }
        }
    }

    private void CacheElements()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                return;
            }
        }

        VisualElement root = uiDocument.rootVisualElement;
        if (root == null)
        {
            return;
        }

        turnsLabel = string.IsNullOrWhiteSpace(turnsLabelName) ? null : root.Q<Label>(turnsLabelName);

        currentResourceLabels = ResolveLabels(root, currentResourceLabelNames);
        perTurnResourceLabels = ResolveLabels(root, perTurnResourceLabelNames);
        skipTurnButton = string.IsNullOrWhiteSpace(skipTurnButtonName) ? null : root.Q<Button>(skipTurnButtonName);
        hudRoot = string.IsNullOrWhiteSpace(hudRootName) ? root : root.Q<VisualElement>(hudRootName) ?? root;
    }

    private void OnValidate()
    {
        panelScale = Mathf.Max(0.1f, panelScale);

        if (!Application.isPlaying)
        {
            return;
        }

        CacheElements();
        ApplyPanelScale();
    }

    private void ApplyPanelScale()
    {
        if (hudRoot == null)
        {
            return;
        }

        hudRoot.style.position = Position.Absolute;
        hudRoot.style.left = 0f;
        hudRoot.style.top = 0f;
        hudRoot.style.transformOrigin = new TransformOrigin(
            new Length(0f, LengthUnit.Percent),
            new Length(0f, LengthUnit.Percent),
            0f
        );
        hudRoot.style.scale = new Scale(new Vector3(panelScale, panelScale, 1f));
    }

    private void BindControls()
    {
        if (skipTurnButton == null)
        {
            return;
        }

        skipTurnButton.clicked -= OnSkipTurnClicked;
        skipTurnButton.clicked += OnSkipTurnClicked;
    }

    private void UnbindControls()
    {
        if (skipTurnButton == null)
        {
            return;
        }

        skipTurnButton.clicked -= OnSkipTurnClicked;
    }

    private void OnSkipTurnClicked()
    {
        EnsureTurnsReference();
        if (turns == null)
        {
            return;
        }

        turns.EndTurn();
    }

    private void EnsureTurnsReference()
    {
        if (turns == null)
        {
            turns = FindAnyObjectByType<Turns>();
        }
    }

    private static Label[] ResolveLabels(VisualElement root, string[] names)
    {
        if (names == null)
        {
            return new Label[0];
        }

        Label[] labels = new Label[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(names[i]))
            {
                labels[i] = null;
                continue;
            }

            labels[i] = root.Q<Label>(names[i]);
        }

        return labels;
    }
}
