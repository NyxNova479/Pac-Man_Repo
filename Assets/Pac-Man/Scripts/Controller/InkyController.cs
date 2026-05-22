using UnityEngine;
using FixedEngine;
using UnityEngine.Tilemaps;


public class InkyController : GhostController
{

    protected override Vector3Int CalculateTargetCell(Vector3Int currentCell)
    {
        return collisionMap.WorldToCell(pacMan.transform.position);
    }


}
