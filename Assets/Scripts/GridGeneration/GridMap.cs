using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GridGeneration
{
    public class GridMap : MonoBehaviour
    {
        [SerializeField] private int width, height;
        [SerializeField] private float spacing, tileSize;
        [SerializeField] private GridTile[] tiles;
        [SerializeField] private int seed;
        private GridTile[,] tileMap;

        // Cache the inspected values so OnValidate regenerates only on relevant changes.
        private int _lastConfigHash;

#if UNITY_EDITOR
        private bool _isEditorRegenQueued;
#endif

        private void Start()
        {
            GenerateMap();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            int configHash = GetConfigHash();
            if (configHash == _lastConfigHash)
            {
                return;
            }

            _lastConfigHash = configHash;

#if UNITY_EDITOR
            QueueEditorRegeneration();
#endif
        }

#if UNITY_EDITOR
        private void QueueEditorRegeneration()
        {
            if (_isEditorRegenQueued)
            {
                return;
            }

            _isEditorRegenQueued = true;
            EditorApplication.delayCall += RegenerateInEditor;
        }

        private void RegenerateInEditor()
        {
            EditorApplication.delayCall -= RegenerateInEditor;
            _isEditorRegenQueued = false;

            if (this == null || Application.isPlaying)
            {
                return;
            }

            GenerateMap();
        }
#endif

        private void GenerateMap()
        {
            if (tiles == null || tiles.Length == 0 || tiles[0] == null)
            {
                Debug.LogError("GridMap: Missing base tile in the tiles array.", this);
                return;
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] == null)
                {
                    Debug.LogError($"GridMap: Tile at index {i} is not assigned.", this);
                    return;
                }

                if (tiles[i].tilePrefab == null)
                {
                    Debug.LogError($"GridMap: Tile at index {i} does not have a prefab assigned.", this);
                    return;
                }
            }

            if (width <= 0 || height <= 0)
            {
                Debug.LogError("GridMap: Width and height must be greater than 0.", this);
                return;
            }

            if (tileSize <= 0)
            {
                Debug.LogError("GridMap: Tile size must be greater than 0.", this);
                return;
            }

            ClearGeneratedTiles();

            tileMap = new GridTile[width, height];

            float step = tileSize + spacing;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var noiseValue = Mathf.PerlinNoise((x + seed) * 0.1f, (y + seed) * 0.1f);
                    if(noiseValue > 0.5f)
                        tileMap[x, y] = tiles[0];
                    else
                    {
                        tileMap[x, y] = tiles[1];
                    }
                    Vector3 localPos = new Vector3(x * step, 0f, y * step);
                    GameObject tileInstance = Instantiate(tileMap[x, y].tilePrefab, transform);
                    tileInstance.transform.localPosition = localPos;
                    tileInstance.name = $"Tile_{x}_{y}";
                }
            }
        }

        private int GetConfigHash()
        {
            int baseTileId = 0;
            int basePrefabId = 0;

            if (tiles != null && tiles.Length > 0 && tiles[0] != null)
            {
                baseTileId = tiles[0].GetEntityId().GetHashCode();
                if (tiles[0].tilePrefab != null)
                {
                    basePrefabId = tiles[0].tilePrefab.GetEntityId().GetHashCode();
                }
            }

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + width;
                hash = hash * 31 + height;
                hash = hash * 31 + spacing.GetHashCode();
                hash = hash * 31 + tileSize.GetHashCode();
                hash = hash * 31 + seed;
                hash = hash * 31 + baseTileId;
                hash = hash * 31 + basePrefabId;
                return hash;
            }
        }

        private void ClearGeneratedTiles()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;

                if (Application.isPlaying)
                {
                    Destroy(child);
                }
#if UNITY_EDITOR
                else
                {
                    DestroyImmediate(child);
                }
#endif
            }
        }
    }
}
