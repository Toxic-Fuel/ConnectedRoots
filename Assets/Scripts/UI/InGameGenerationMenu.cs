using GridGeneration;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InGameGenerationMenu : MonoBehaviour
{
    private enum DifficultyPreset
    {
        Easy,
        Medium,
        Hard,
        Extreme
    }

    [Header("References")]
    [SerializeField] private GridMap gridMap;

    [Header("Menu")]
    [SerializeField] private InputActionReference toggleMenuAction;
    [SerializeField] private bool showOnStart;
    [SerializeField] private bool persistAcrossScenes = true;
    [SerializeField] private bool blockGameplayInputWhileOpen = true;
    [SerializeField] private bool blockSceneUiInputWhileMenuIsOpen = true;
    [SerializeField] private bool mainMenuOnlyUntilGridMapFound = true;
    [SerializeField] private bool showFloatingToggleButton = true;
    [SerializeField] private Vector2 floatingButtonSize = new Vector2(140f, 58f);
    [SerializeField] private bool autoFindGridMap = true;
    [SerializeField] private bool autoApplyPendingChangesWhenGridMapAppears = true;

    [Header("Typography")]
    [SerializeField, Range(14, 48)] private int windowTitleFontSize = 28;
    [SerializeField, Range(12, 42)] private int contentFontSize = 24;
    [SerializeField, Range(12, 48)] private int buttonFontSize = 24;
    [SerializeField, Range(12, 42)] private int floatingButtonFontSize = 22;
    [SerializeField, Range(24f, 84f)] private float controlHeight = 40f;

    [Header("Theme")]
    [SerializeField] private Color menuWindowColor = new Color(0.91f, 0.89f, 0.80f, 0.97f);
    [SerializeField] private Color menuTextColor = new Color(0.08f, 0.08f, 0.08f, 1f);
    [SerializeField] private Color menuButtonColor = new Color(0.68f, 0.72f, 0.52f, 1f);
    [SerializeField] private Color menuButtonHoverColor = new Color(0.60f, 0.66f, 0.46f, 1f);
    [SerializeField] private Color menuButtonActiveColor = new Color(0.44f, 0.50f, 0.32f, 1f);
    [SerializeField] private Color menuInputFieldColor = new Color(0.85f, 0.84f, 0.74f, 1f);
    [SerializeField] private Color menuInputFieldBorderColor = new Color(0.26f, 0.30f, 0.18f, 1f);
    [SerializeField] private Color menuSliderTrackColor = new Color(0.39f, 0.43f, 0.29f, 1f);
    [SerializeField] private Color menuSliderThumbColor = new Color(0.24f, 0.28f, 0.17f, 1f);
    [SerializeField] private Color menuOutlineColor = new Color(0.06f, 0.06f, 0.06f, 1f);
    [SerializeField, Range(1f, 8f)] private float menuOutlineThickness = 3f;
    [SerializeField] private Color floatingButtonBaseColor = new Color(0.52f, 0.58f, 0.36f, 1f);
    [SerializeField] private Color floatingButtonHoverColor = new Color(0.60f, 0.66f, 0.42f, 1f);
    [SerializeField] private Color floatingButtonActiveColor = new Color(0.34f, 0.41f, 0.24f, 1f);
    [SerializeField] private Color floatingButtonTextColor = new Color(0.08f, 0.08f, 0.08f, 1f);
    [SerializeField] private Color floatingButtonAccentColor = new Color(0.84f, 0.83f, 0.67f, 1f);
    [SerializeField] private Color floatingButtonShadowColor = new Color(0f, 0f, 0f, 0.35f);

    [Header("Regeneration")]
    [SerializeField] private bool regenerateImmediatelyOnApply = true;
    [SerializeField] private bool regenerateWhenApplyingPendingChanges = true;

    private const int WindowId = 92741;
    private Rect windowRect = new Rect(16f, 120f, 640f, 560f);
    private Vector2 scrollPosition;
    private bool isOpen;
    private bool pendingApplyWithoutGridMap;
    private bool centerWindowOnNextDraw = true;
    private EventSystem blockedEventSystem;
    private bool blockedEventSystemPreviousEnabled;

    private Texture2D windowTexture;
    private Texture2D buttonTexture;
    private Texture2D buttonHoverTexture;
    private Texture2D buttonActiveTexture;
    private Texture2D inputFieldTexture;
    private Texture2D sliderTrackTexture;
    private Texture2D sliderThumbTexture;
    private Texture2D outlineTexture;
    private Texture2D floatingButtonTexture;
    private Texture2D floatingButtonHoverTexture;
    private Texture2D floatingButtonActiveTexture;
    private Texture2D floatingButtonAccentTexture;
    private Texture2D floatingButtonShadowTexture;

    private static InGameGenerationMenu instance;

    public static bool IsAnyMenuOpen { get; private set; }

    private float obstaclePercent = 0.30f;
    private bool limitMineSourceTiles = true;
    private float maxMineSourcePercent = 0.05f;
    private int minMineSourceTiles = 6;
    private bool guaranteeStarterResources = true;
    private int starterMinDistance = 2;
    private int starterRadius = 4;
    private bool preserveNearestMine = true;
    private string seedText = "0";

    private void Awake()
    {
        if (persistAcrossScenes)
        {
            if (instance != null && instance != this)
            {
                bool hostsEventSystem = GetComponent<EventSystem>() != null || GetComponent<BaseInputModule>() != null;
                if (hostsEventSystem)
                {
                    Destroy(this);
                }
                else
                {
                    Destroy(gameObject);
                }

                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (gridMap == null)
        {
            TryResolveGridMap();
        }

        isOpen = showOnStart;
        SetMenuOpenState(isOpen);

        if (!CanDisplayMenu())
        {
            SetMenuOpenState(false);
        }

        if (gridMap != null)
        {
            PullFromGridMap();
        }
    }

    private void OnEnable()
    {
        if (toggleMenuAction != null && toggleMenuAction.action != null)
        {
            toggleMenuAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (toggleMenuAction != null && toggleMenuAction.action != null)
        {
            toggleMenuAction.action.Disable();
        }

        SetMenuOpenState(false);
        RestoreBlockedEventSystem();

        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnDestroy()
    {
        DestroyThemeTextures();
    }

    private void Update()
    {
        TryResolveGridMap();

        if (!CanDisplayMenu())
        {
            SetMenuOpenState(false);
            return;
        }

        bool shouldToggle = false;

        if (toggleMenuAction != null && toggleMenuAction.action != null && toggleMenuAction.action.WasPressedThisFrame())
        {
            shouldToggle = true;
        }

        if (!shouldToggle && Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame)
        {
            shouldToggle = true;
        }

        if (!shouldToggle && isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            shouldToggle = true;
        }

        if (shouldToggle)
        {
            ToggleMenu();
        }
    }

    private void OnGUI()
    {
        if (!CanDisplayMenu())
        {
            return;
        }

        EnsureThemeTextures();

        DrawFloatingToggleButton();

        if (!isOpen)
        {
            return;
        }

        float maxWidth = Mathf.Max(360f, Screen.width - 24f);
        float maxHeight = Mathf.Max(300f, Screen.height - 24f);
        windowRect.width = Mathf.Min(windowRect.width, maxWidth);
        windowRect.height = Mathf.Min(windowRect.height, maxHeight);

        if (centerWindowOnNextDraw)
        {
            windowRect.x = (Screen.width - windowRect.width) * 0.5f;
            windowRect.y = (Screen.height - windowRect.height) * 0.5f;
            centerWindowOnNextDraw = false;
        }

        windowRect.x = Mathf.Clamp(windowRect.x, 0f, Screen.width - windowRect.width);
        windowRect.y = Mathf.Clamp(windowRect.y, 0f, Screen.height - windowRect.height);

        GUIStyle windowStyle = new GUIStyle(GUI.skin.window)
        {
            fontSize = windowTitleFontSize,
            alignment = TextAnchor.UpperCenter,
            padding = new RectOffset(12, 12, 12, 12)
        };

        windowStyle.normal.background = windowTexture;
        windowStyle.onNormal.background = windowTexture;
        windowStyle.active.background = windowTexture;
        windowStyle.focused.background = windowTexture;
        windowStyle.normal.textColor = menuTextColor;
        windowStyle.onNormal.textColor = menuTextColor;
        windowStyle.active.textColor = menuTextColor;
        windowStyle.focused.textColor = menuTextColor;

        windowRect = GUI.Window(WindowId, windowRect, DrawWindow, "World Generation", windowStyle);
        DrawOutline(windowRect, menuOutlineThickness, outlineTexture);
    }

    private void ToggleMenu()
    {
        if (!CanDisplayMenu())
        {
            SetMenuOpenState(false);
            return;
        }

        SetMenuOpenState(!isOpen);
        if (isOpen)
        {
            centerWindowOnNextDraw = true;
            if (gridMap != null)
            {
                PullFromGridMap();
            }
        }
    }

    private void SetMenuOpenState(bool open)
    {
        isOpen = open;
        IsAnyMenuOpen = blockGameplayInputWhileOpen && isOpen;
        UpdateEventSystemBlockState();
    }

    private void UpdateEventSystemBlockState()
    {
        if (!blockSceneUiInputWhileMenuIsOpen)
        {
            RestoreBlockedEventSystem();
            return;
        }

        bool shouldBlock = IsAnyMenuOpen && CanDisplayMenu();
        if (!shouldBlock)
        {
            RestoreBlockedEventSystem();
            return;
        }

        EventSystem currentEventSystem = EventSystem.current;
        if (currentEventSystem == null)
        {
            return;
        }

        if (blockedEventSystem != currentEventSystem)
        {
            RestoreBlockedEventSystem();
            blockedEventSystem = currentEventSystem;
            blockedEventSystemPreviousEnabled = currentEventSystem.enabled;
        }

        if (currentEventSystem.enabled)
        {
            currentEventSystem.enabled = false;
        }
    }

    private void RestoreBlockedEventSystem()
    {
        if (blockedEventSystem != null)
        {
            blockedEventSystem.enabled = blockedEventSystemPreviousEnabled;
            blockedEventSystem = null;
        }
    }

    private void TryResolveGridMap()
    {
        if (gridMap != null || !autoFindGridMap)
        {
            return;
        }

        gridMap = FindAnyObjectByType<GridMap>();
        if (gridMap == null)
        {
            return;
        }

        if (pendingApplyWithoutGridMap && autoApplyPendingChangesWhenGridMapAppears)
        {
            PushToGridMap();
            if (regenerateWhenApplyingPendingChanges)
            {
                gridMap.GenerateLandMap();
            }

            pendingApplyWithoutGridMap = false;
        }

        if (CanDisplayMenu())
        {
            PullFromGridMap();
        }
        else
        {
            SetMenuOpenState(false);
        }
    }

    private bool CanDisplayMenu()
    {
        return !mainMenuOnlyUntilGridMapFound || gridMap == null;
    }

    private void DrawFloatingToggleButton()
    {
        if (!showFloatingToggleButton)
        {
            return;
        }

        string buttonLabel = isOpen ? "Close Menu" : "Gen Menu";
        GUIStyle floatingButtonStyle = CreateFloatingButtonStyle(floatingButtonFontSize);

        float buttonWidth = Mathf.Clamp(floatingButtonSize.x, 100f, 280f);
        float buttonHeight = Mathf.Clamp(floatingButtonSize.y, 44f, 120f);

        // Ensure the text always fits inside the floating button.
        Vector2 textSize = floatingButtonStyle.CalcSize(new GUIContent(buttonLabel));
        float requiredWidth = textSize.x + floatingButtonStyle.padding.left + floatingButtonStyle.padding.right + 20f;
        float maxAllowedWidth = Mathf.Max(140f, Screen.width * 0.45f);
        buttonWidth = Mathf.Clamp(Mathf.Max(buttonWidth, requiredWidth), 100f, maxAllowedWidth);

        Rect buttonRect = new Rect(
            0f,
            Screen.height - buttonHeight,
            buttonWidth,
            buttonHeight);

        Rect shadowRect = new Rect(buttonRect.x + 3f, buttonRect.y + 3f, buttonRect.width, buttonRect.height);
        GUI.DrawTexture(shadowRect, floatingButtonShadowTexture);
        GUI.DrawTexture(new Rect(buttonRect.x, buttonRect.y, buttonRect.width, 5f), floatingButtonAccentTexture);

        if (GUI.Button(buttonRect, buttonLabel, floatingButtonStyle))
        {
            ToggleMenu();
        }
    }

    private void DrawWindow(int id)
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = contentFontSize,
            richText = false
        };

        labelStyle.normal.textColor = menuTextColor;
        labelStyle.onNormal.textColor = menuTextColor;
        labelStyle.hover.textColor = menuTextColor;
        labelStyle.onHover.textColor = menuTextColor;
        labelStyle.active.textColor = menuTextColor;
        labelStyle.onActive.textColor = menuTextColor;
        labelStyle.focused.textColor = menuTextColor;
        labelStyle.onFocused.textColor = menuTextColor;

        GUIStyle sectionStyle = new GUIStyle(labelStyle)
        {
            fontStyle = FontStyle.Bold,
            fontSize = contentFontSize + 2
        };

        GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle)
        {
            fontSize = contentFontSize,
            normal = { textColor = menuTextColor },
            onNormal = { textColor = menuTextColor },
            hover = { textColor = menuTextColor },
            onHover = { textColor = menuTextColor },
            active = { textColor = menuTextColor },
            onActive = { textColor = menuTextColor },
            focused = { textColor = menuTextColor },
            onFocused = { textColor = menuTextColor }
        };

        GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField)
        {
            fontSize = contentFontSize,
            padding = new RectOffset(10, 10, 6, 6)
        };

        textFieldStyle.normal.background = inputFieldTexture;
        textFieldStyle.focused.background = inputFieldTexture;
        textFieldStyle.hover.background = inputFieldTexture;
        textFieldStyle.active.background = inputFieldTexture;
        textFieldStyle.normal.textColor = menuTextColor;
        textFieldStyle.onNormal.textColor = menuTextColor;
        textFieldStyle.focused.textColor = menuTextColor;
        textFieldStyle.onFocused.textColor = menuTextColor;
        textFieldStyle.hover.textColor = menuTextColor;
        textFieldStyle.onHover.textColor = menuTextColor;
        textFieldStyle.active.textColor = menuTextColor;
        textFieldStyle.onActive.textColor = menuTextColor;

        GUIStyle buttonStyle = CreateButtonStyle(buttonFontSize);

        GUIStyle sliderStyle = new GUIStyle(GUI.skin.horizontalSlider)
        {
            fixedHeight = 10f,
            margin = new RectOffset(4, 4, 8, 8),
            border = new RectOffset(0, 0, 0, 0)
        };
        sliderStyle.normal.background = sliderTrackTexture;

        GUIStyle sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
        {
            fixedHeight = 22f,
            fixedWidth = 22f,
            margin = new RectOffset(0, 0, 0, 0)
        };
        sliderThumbStyle.normal.background = sliderThumbTexture;
        sliderThumbStyle.hover.background = buttonHoverTexture;
        sliderThumbStyle.active.background = buttonActiveTexture;

        float clampedControlHeight = Mathf.Clamp(controlHeight, 24f, 100f);
        GUILayoutOption[] controlHeightOption = { GUILayout.Height(clampedControlHeight) };

        Rect contentRect = new Rect(8f, 26f, windowRect.width - 16f, windowRect.height - 34f);
        GUILayout.BeginArea(contentRect);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.ExpandHeight(true));

        GUILayout.Label("Seed", sectionStyle);
        seedText = GUILayout.TextField(seedText ?? string.Empty, textFieldStyle, controlHeightOption);
        seedText = SanitizeSeedInput(seedText);

        GUILayout.Space(6f);
        GUILayout.Label($"Obstacle Percent: {obstaclePercent:0.00}", labelStyle);
        obstaclePercent = GUILayout.HorizontalSlider(obstaclePercent, 0f, 0.85f, sliderStyle, sliderThumbStyle);

        GUILayout.Space(6f);
        limitMineSourceTiles = GUILayout.Toggle(limitMineSourceTiles, "Limit Mine Sources", toggleStyle, controlHeightOption);
        if (limitMineSourceTiles)
        {
            GUILayout.Label($"Max Mine Source Percent: {maxMineSourcePercent:0.000}", labelStyle);
            maxMineSourcePercent = GUILayout.HorizontalSlider(maxMineSourcePercent, 0f, 0.15f, sliderStyle, sliderThumbStyle);

            GUILayout.Label($"Min Mine Source Tiles: {minMineSourceTiles}", labelStyle);
            minMineSourceTiles = Mathf.RoundToInt(GUILayout.HorizontalSlider(minMineSourceTiles, 0f, 30f, sliderStyle, sliderThumbStyle));
            preserveNearestMine = GUILayout.Toggle(preserveNearestMine, "Preserve nearest mine to city", toggleStyle, controlHeightOption);
        }

        GUILayout.Space(6f);
        guaranteeStarterResources = GUILayout.Toggle(guaranteeStarterResources, "Guarantee Starter Resource Nodes", toggleStyle, controlHeightOption);
        if (guaranteeStarterResources)
        {
            GUILayout.Label($"Starter Min Distance: {starterMinDistance}", labelStyle);
            starterMinDistance = Mathf.RoundToInt(GUILayout.HorizontalSlider(starterMinDistance, 1f, 8f, sliderStyle, sliderThumbStyle));

            int minRadius = Mathf.Max(1, starterMinDistance);
            starterRadius = Mathf.Max(minRadius, starterRadius);
            GUILayout.Label($"Starter Radius: {starterRadius}", labelStyle);
            starterRadius = Mathf.RoundToInt(GUILayout.HorizontalSlider(starterRadius, minRadius, 12f, sliderStyle, sliderThumbStyle));
        }

        GUILayout.Space(10f);
        GUILayout.Label("Difficulty Presets", sectionStyle);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Easy", buttonStyle, controlHeightOption)) ApplyPreset(DifficultyPreset.Easy);
        if (GUILayout.Button("Medium", buttonStyle, controlHeightOption)) ApplyPreset(DifficultyPreset.Medium);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Hard", buttonStyle, controlHeightOption)) ApplyPreset(DifficultyPreset.Hard);
        if (GUILayout.Button("Extreme", buttonStyle, controlHeightOption)) ApplyPreset(DifficultyPreset.Extreme);
        GUILayout.EndHorizontal();

        GUILayout.Space(12f);
        if (GUILayout.Button("Apply", buttonStyle, controlHeightOption))
        {
            if (gridMap != null)
            {
                PushToGridMap();
                if (regenerateImmediatelyOnApply)
                {
                    gridMap.GenerateLandMap();
                }
            }
            else
            {
                pendingApplyWithoutGridMap = true;
            }
        }

        if (GUILayout.Button("Apply + New Seed", buttonStyle, controlHeightOption))
        {
            seedText = Random.Range(int.MinValue, int.MaxValue).ToString();

            if (gridMap != null)
            {
                PushToGridMap();
                gridMap.GenerateLandMap();
            }
            else
            {
                pendingApplyWithoutGridMap = true;
            }
        }

        if (GUILayout.Button("Close", buttonStyle, controlHeightOption))
        {
            SetMenuOpenState(false);
        }

        GUILayout.Space(8f);
        GUILayout.Label("Toggle via action/F2 or bottom-left button", labelStyle);

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
    }

    private GUIStyle CreateButtonStyle(int fontSize)
    {
        var style = new GUIStyle(GUI.skin.button)
        {
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            border = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(4, 4, 4, 4)
        };

        style.normal.background = buttonTexture;
        style.onNormal.background = buttonTexture;
        style.hover.background = buttonHoverTexture;
        style.onHover.background = buttonHoverTexture;
        style.active.background = buttonActiveTexture;
        style.onActive.background = buttonActiveTexture;
        style.focused.background = buttonHoverTexture;
        style.onFocused.background = buttonHoverTexture;
        style.normal.textColor = menuTextColor;
        style.onNormal.textColor = menuTextColor;
        style.hover.textColor = menuTextColor;
        style.onHover.textColor = menuTextColor;
        style.active.textColor = menuTextColor;
        style.onActive.textColor = menuTextColor;
        style.focused.textColor = menuTextColor;
        style.onFocused.textColor = menuTextColor;
        return style;
    }

    private GUIStyle CreateFloatingButtonStyle(int fontSize)
    {
        var style = new GUIStyle(GUI.skin.button)
        {
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            border = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(10, 10, 6, 6)
        };

        style.normal.background = floatingButtonTexture;
        style.onNormal.background = floatingButtonTexture;
        style.hover.background = floatingButtonHoverTexture;
        style.onHover.background = floatingButtonHoverTexture;
        style.active.background = floatingButtonActiveTexture;
        style.onActive.background = floatingButtonActiveTexture;
        style.focused.background = floatingButtonHoverTexture;
        style.onFocused.background = floatingButtonHoverTexture;

        style.normal.textColor = floatingButtonTextColor;
        style.onNormal.textColor = floatingButtonTextColor;
        style.hover.textColor = floatingButtonTextColor;
        style.onHover.textColor = floatingButtonTextColor;
        style.active.textColor = floatingButtonTextColor;
        style.onActive.textColor = floatingButtonTextColor;
        style.focused.textColor = floatingButtonTextColor;
        style.onFocused.textColor = floatingButtonTextColor;
        return style;
    }

    private void EnsureThemeTextures()
    {
        if (windowTexture != null)
        {
            return;
        }

        windowTexture = CreateSolidTexture(menuWindowColor);
        buttonTexture = CreateSolidTexture(menuButtonColor);
        buttonHoverTexture = CreateSolidTexture(menuButtonHoverColor);
        buttonActiveTexture = CreateSolidTexture(menuButtonActiveColor);
        inputFieldTexture = CreateFramedTexture(menuInputFieldColor, menuInputFieldBorderColor);
        sliderTrackTexture = CreateSolidTexture(menuSliderTrackColor);
        sliderThumbTexture = CreateSolidTexture(menuSliderThumbColor);
        outlineTexture = CreateSolidTexture(menuOutlineColor);
        floatingButtonTexture = CreateSolidTexture(floatingButtonBaseColor);
        floatingButtonHoverTexture = CreateSolidTexture(floatingButtonHoverColor);
        floatingButtonActiveTexture = CreateSolidTexture(floatingButtonActiveColor);
        floatingButtonAccentTexture = CreateSolidTexture(floatingButtonAccentColor);
        floatingButtonShadowTexture = CreateSolidTexture(floatingButtonShadowColor);
    }

    private static Texture2D CreateFramedTexture(Color fillColor, Color borderColor)
    {
        const int size = 8;
        const int border = 1;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Point,
            hideFlags = HideFlags.HideAndDontSave
        };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isBorder = x < border || y < border || x >= size - border || y >= size - border;
                texture.SetPixel(x, y, isBorder ? borderColor : fillColor);
            }
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateSolidTexture(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void DestroyThemeTextures()
    {
        DestroyTexture(ref windowTexture);
        DestroyTexture(ref buttonTexture);
        DestroyTexture(ref buttonHoverTexture);
        DestroyTexture(ref buttonActiveTexture);
        DestroyTexture(ref inputFieldTexture);
        DestroyTexture(ref sliderTrackTexture);
        DestroyTexture(ref sliderThumbTexture);
        DestroyTexture(ref outlineTexture);
        DestroyTexture(ref floatingButtonTexture);
        DestroyTexture(ref floatingButtonHoverTexture);
        DestroyTexture(ref floatingButtonActiveTexture);
        DestroyTexture(ref floatingButtonAccentTexture);
        DestroyTexture(ref floatingButtonShadowTexture);
    }

    private static void DestroyTexture(ref Texture2D texture)
    {
        if (texture == null)
        {
            return;
        }

        Destroy(texture);
        texture = null;
    }

    private void PullFromGridMap()
    {
        if (gridMap == null)
        {
            return;
        }

        obstaclePercent = gridMap.ObstaclePercent;
        limitMineSourceTiles = gridMap.LimitMineSourceTiles;
        maxMineSourcePercent = gridMap.MaxMineSourcePercent;
        minMineSourceTiles = gridMap.MinMineSourceTiles;
        guaranteeStarterResources = gridMap.GuaranteeStarterResourceNodes;
        starterMinDistance = gridMap.StarterResourceMinDistanceFromCity;
        starterRadius = gridMap.StarterResourceRadius;
        preserveNearestMine = gridMap.PreserveNearestMineSourceToCity;
        seedText = gridMap.seed.ToString();
    }

    private void PushToGridMap()
    {
        if (gridMap == null)
        {
            return;
        }

        gridMap.ObstaclePercent = obstaclePercent;
        gridMap.LimitMineSourceTiles = limitMineSourceTiles;
        gridMap.MaxMineSourcePercent = maxMineSourcePercent;
        gridMap.MinMineSourceTiles = minMineSourceTiles;
        gridMap.GuaranteeStarterResourceNodes = guaranteeStarterResources;
        gridMap.StarterResourceMinDistanceFromCity = starterMinDistance;
        gridMap.StarterResourceRadius = starterRadius;
        gridMap.PreserveNearestMineSourceToCity = preserveNearestMine;

        if (TryParseSeed(seedText, out int parsedSeed))
        {
            gridMap.seed = parsedSeed;
            seedText = parsedSeed.ToString();
        }
        else
        {
            gridMap.seed = 0;
            seedText = "0";
        }
    }

    private static string SanitizeSeedInput(string rawInput)
    {
        if (string.IsNullOrEmpty(rawInput))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(rawInput.Length);
        bool canAddSign = true;
        for (int i = 0; i < rawInput.Length; i++)
        {
            char c = rawInput[i];
            if (c == '-' && canAddSign && builder.Length == 0)
            {
                builder.Append(c);
                canAddSign = false;
                continue;
            }

            if (char.IsDigit(c))
            {
                builder.Append(c);
                canAddSign = false;
            }
        }

        return builder.ToString();
    }

    private static bool TryParseSeed(string input, out int parsedSeed)
    {
        parsedSeed = 0;
        if (string.IsNullOrWhiteSpace(input) || input == "-")
        {
            return false;
        }

        if (!long.TryParse(input, out long longSeed))
        {
            return false;
        }

        if (longSeed > int.MaxValue)
        {
            parsedSeed = int.MaxValue;
            return true;
        }

        if (longSeed < int.MinValue)
        {
            parsedSeed = int.MinValue;
            return true;
        }

        parsedSeed = (int)longSeed;
        return true;
    }

    private static void DrawOutline(Rect rect, float thickness, Texture2D texture)
    {
        if (texture == null)
        {
            return;
        }

        float t = Mathf.Max(1f, thickness);
        GUI.DrawTexture(new Rect(rect.x - t, rect.y - t, rect.width + (2f * t), t), texture);
        GUI.DrawTexture(new Rect(rect.x - t, rect.yMax, rect.width + (2f * t), t), texture);
        GUI.DrawTexture(new Rect(rect.x - t, rect.y, t, rect.height), texture);
        GUI.DrawTexture(new Rect(rect.xMax, rect.y, t, rect.height), texture);
    }

    private void ApplyPreset(DifficultyPreset preset)
    {
        switch (preset)
        {
            case DifficultyPreset.Easy:
                obstaclePercent = 0.22f;
                limitMineSourceTiles = true;
                maxMineSourcePercent = 0.07f;
                minMineSourceTiles = 8;
                guaranteeStarterResources = true;
                starterMinDistance = 1;
                starterRadius = 4;
                preserveNearestMine = true;
                break;

            case DifficultyPreset.Medium:
                obstaclePercent = 0.30f;
                limitMineSourceTiles = true;
                maxMineSourcePercent = 0.05f;
                minMineSourceTiles = 6;
                guaranteeStarterResources = true;
                starterMinDistance = 2;
                starterRadius = 4;
                preserveNearestMine = true;
                break;

            case DifficultyPreset.Hard:
                obstaclePercent = 0.38f;
                limitMineSourceTiles = true;
                maxMineSourcePercent = 0.035f;
                minMineSourceTiles = 4;
                guaranteeStarterResources = true;
                starterMinDistance = 2;
                starterRadius = 3;
                preserveNearestMine = true;
                break;

            case DifficultyPreset.Extreme:
                obstaclePercent = 0.48f;
                limitMineSourceTiles = true;
                maxMineSourcePercent = 0.02f;
                minMineSourceTiles = 2;
                guaranteeStarterResources = true;
                starterMinDistance = 3;
                starterRadius = 3;
                preserveNearestMine = true;
                break;
        }
    }
}
