using UnityEngine;
using FixedEngine;
using UnityEngine.Tilemaps;


public class InkyController : GhostController
{

    [SerializeField] BlinkyController blinky;

    protected override Vector3Int CalculateTargetCell(Vector3Int currentCell)
    {
        return collisionMap.WorldToCell(pacMan.transform.position);
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
