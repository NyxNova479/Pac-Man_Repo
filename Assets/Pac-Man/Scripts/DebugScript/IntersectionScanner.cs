using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
[ExecuteAlways]
public class IntersectionScanner : MonoBehaviour
{
    [SerializeField] private Tilemap collisionMap;
    [SerializeField] private Color debugColorAll = Color.green;
    [SerializeField] private float debugSphereSize = 1f;
    [SerializeField] private Color gizmoColorInter = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private Vector2 gizmoSize = new Vector2(8f, 8f);
    [SerializeField] private Color textColor = Color.white;

    private void OnDrawGizmos()
    {
        if (collisionMap == null) return;

        var bounds = collisionMap.cellBounds;
        // Affiche les bounds pour vérifier
        Debug.Log($"[Scanner] Bounds X[{bounds.xMin},{bounds.xMax}] Y[{bounds.yMin},{bounds.yMax}]");

        foreach (var cell in bounds.allPositionsWithin)
        {
            var tile = collisionMap.GetTile(cell) as WalkableTile;
            if (tile == null) continue;

            // 1) Dessine un point vert sur TOUTES les cases Path
            if (tile.tileType == TileType.Path)
            {
                Vector3 center = collisionMap.GetCellCenterWorld(cell);
                Gizmos.color = debugColorAll;
                Gizmos.DrawSphere(center, debugSphereSize);
                Debug.Log($"  ▶ Path at {cell}");
            }
            else
            {
                // on skip les autres
                continue;
            }

            // 2) Compte les voisins Path/​Warp
            int count = 0;
            foreach (var dir in new[] { Vector3Int.up, Vector3Int.down,
                                         Vector3Int.left, Vector3Int.right })
            {
                var n = collisionMap.GetTile(cell + dir) as WalkableTile;
                if (n != null &&
                   (n.tileType == TileType.Path || n.tileType == TileType.Warp))
                    count++;
            }

            // 3) Si intersection (3+ voisins), dessine un cube rouge et label
            if (count >= 3)
            {
                Vector3 center = collisionMap.GetCellCenterWorld(cell);
                Gizmos.color = gizmoColorInter;
                Gizmos.DrawCube(center, new Vector3(gizmoSize.x, gizmoSize.y, 0f));

                UnityEditor.Handles.color = textColor;
                UnityEditor.Handles.Label(
                    center + new Vector3(-gizmoSize.x * 0.5f + 1f,
                                         -gizmoSize.y * 0.5f + 1f,
                                         0),
                    $"({cell.x},{cell.y}) nb={count}");
                Debug.Log($"    └─ Intersection at {cell}, count={count}");
            }
        }
    }
}
#endif
