using GridGeneration;
using UnityEngine;
using UnityEngine.InputSystem;

public class RecourseCollectorBuilding : MonoBehaviour
{
    [SerializeField] private GridMap gridMap;
    [SerializeField] private Building selectedBuilding = Building.Sawmill;

    private Keyboard keyboard;
    private bool mineModeSelected;

    private void OnEnable()
    {
        keyboard = Keyboard.current;
    }

    private void Update()
    {
        HandleBuildingKeybinds();
    }

    private void HandleBuildingKeybinds()
    {
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.oKey.wasPressedThisFrame)
        {
            ToggleMineMode();
        }

        if (mineModeSelected)
        {
            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                SelectBuilding(Building.ValleyMine);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                SelectBuilding(Building.MountainMine);
            }
        }
        else
        {
            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                SelectBuilding(Building.Sawmill);
            }
        }
    }

    private void ToggleMineMode()
    {
        mineModeSelected = !mineModeSelected;

        if (mineModeSelected)
        {
            SelectBuilding(Building.ValleyMine);
        }
        else
        {
            SelectBuilding(Building.Sawmill);
        }
    }

    public bool CanPlaceSelectedBuilding(Vector2Int coordinate)
    {
        return CanPlaceBuilding(selectedBuilding, coordinate);
    }

    public bool CanPlaceBuilding(Building buildingType, Vector2Int coordinate)
    {
        if (gridMap == null || !gridMap.IsInsideGrid(coordinate))
        {
            return false;
        }

        GridTile tile = gridMap.GetTileAt(coordinate.x, coordinate.y);
        if (tile == null)
        {
            return false;
        }

        switch (buildingType)
        {
            case Building.Sawmill:
                return HasExactTileName(tile, "Forest");

            case Building.ValleyMine:
                return HasExactTileName(tile, "Valley");

            case Building.MountainMine:
                return HasExactTileName(tile, "Obstacle1");

            default:
                return false;
        }
    }

    private static bool HasExactTileName(GridTile tile, string expectedName)
    {
        return tile != null
            && !string.IsNullOrWhiteSpace(expectedName)
            && string.Equals(tile.tileName, expectedName, System.StringComparison.OrdinalIgnoreCase);
    }

    private void SelectBuilding(Building buildingType)
    {
        selectedBuilding = buildingType;
    }
}
