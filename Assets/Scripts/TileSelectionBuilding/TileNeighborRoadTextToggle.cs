using GridGeneration;
using UnityEngine;

namespace TileSelectionBuilding
{
    public class TileNeighborRoadTextToggle : MonoBehaviour
    {
        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        [Header("Text References")]
        [SerializeField] private GameObject woodText;
        [SerializeField] private GameObject stoneText;

        [Header("Road Detection References (Auto Resolved)")]
        private GridMap gridMap;
        private TileBuilding tileBuilding;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeToRoadUpdates();
            RefreshTextVisibility();
        }

        private void OnDisable()
        {
            UnsubscribeFromRoadUpdates();
        }

        private void SubscribeToRoadUpdates()
        {
            if (tileBuilding != null)
            {
                tileBuilding.RoadPlaced += RefreshTextVisibility;
            }
        }

        private void UnsubscribeFromRoadUpdates()
        {
            if (tileBuilding != null)
            {
                tileBuilding.RoadPlaced -= RefreshTextVisibility;
            }
        }

        private void ResolveReferences()
        {
            // Scene has one GridMap and one TileBuilding, so resolve them automatically.
            if (gridMap == null)
            {
                gridMap = FindAnyObjectByType<GridMap>();
            }

            if (tileBuilding == null)
            {
                tileBuilding = FindAnyObjectByType<TileBuilding>();
            }
        }

        private void RefreshTextVisibility()
        {
            ResolveReferences();

            if (gridMap == null || tileBuilding == null)
            {
                SetTextsActive(false);
                return;
            }

            if (!gridMap.TryWorldToGridCoordinate(transform.position, out Vector2Int coordinate))
            {
                SetTextsActive(false);
                return;
            }

            bool hasNeighborRoad = HasNeighborRoad(coordinate);
            SetTextsActive(hasNeighborRoad);
        }

        private bool HasNeighborRoad(Vector2Int coordinate)
        {
            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                Vector2Int neighbor = coordinate + CardinalDirections[i];
                if (!gridMap.IsInsideGrid(neighbor))
                {
                    continue;
                }

                if (tileBuilding.IsRoadAlreadyBuilt(neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetTextsActive(bool isActive)
        {
            if (woodText != null)
            {
                woodText.SetActive(isActive);
            }

            if (stoneText != null)
            {
                stoneText.SetActive(isActive);
            }
        }
    }
}
