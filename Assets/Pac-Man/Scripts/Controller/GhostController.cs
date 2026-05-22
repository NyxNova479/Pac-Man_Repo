using FixedEngine;
using PacMan.Input;
using UnityEngine;
using UnityEngine.Tilemaps;

using Fixed = FixedEngine.Q8_8;

public abstract class GhostController : MonoBehaviour, IFixedTick
{

    [SerializeField] protected Tilemap collisionMap;
    [SerializeField] private int speedRaw = 256;
    [SerializeField] protected int tileSize = 8;
    [SerializeField] protected LogicGrid grid;
    [SerializeField] protected PacManController pacMan;

    private FixedPoint<Fixed> m_speed;
    private FixedPoint<Fixed> m_startSpeed;
    private FixedTransform2D<Fixed> m_transform;
    private FixedMover2D<Fixed> m_mover;

    private FixedVector2<Fixed> m_desiredDir = FixedVector2<Fixed>.Zero;
    private FixedVector2<Fixed> m_currentDir = FixedVector2<Fixed>.Zero;
    private FixedVector2<Fixed> m_incomingDir = FixedVector2<Fixed>.Zero;
    protected Vector3Int m_targetCell;

    protected abstract Vector3Int CalculateTargetCell(Vector3Int currentCell);

    private void Awake()
    {
        m_speed = new FixedPoint<Fixed>(speedRaw);
        m_startSpeed = m_speed;
        m_transform = new FixedTransform2D<Fixed>();
        m_mover = new FixedMover2D<Fixed> { Transform = m_transform };

        Vector3Int startCell = collisionMap.WorldToCell(transform.position);

        m_mover.SnapToCellFromUnity(transform.position, tileSize, grid.origin);
        m_targetCell = CalculateTargetCell(startCell);

        TickManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (TickManager.Instance != null)
            TickManager.Instance.Unregister(this);
    }

    public void FixedTick()
    {
        SimulationStep();
    }

    private void SimulationStep()
    {

        // --- BLOCK N°01 ; Récupération position et cellule ---

        FixedVector2<Fixed> pos = m_mover.Position2D;
        var worldPos = new Vector3(
                pos.x.ToFloat(),
                pos.y.ToFloat(),
                transform.position.z
            );

        Vector3Int cell = collisionMap.WorldToCell(worldPos);
        FixedVector2<Fixed> center = GetFixedCenter(cell);

        // --- BLOCK N°02 : Détection si intersection ---

        int walkableCount = 0;

        foreach (var off in new[] { Vector3Int.up, Vector3Int.left, Vector3Int.down, Vector3Int.right })
        {
            var logic = grid.GetCell(cell + off);
            if (logic != null && logic.isGhostWalkable) walkableCount++;
            if (logic != null && logic.isGhostWalkable && (logic.isWarp || logic.isPortal)) m_speed = m_speed / 2;
            else
            {
                m_speed = m_startSpeed;
            }
        }
        bool atIntersection = walkableCount > 1;

        // --- BLOCK N°03 : Récupérer la cible ---

        if (pos == center)
        {

        }

        // --- BLOCK N°04 : Choisir sa direction ---

        if (m_currentDir == FixedVector2<Fixed>.Zero || pos == center && atIntersection)
        {
            m_targetCell = CalculateTargetCell(cell);

            m_desiredDir = ChooseDirection(cell, m_incomingDir, m_targetCell);
            if (m_desiredDir != FixedVector2<Fixed>.Zero)
            {
                m_currentDir = m_desiredDir;
            }

        }

        // --- BLOCK N°05 : Déplacement ---

        if (m_currentDir != FixedVector2<Fixed>.Zero)
        {

            if (CanMove(cell, m_currentDir))
            {
                // Eviter le Tunneling
                SafeMove(m_currentDir);
            }

            else
            {
                var deltaX = center.x - pos.x;
                var deltaY = center.y - pos.y;

                var moveX = FixedMath.Abs(deltaX).Raw <= m_speed.Raw
                    ? deltaX
                    : FixedMath.Sign(deltaX) * m_speed;

                var moveY = FixedMath.Abs(deltaY).Raw <= m_speed.Raw
                    ? deltaY
                    : FixedMath.Sign(deltaY) * m_speed;

                m_mover.Move(new FixedVector2<Fixed>(moveX, moveY));

                if (deltaX.Raw == moveX.Raw
                    && deltaY.Raw == moveY.Raw)
                    m_currentDir = FixedVector2<Fixed>.Zero;
            }
        }
    }

