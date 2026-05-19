using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using PacMan.Input;

namespace FixedEngine
{

    [DefaultExecutionOrder(-1000)]
    public class TickManager : MonoBehaviour
    {
        // TickMachineRunner est une classe abriquée (dans TickManager)
        private sealed class TickMachineRunner
        {
            // StopWatch est une horloge du sytème d'exploitation indépendante de Unity.
            // C'est une horloge de haute précision bien plus précise que Time.time.
            private readonly Stopwatch hostClock = new();

            // nextTickDeadLine est le moment où le prochain Tick doit être déclencher.
            // A chaque Pump(), si l'heure actuelle n'a pas encore atteint cette deadline, alors on ne fait rien.
            // Dès qu'elle est atteinte, on déclenche un Tick et on avance la deadline d'un interval.
            private double nextTickDeadLine;

            private bool isInitialized;
            
            public void Reset(double tickInterval)
            {
                hostClock.Restart();
                nextTickDeadLine = tickInterval;
                isInitialized = true;
            }

            public void Pump(TickManager manager)
            {
                if (!isInitialized) Reset(manager.tickInterval);
                double now = hostClock.Elapsed.TotalSeconds;
                if (now < nextTickDeadLine) return;

                manager.StepTick();
                nextTickDeadLine += manager.tickInterval;
            }



        }

        public static TickManager Instance { get; private set; }

        public int ticksPerSeconds = 60;

        public double tickInterval;
        public double accumulator;

        // L'avantage d'utiliser une liste pour le déclenchement des FixedTick() dans les autres objets, c'est qu'on a un ordre clair entre eux.
        private readonly List<IFixedTick> listeners = new();

        private readonly TickMachineRunner runner = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            RecomputeTickInterval();
            accumulator = 0.0;
            FixedTickContext.Reset();
            runner.Reset(tickInterval);
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.pauseStateChanged += OnEditorPauseState;
#endif
        }
        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.pauseStateChanged -= OnEditorPauseState;
#endif
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void RecomputeTickInterval()
        {
            ticksPerSeconds = Mathf.Max(1, ticksPerSeconds);
            tickInterval = 1.0 / ticksPerSeconds;
        }

        private void Update()
        {
            runner.Pump(this);
        }

        public void StepTick()
        {
            accumulator = 0.0;
            PacManTickInput.CaptureCurrentTick();
            FixedTickContext.AdvanceTick();

            for(int i = 0, n = listeners.Count; i < n; i++)
            {
                listeners[i].FixedTick();
            }
        }

        public void Register(IFixedTick tick)
        {
            if (tick == null || listeners.Contains(tick)) return;

            listeners.Add(tick);
        }

        public void Unregister(IFixedTick tick)
        {
            if (tick == null) return;
            listeners.Remove(tick);
        }

#if UNITY_EDITOR

        private void OnEditorPauseState(UnityEditor.PauseState pauseState)
        {
            if(!Application.isPlaying) return;

            if(pauseState == UnityEditor.PauseState.Unpaused)
            {
                accumulator = 0.0;
                runner.Reset(tickInterval);
            }
        }
#endif


    }

    // IFixedTick est le contrat que tout système doit implémenter pour participer à la boucle logique.
    // Une Interface est un contrat qui force tous ceux qui en héritent à implémenter ses fonctions. Dans notre cas, FixedTick() doit donc être implémenter à tous les enfants.
    // Ce pattern est appelé pattern Observer.
    public interface IFixedTick
    {
        void FixedTick();
    }

}
