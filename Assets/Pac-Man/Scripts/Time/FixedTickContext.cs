namespace FixedEngine
{

    public static class FixedTickContext
    {
        public static int CurrentTick { get; private set; } = -1;

        public static void Reset() => CurrentTick = -1;

        public static void AdvanceTick() => CurrentTick++;

    }




}