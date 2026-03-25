using GridGeneration;
using UnityEngine;

[CreateAssetMenu(fileName = "GridTile", menuName = "Scriptable Objects/GridTile")]
public class GridTile : ScriptableObject
{
    public string tileName;
    public TileType tileType;
    public GameObject tilePrefab;
}
