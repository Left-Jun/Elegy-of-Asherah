using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableTilemap : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private int searchRadius = 1;
    [SerializeField] private int breakRadius = 0;

    private void Awake()
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();
    }

    private void OnValidate()
    {
        searchRadius = Mathf.Max(0, searchRadius);
        breakRadius = Mathf.Max(0, breakRadius);
    }

    public bool TryBreakAtWorldPosition(Vector3 worldPosition)
    {
        if (tilemap == null)
        {
            Debug.LogWarning("BreakableTilemap: Tilemap component is missing.", this);
            return false;
        }

        Vector3Int centerCell = tilemap.WorldToCell(worldPosition);
        if (!TryFindBreakableCell(centerCell, out Vector3Int targetCell))
            return false;

        BreakCellsAround(targetCell);
        return true;
    }

    private bool TryFindBreakableCell(Vector3Int centerCell, out Vector3Int targetCell)
    {
        if (tilemap.HasTile(centerCell))
        {
            targetCell = centerCell;
            return true;
        }

        float bestDistance = float.MaxValue;
        targetCell = centerCell;
        bool foundTile = false;

        for (int x = -searchRadius; x <= searchRadius; x++)
        {
            for (int y = -searchRadius; y <= searchRadius; y++)
            {
                Vector3Int cell = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z);
                if (!tilemap.HasTile(cell))
                    continue;

                float distance = (cell - centerCell).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    targetCell = cell;
                    foundTile = true;
                }
            }
        }

        return foundTile;
    }

    private void BreakCellsAround(Vector3Int centerCell)
    {
        for (int x = -breakRadius; x <= breakRadius; x++)
        {
            for (int y = -breakRadius; y <= breakRadius; y++)
            {
                Vector3Int cell = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z);
                if (!tilemap.HasTile(cell))
                    continue;

                tilemap.SetTile(cell, null);
                tilemap.RefreshTile(cell);
            }
        }
    }
}
