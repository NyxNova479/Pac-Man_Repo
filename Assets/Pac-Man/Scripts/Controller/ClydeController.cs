using UnityEngine;
using FixedEngine;
using UnityEngine.Tilemaps;


public class ClydeController : GhostController
{

    [SerializeField] private int tileAutour = 8;
    protected override Vector3Int CalculateTargetCell(Vector3Int currentCell)
    {
        Vector3Int pacCell = collisionMap.WorldToCell(pacMan.transform.position);
        Vector3Int targetCell;

        int dx = pacCell.x - currentCell.x;
        int dy = pacCell.y - currentCell.y;
        int dist2 = dx * dx + dy * dy;

        int threshold2 = tileAutour * tileAutour;



        if (dist2 > threshold2)
        {

            targetCell = pacCell;
        }
        else
        {

            targetCell = collisionMap.origin; //pacCell + new Vector3Int(tileDevant * dx, tileDevant * dy, 0);
        }


        
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(worldCenter, collisionMap.cellSize);
        Vector3Int pacCell = collisionMap.WorldToCell(pacMan.transform.position);

        if (pacMan != null)
        {

            Vector3 pacCenter = collisionMap.GetCellCenterWorld(pacCell);

            float tileWidth = collisionMap.cellSize.x;
            float radius = tileAutour * tileWidth;


            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pacCenter, radius);



        }
    }

    

#endif

}
