using UnityEngine;
using FixedEngine;
using UnityEngine.InputSystem;

namespace PacMan.Input
{
    using Fixed = Q8_8;

    public enum PacManCommand
    {
        None,
        Up,
        Left,
        Down,
        Right
    }
    public static class PacManTickInput
    {
        private const string PlayerMapName = "Player";
        private const string MoveActionName = "Move";

        private static InputAction moveAction;
        private static InputActionAsset configuredAsset;

        // on a un private setter donc seul cette classe peut définir la valeur de currentCommand. PacManController comme tout système extérieur n'est autorisé qu'a la lire
        public static PacManCommand CurrentCommand { get; private set; }
            = PacManCommand.None;

        public static void Configure(InputActionAsset inputAsset)
        {
            if (inputAsset == null) return;

            if (configuredAsset == inputAsset && moveAction != null)
                return;
            Release(configuredAsset);

            var map = inputAsset.FindActionMap(PlayerMapName, true);
            moveAction = map.FindAction(MoveActionName);
            moveAction.Enable();

            configuredAsset = inputAsset;
            CurrentCommand = PacManCommand.None;
        }

        public static void Release(InputActionAsset inputAsset)
        {
            if (moveAction == null) return;

            if (inputAsset != null && configuredAsset != inputAsset) return;

            moveAction.Disable();
            moveAction = null;
            configuredAsset = null;
            CurrentCommand = PacManCommand.None;
        }

        internal static FixedVector2<Fixed> ToFixedDirection(PacManCommand command)
        {
            switch (command)
            {
                // Si command = Up on fait up ...
                case PacManCommand.Up: return FixedVector2<Fixed>.Up;
                case PacManCommand.Left: return FixedVector2<Fixed>.Left;
                case PacManCommand.Down: return FixedVector2<Fixed>.Down;
                case PacManCommand.Right: return FixedVector2<Fixed>.Right;
                default: return FixedVector2<Fixed>.Zero;
            }

        }

        private static Vector2 ReadMoveInput()
        {
            return moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        }

        private static PacManCommand ResolveCommand(Vector2 rawInput)
        {
            var inputDir = FixedInputQuantizer.QuantizeVector2<Fixed>(rawInput);

            if (inputDir.x.Raw != 0 && inputDir.y.Raw == 0)
                return inputDir.x > 0 ? PacManCommand.Right : PacManCommand.Left;

            if (inputDir.y.Raw != 0 && inputDir.x.Raw == 0)
                return inputDir.y > 0 ? PacManCommand.Up : PacManCommand.Down;

            return PacManCommand.None;
        }

        internal static void CaptureCurrentTick()
        {
            CurrentCommand = ResolveCommand(ReadMoveInput());
        }
        // la methode qui suit vas s'executer avant tout le reste
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            moveAction = null;
            configuredAsset = null;
            CurrentCommand = PacManCommand.None;
        }
    }
}