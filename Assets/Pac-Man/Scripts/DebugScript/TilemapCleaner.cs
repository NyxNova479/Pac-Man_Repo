using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TilemapCleaner : MonoBehaviour
{
    public Tilemap source;
    public Tilemap target;

    [ContextMenu("Copier vers target")]
    public void CopyCleanTilemap()
    {
        if (source == null || target == null)
        {
            Debug.LogError("[TilemapCleaner] Assigne une source et une cible !");
            return;
        }

        var bounds = source.cellBounds;
        int copied = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                var tile = source.GetTile(cell);
                if (tile == null) continue;

                // 1. Copier la tuile
                target.SetTile(cell, tile);

                // 2. Copier la matrice de transformation
                var matrix = source.GetTransformMatrix(cell);
                target.SetTransformMatrix(cell, matrix);

                copied++;
            }
        }

        Debug.Log($"[TilemapCleaner] {copied} tuiles recopiées avec transformation.");
    }
}
