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

        return TryBreakTilemapAtWorldPosition(tilemap, worldPosition, searchRadius, breakRadius);
    }

    public static bool TryBreakTilemapAtWorldPosition(Tilemap targetTilemap, Vector3 worldPosition, int searchRadius, int breakRadius)
    {
        if (targetTilemap == null)
            return false;

        int safeSearchRadius = Mathf.Max(0, searchRadius);
        int safeBreakRadius = Mathf.Max(0, breakRadius);
        Vector3Int centerCell = targetTilemap.WorldToCell(worldPosition);

        if (!TryFindBreakableCell(targetTilemap, centerCell, safeSearchRadius, out Vector3Int targetCell))
            return false;

        BreakCellsAround(targetTilemap, targetCell, safeBreakRadius);
        return true;
    }

    private static bool TryFindBreakableCell(Tilemap targetTilemap, Vector3Int centerCell, int searchRadius, out Vector3Int targetCell)
    {
        if (targetTilemap.HasTile(centerCell))
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
                if (!targetTilemap.HasTile(cell))
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

    private static void BreakCellsAround(Tilemap targetTilemap, Vector3Int centerCell, int breakRadius)
    {
        for (int x = -breakRadius; x <= breakRadius; x++)
        {
            for (int y = -breakRadius; y <= breakRadius; y++)
            {
                Vector3Int cell = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z);
                if (!targetTilemap.HasTile(cell))
                    continue;

                targetTilemap.SetTile(cell, null);
                targetTilemap.RefreshTile(cell);
            }
        }
    }
}
