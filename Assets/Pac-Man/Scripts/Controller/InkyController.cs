using UnityEngine;
using FixedEngine;
using UnityEngine.Tilemaps;


public class InkyController : GhostController
{

    [SerializeField] BlinkyController blinky;

    protected override Vector3Int CalculateTargetCell(Vector3Int currentCell)
    {

        Vector3Int pacCell = collisionMap.WorldToCell(pacMan.transform.position);
        Vector3Int blinkyCell = collisionMap.WorldToCell(blinky.transform.position);
        int tileDevant = 2;

        var dir = pacMan.m_currentDir;

        int dx = dir.x.Raw > 0 ? 1 : dir.x.Raw < 0 ? -1 : 0;
        int dy = dir.y.Raw > 0 ? 1 : dir.y.Raw < 0 ? -1 : 0;


        Vector3Int devPacMan = pacCell + new Vector3Int(dx * tileDevant, dy * tileDevant, 0);


        Vector3Int vec = new Vector3Int(devPacMan.x - blinkyCell.x, devPacMan.y - blinkyCell.y, 0);


        Vector3Int targetCell = blinkyCell + new Vector3Int(vec.x * 2, vec.y * 2, 0);



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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(worldCenter, collisionMap.cellSize);


    }

#endif
}
