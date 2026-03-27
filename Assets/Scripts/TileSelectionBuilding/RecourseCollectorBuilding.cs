using GridGeneration;
using UnityEngine;

public class RecourseCollectorBuilding : MonoBehaviour
{
    public enum BuildingType
    {
        Sawmill,
        Mine1,
        Mine2
    }

    [SerializeField] private GridMap gridMap;
    [SerializeField] private BuildingType selectedBuilding;

    public bool CanPlaceSelectedBuilding(Vector2Int coordinate)
    {
        return CanPlaceBuilding(selectedBuilding, coordinate);
    }

    public bool CanPlaceBuilding(BuildingType buildingType, Vector2Int coordinate)
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
            case BuildingType.Sawmill:
                return tile.tileType == TileType.Forest;

            case BuildingType.Mine1:
                return tile.tileType == TileType.Valley;

            case BuildingType.Mine2:
                return tile.tileType == TileType.Mountain;

            default:
                return false;
        }
    }
}
