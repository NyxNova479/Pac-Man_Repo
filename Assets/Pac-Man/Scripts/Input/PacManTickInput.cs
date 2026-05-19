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

        // On a ici un private setter. Donc seule cette classe peut définir la valeure de Currentcommand.
        // PacManController comme tout système extérieur n'est autorisé qu'à la lire.
        public static PacManCommand CurrentCommand { get; private set; } = PacManCommand.None;

        public static void Configure(InputActionAsset inputAsset)
        {
            if (inputAsset == null) return;

            if(configuredAsset  == inputAsset && moveAction != null) return
        }
    }




}
