using UnityEngine;
using FixedEngine;
using UnityEngine.Tilemaps;


public class BlinkyController : GhostController
{

    protected override Vector3Int CalculateTargetCell(Vector3Int currentCell)
    {
        return collisionMap.WorldToCell(pacMan.transform.position);
    }


}
