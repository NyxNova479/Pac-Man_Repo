using FixedEngine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PacMan.Input
{
    public enum PacManCommand
    {
        None,
        Up,
        Left,
        Down,
        Right,
    }

    public static class PacManTickInput
    {
        private const string PlayerMapName = "Player";
        private const string MoveActionName = "Move";

        private static InputAction moveAction;
        private static InputActionAsset configuredAsset;

        public static PacManCommand CurrentCommand { get; private set; } = PacManCommand.None;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            moveAction = null;
            configuredAsset = null;
            CurrentCommand = PacManCommand.None;
        }

        public static void Configure(InputActionAsset inputAsset)
        {
            if (inputAsset == null)
            {
                Debug.LogError("[PacManTickInput] Aucun InputActionAsset assigne.");
                return;
            }

            if (configuredAsset == inputAsset && moveAction != null)
                return;

            Release(configuredAsset);

            var map = inputAsset.FindActionMap(PlayerMapName, true);
            moveAction = map.FindAction(MoveActionName, true);
            moveAction.Enable();

            configuredAsset = inputAsset;
            CurrentCommand = PacManCommand.None;
        }

        public static void Release(InputActionAsset inputAsset)
        {
            if (moveAction == null)
                return;

            if (inputAsset != null && configuredAsset != inputAsset)
                return;

            moveAction.Disable();
            moveAction = null;
            configuredAsset = null;
            CurrentCommand = PacManCommand.None;
        }

        internal static void CaptureCurrentTick()
        {
            CurrentCommand = ResolveCommand(ReadMoveInput());
        }

        internal static FixedVector2<Q8_8> ToFixedDirection(PacManCommand command)
        {
            switch (command)
            {
                case PacManCommand.Up:
                    return FixedVector2<Q8_8>.Up;
                case PacManCommand.Left:
                    return FixedVector2<Q8_8>.Left;
                case PacManCommand.Down:
                    return FixedVector2<Q8_8>.Down;
                case PacManCommand.Right:
                    return FixedVector2<Q8_8>.Right;
                default:
                    return FixedVector2<Q8_8>.Zero;
            }
        }

        private static Vector2 ReadMoveInput()
        {
            return moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        }

        private static PacManCommand ResolveCommand(Vector2 rawInput)
        {
            var inputDir = FixedInputQuantizer.QuantizeVector2<Q8_8>(rawInput);

            if (inputDir.x.Raw != 0 && inputDir.y.Raw == 0)
                return inputDir.x.Raw > 0 ? PacManCommand.Right : PacManCommand.Left;

            if (inputDir.y.Raw != 0 && inputDir.x.Raw == 0)
                return inputDir.y.Raw > 0 ? PacManCommand.Up : PacManCommand.Down;

            return PacManCommand.None;
        }
    }
}
