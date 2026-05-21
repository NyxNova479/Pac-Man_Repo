using UnityEngine;
using FixedEngine;
using UnityEngine.Tilemaps;


public class PinkyController : GhostController
{

    protected override Vector3Int CalculateTargetCell(Vector3Int currentCell)
    {
        Vector3Int pacCell = collisionMap.WorldToCell(pacMan.transform.position);

        var dir = pacMan.m_currentDir;
        int tileDevant = 4;

        int dx = dir.x.Raw > 0 ? 1 : dir.x.Raw < 0 ? -1 : 0;
        int dy = dir.y.Raw > 0 ? 1 : dir.y.Raw < 0 ? -1 : 0;

        Vector3Int targetCell = pacCell + new Vector3Int(tileDevant * dx, tileDevant * dy, 0);

        var bounds = collisionMap.cellBounds;
        int largeur = bounds.size.x;
        int hauteur = bounds.size.y;
        int x0 = bounds.xMin;
        int y0 = bounds.yMin;

        int targetX = (targetCell.x - x0) % largeur;
        if (targetX < 0) targetX += largeur;
        targetX += x0;

        int targetY = (targetCell.y - y0) % hauteur;
        if (targetY < 0) targetY += hauteur;
        targetY += y0;

        return new Vector3Int(targetX, targetY, 0);
    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        Vector3 worldCenter = collisionMap.GetCellCenterWorld(m_targetCell);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(worldCenter, collisionMap.cellSize);
    }

#endif

}
