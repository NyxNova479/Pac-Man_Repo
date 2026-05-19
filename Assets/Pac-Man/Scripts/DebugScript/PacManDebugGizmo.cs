using UnityEngine;
using UnityEngine.Tilemaps;
using FixedEngine;

#if UNITY_EDITOR
[ExecuteAlways]
public class PacManBoxGizmo : MonoBehaviour
{
    public Color tileColor = new Color(1f, 1f, 0f, 0.3f); // jaune transparent
    public Color textColor = Color.white;

    [SerializeField] private PacManController pacman;

    private void OnDrawGizmos()
    {
        //    public Tilemap collisionMap_debug => collisionMap;
        if (pacman == null || pacman.CollisionMapDebug == null) return;

        Tilemap map = pacman.CollisionMapDebug;
        BoundsInt bounds = map.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase tile = map.GetTile(pos);
            if (tile is WalkableTile walkable)
            {
                // Centre de la cellule en coordonnées monde
                Vector3 worldCenter = map.GetCellCenterWorld(pos);

                // Dessin de Gizmo
                Gizmos.color = tileColor;
                Gizmos.DrawCube(worldCenter, new Vector3(8f, 8f, 0f));

#if UNITY_EDITOR
                UnityEditor.Handles.color = textColor;
                UnityEditor.Handles.Label(worldCenter + new Vector3(-3.5f, -2.5f, 0),
                    $"({worldCenter.x:0},{worldCenter.y:0})");
#endif
            }
        }
    }
}
#endif
