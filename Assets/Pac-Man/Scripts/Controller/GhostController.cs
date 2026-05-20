using UnityEngine;
using FixedEngine;
using UnityEngine.Tilemaps;

using Fixed = FixedEngine.Q8_8;

public abstract class GhostController : MonoBehaviour,IFixedTick
{
    [SerializeField] protected Tilemap collisionMap;
    [SerializeField] protected int speedRaw = 256;
    [SerializeField] protected int tileSize = 8;
    [SerializeField] protected LogicGrid grid;
    [SerializeField] protected PacManController pacManController;

    protected FixedPoint<Fixed> m_speed;
    protected FixedTransform2D<Fixed> m_transform;
    protected FixedMover2D<Fixed> m_mover;

    protected FixedVector2<Fixed> m_desiredDir = FixedVector2<Fixed>.Zero;
    protected FixedVector2<Fixed> m_currentDir = FixedVector2<Fixed>.Zero;
    protected FixedVector2<Fixed> m_incomingDir = FixedVector2<Fixed>.Zero;
    protected Vector3Int m_targetCell;

    protected abstract Vector3Int CalculateTargetCell(Vector3Int currentCell);

 

    void Awake()
    {
        m_speed = new FixedPoint<Fixed>(speedRaw);
        m_transform = new FixedTransform2D<Fixed>();
        m_mover = new FixedMover2D<Fixed>() { Transform = m_transform };

        Vector3Int startCell = collisionMap.WorldToCell(transform.position);

        m_mover.SnapToCellFromUnity(transform.position, tileSize, grid.origin);
        m_targetCell = CalculateTargetCell(startCell);

        TickManager.Instance.Register(this);

    }

    private void OnDestroy()
    {
        if (TickManager.Instance != null)
        {
            TickManager.Instance.Unregister(this);
        }
    }

    public  void FixedTick()
    {

        // --- BLOC N°01 : position cellule courantes ---

        FixedVector2<Fixed> pos = m_mover.Position2D;
        var worldPos = new Vector3(pos.x.ToFloat(), pos.y.ToFloat(), transform.position.z);
        Vector3Int cell = collisionMap.WorldToCell(worldPos);
        FixedVector2<Fixed> center = GetFixedCenter(cell);

        // BLOC N°02 Demarage
        if (m_currentDir == FixedVector2<Fixed>.Zero && pacManController.m_currentDir != FixedVector2<Fixed>.Zero && CanMove(cell, pacManController.m_currentDir))
        {
            m_currentDir = pacManController.m_currentDir;
        }


        // --- BLOC N°03 : Deplacement normal ---

        if (m_currentDir != FixedVector2<Fixed>.Zero)
        {
            if (CanMove(cell, m_currentDir))
            {
                // eviter le tunneling (clipping)
                SafeMove(m_currentDir);
            }
            else
            {
                var deltaX = center.x - pos.x;
                var deltaY = center.y - pos.y;

                var moveX = FixedMath.Abs(deltaX).Raw <= m_speed.Raw ? deltaX : FixedMath.Sign(deltaX) * m_speed;

                var moveY = FixedMath.Abs(deltaY).Raw <= m_speed.Raw ? deltaY : FixedMath.Sign(deltaY) * m_speed;

                m_mover.Move(new FixedVector2<Fixed>(moveX, moveY));

                if (deltaX.Raw == moveX.Raw && deltaY.Raw == moveY.Raw)
                {
                    m_currentDir = FixedVector2<Fixed>.Zero;
                }
            }
        }
    } // fin fixed tick


    protected bool CanMove(Vector3Int cell, FixedVector2<Fixed> dir)
    {
        int dx = dir.x.Raw > 0 ? 1 : dir.x.Raw < 0 ? -1 : 0;
        int dy = dir.y.Raw > 0 ? 1 : dir.y.Raw < 0 ? -1 : 0;

        var nextCell = new Vector3Int(cell.x + dx, cell.y + dy, 0);
        var logic = grid.GetCell(nextCell);
        return (logic != null && logic.isWalkable) || logic == null;
    }

    protected void SafeMove(FixedVector2<Fixed> direction)
    {
        const int subSteps = 4;

        var step = m_speed / FixedPoint<Fixed>.FromInt(subSteps);
        var start = m_mover.Position2D;

        for (int i = 1; i < subSteps; i++)
        {
            var offSet = direction * (step * FixedPoint<Fixed>.FromInt(i));
            var nextPos = start + offSet;

            Vector3 worldPos = new Vector3(nextPos.x.ToFloat(), nextPos.y.ToFloat(), 0f);

            Vector3Int cell = collisionMap.WorldToCell(worldPos);

            var logic = grid.GetCell(cell);
            bool walkable = logic == null || logic.isWalkable;
        }

        m_mover.Move(direction * m_speed);
    }

    void LateUpdate()
    {
        m_transform.ApplyToTransform2D(transform);
    }

    protected FixedVector2<Fixed> GetFixedCenter(Vector3Int cell)
    {
        Vector3 worldCenter = collisionMap.GetCellCenterLocal(cell);
        return new FixedVector2<Fixed>(worldCenter.x, worldCenter.y);
    }
}
