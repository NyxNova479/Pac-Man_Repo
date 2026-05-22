using UnityEngine;
using FixedEngine;
using UnityEngine.Tilemaps;


public class ClydeController : GhostController
{

    protected override Vector3Int CalculateTargetCell(Vector3Int currentCell)
    {
        Vector3Int pacCell = collisionMap.WorldToCell(pacMan.transform.position);
        Vector3Int targetCell;

        var dir = pacMan.m_currentDir;
        int tileDevant = 8;
        Vector3Int perimeterX = pacCell + new Vector3Int(pacCell.x + tileDevant,0, 0);
        Vector3Int perimeterY = pacCell + new Vector3Int(0, pacCell.y + tileDevant, 0);

        if(currentCell.x-perimeterX.x > 0)
        {
            targetCell = collisionMap.WorldToCell(pacMan.transform.position);
        }

        int dx = dir.x.Raw > 0 ? 1 : dir.x.Raw < 0 ? -1 : 0;
        int dy = dir.y.Raw > 0 ? 1 : dir.y.Raw < 0 ? -1 : 0;

        targetCell = pacCell + new Vector3Int(tileDevant * dx, tileDevant * dy, 0);

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


}
