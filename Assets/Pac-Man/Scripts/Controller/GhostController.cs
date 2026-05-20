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

    protected FixedPoint<Fixed> m_speed;
    protected FixedTransform2D<Fixed> m_transform;
    protected FixedMover2D<Fixed> m_mover;

    protected FixedVector2<Fixed> m_currentDir = FixedVector2<Fixed>.Zero;

    public Tilemap CollisionMapDebug => collisionMap;

    public abstract void FixedTick();

    public abstract bool CanMove(Vector3Int cell, FixedVector2<Fixed> dir);

    public abstract void SafeMove(FixedVector2<Fixed> direction);

    void LateUpdate()
    {
        m_transform.ApplyToTransform2D(transform);
    }
}