    private FixedVector2<Fixed> GetFixedCenter(Vector3Int cell)
    {
        Vector3 worldCenter = collisionMap.GetCellCenterLocal(cell);
        return new FixedVector2<Fixed>(worldCenter.x, worldCenter.y);
    }

    private bool CanMove(Vector3Int cell, FixedVector2<Fixed> dir)
    {
        int dx = dir.x.Raw > 0 ? 1 : dir.x.Raw < 0 ? -1 : 0;
        int dy = dir.y.Raw > 0 ? 1 : dir.y.Raw < 0 ? -1 : 0;

        var nextCell = new Vector3Int(cell.x + dx, cell.y + dy, 0);
        var logic = grid.GetCell(nextCell);


        return (logic != null && logic.isWalkable) || logic == null;
    }

    private void SafeMove(FixedVector2<Fixed> direction)
    {
        // Subdivise le déplacement en 4 sous-étapes pour détecter une mur en cours
        // de route.
        const int subSteps = 4;

        var step = m_speed / FixedPoint<Fixed>.FromInt(subSteps);
        var start = m_mover.Position2D;

        for (int i = 1; i < subSteps; i++)
        {
            var offset = direction * (step * FixedPoint<Fixed>.FromInt(i));
            var nextPos = start + offset;

            Vector3 worldPos = new Vector3(
                nextPos.x.ToFloat(),
                nextPos.y.ToFloat(),
                0f);

            Vector3Int cell = collisionMap.WorldToCell(worldPos);

            var logic = grid.GetCell(cell);
            if (logic != null && !logic.isGhostWalkable)
                return;
        }

        m_mover.Move(direction * m_speed);
        m_incomingDir = direction;
    }

    // IA Greedy
    private FixedVector2<Fixed> ChooseDirection(
    Vector3Int currentCell, FixedVector2<Fixed> previousDir, Vector3Int targetCell)
    {
        FixedVector2<Fixed> bestDir = FixedVector2<Fixed>.Zero;
        int bestDist2 = int.MaxValue;

        // haut, gauche, bas, droit
        var offsets = new[] { Vector3Int.up, Vector3Int.left, Vector3Int.down, Vector3Int.right };
        var direction = new[]
        {
            new FixedVector2<Fixed>(FixedPoint<Fixed>.FromInt(0), FixedPoint<Fixed>.FromInt(1)),  // Up
            new FixedVector2<Fixed>(FixedPoint<Fixed>.FromInt(-1), FixedPoint<Fixed>.FromInt(0)), // Left
            new FixedVector2<Fixed>(FixedPoint<Fixed>.FromInt(0), FixedPoint<Fixed>.FromInt(-1)), // Down
            new FixedVector2<Fixed>(FixedPoint<Fixed>.FromInt(1), FixedPoint<Fixed>.FromInt(0))   // Right
        };

        for (int i = 0; i < 4; i++)
        {

            var off = offsets[i];
            var dir = direction[i];

            if (dir == -previousDir) continue;
            if (!CanMove(currentCell, dir)) continue;

            var caseAdjacent = currentCell + off;
            int dx = caseAdjacent.x - targetCell.x;
            int dy = caseAdjacent.y - targetCell.y;
            int dist2 = dx * dx + dy * dy;

            if (bestDir == FixedVector2<Fixed>.Zero || dist2 < bestDist2)
            {
                bestDir = dir;
                bestDist2 = dist2;

            }
        }

        return bestDir;
    }

    void LateUpdate()
    {
        m_transform.ApplyToTransform2D(transform);
    }



}
