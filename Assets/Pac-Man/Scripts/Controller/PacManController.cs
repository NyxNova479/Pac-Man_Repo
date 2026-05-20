using UnityEngine;
using FixedEngine;
using PacMan.Input;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

using Fixed = FixedEngine.Q8_8;
//using System.Linq.Expressions;
public class PacManController : MonoBehaviour, IFixedTick
{
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private Tilemap collisionMap;
    [SerializeField] private int speedRaw = 256;
    [SerializeField] private int tileSize = 8;
    [SerializeField] private LogicGrid grid;

    private FixedPoint<Fixed> m_speed;
    private FixedTransform2D<Fixed> m_transform;
    private FixedMover2D<Fixed> m_mover;

    private FixedVector2<Fixed> m_desiredDir = FixedVector2<Fixed>.Zero;
    public FixedVector2<Fixed> m_currentDir = FixedVector2<Fixed>.Zero;
    private bool b_isPreTurning = false;

    private FixedPoint<Fixed> m_turnTargetX;
    private FixedPoint<Fixed> m_turnTargetY;

    public Tilemap CollisionMapDebug => collisionMap;

    void Awake()
    {
        PacManTickInput.Configure(inputActionAsset);

        m_speed = new FixedPoint<Fixed>(speedRaw);
        m_transform = new FixedTransform2D<Fixed>();
        m_mover = new FixedMover2D<Fixed>() { Transform = m_transform };

        m_mover.SnapToCellFromUnity(transform.position, tileSize, grid.origin);

        Vector3Int startCell = collisionMap.WorldToCell(transform.position);
        var logic = grid.GetCell(startCell);

        Debug.Log($"[PacManController] cellule de départ : {startCell} -> {logic?.tileType}");
        Debug.Log($"[PacManController] speedRaw = {speedRaw}, = speed.Raw = {m_speed.Raw}");

        TickManager.Instance.Register(this);

        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
        QualitySettings.maxQueuedFrames = 1;
    }

    private void OnDestroy()
    {
        if (TickManager.Instance != null)
        {
            TickManager.Instance.Unregister(this);
        }
        PacManTickInput.Release(inputActionAsset);
    }
    public void FixedTick()
    {


        // BLOC 1 maj input
        var inputDir = PacManTickInput.ToFixedDirection(PacManTickInput.CurrentCommand);
        if (inputDir != FixedVector2<Fixed>.Zero)
        {
            // m_desiredDir mis a jour a chaques tick ou le joueur exprime une direction. Si le joueur fait rien m_desiredDir conserve sa dernière valeur
            if (!b_isPreTurning)
            {
                m_desiredDir = inputDir;
            }
            else if (inputDir != m_currentDir && inputDir != -m_currentDir)
            {
                m_currentDir = inputDir;
            }

        }

        // BLOC 2 position cellule courantes

        FixedVector2<Fixed> pos = m_mover.Position2D;
        var worldPos = new Vector3(pos.x.ToFloat(), pos.y.ToFloat(), transform.position.z);
        Vector3Int cell = collisionMap.WorldToCell(worldPos);
        FixedVector2<Fixed> center = GetFixedCenter(cell);

        // BLOC 3 Demarage
        if (m_currentDir == FixedVector2<Fixed>.Zero && m_desiredDir != FixedVector2<Fixed>.Zero && CanMove(cell, m_desiredDir))
        {
            m_currentDir = m_desiredDir;
        }

        // BLOC 4 demi tour immediat
        if (m_desiredDir == -m_currentDir && m_desiredDir != FixedVector2<Fixed>.Zero && CanMove(cell, m_desiredDir))
        {
            m_currentDir = m_desiredDir;
        }

        // BLOC 5 pre turn
        if (!b_isPreTurning && m_currentDir != FixedVector2<Fixed>.Zero && m_desiredDir != m_currentDir && m_desiredDir != -m_currentDir && CanMove(cell, m_desiredDir))
        {
            var nextCell = cell + new Vector3Int(FixedMath.Sign(m_desiredDir.x), FixedMath.Sign(m_desiredDir.y), 0);

            var logic = grid.GetCell(nextCell);
            if (logic == null || !logic.isWalkable) goto SkipPreTurn; // teleporte vers SkipPreTurn

            var nextCenter = GetFixedCenter(nextCell);
            if (m_currentDir.x.Raw != 0) // horizontal > vertical
            {
                m_turnTargetX = center.x;
                m_turnTargetY = nextCenter.y;
            }
            else // vertical > horizontal
            {
                m_turnTargetX = nextCenter.x;
                m_turnTargetY = center.y;
            }

            b_isPreTurning = true;
        }
    SkipPreTurn:
        // --- BLOC N°06 : EXECUTION DU PRE TURN ---

        if (b_isPreTurning)
        {
            FixedVector2<Fixed> curPos = m_mover.Position2D;
            var dx = m_turnTargetX - curPos.x;
            var dy = m_turnTargetY - curPos.y;

            FixedPoint<Fixed> moveX = new FixedPoint<Fixed>(0);
            FixedPoint<Fixed> moveY = new FixedPoint<Fixed>(0);

            if (dx.Raw != 0) moveX = FixedMath.Abs(dx).Raw <= m_speed.Raw ? dx : FixedMath.Sign(dx) * m_speed;
            if (dy.Raw != 0) moveY = FixedMath.Abs(dy).Raw <= m_speed.Raw ? dy : FixedMath.Sign(dy) * m_speed;

            m_mover.Move(new FixedVector2<Fixed>(moveX, moveY));

            if (dx.Raw == 0 && dy.Raw == 0)
            {
                b_isPreTurning = false;
                m_currentDir = m_desiredDir;
            }
            return;
        }

        // BLOC 7 Deplacement normal
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

    void LateUpdate()
    {
        m_transform.ApplyToTransform2D(transform);
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
        return (logic != null &&  logic.isWalkable) || logic == null;
        //return LogicCell.isWalkable
    }

    private void SafeMove(FixedVector2<Fixed> direction)
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
}
