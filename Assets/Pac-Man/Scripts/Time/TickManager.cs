using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using PacMan.Input;

namespace FixedEngine
{
    [DefaultExecutionOrder(-1000)]
    public class TickManager : MonoBehaviour
    {
        // sealed == ne peux pas etre herité
        //tickmachinerunner est une claase imbriquée dans tickmanager

        private sealed class TickMachineRunner
        {
            //stopwatch est une horloge du système d'exploitation, indépendante d'unity.
            // c'est une horloge haute précision, bien plus précise que Time.time         
            private readonly Stopwatch hostClock = new();

            //nexttickdeadline est le moment auquel le prochain tick doit etre déclencher
            //A chaque Pump(), si l'heure actuelle n'a pas encore atteint cette deadline
            // on ne fait rien. Dès qu'elle est atteinte, on déclenche un tick et on avance la deadline d'un interval.
            private double nextTickDeadline;
            private bool isInitialized;

            public void Reset(double tickInterval)
            {
                hostClock.Restart();
                nextTickDeadline = tickInterval;
                isInitialized = true;
            }

            public void Pump(TickManager manager)
            {
                if (!isInitialized) Reset(manager.tickInterval);

                double now = hostClock.Elapsed.TotalSeconds;
                if (now < nextTickDeadline) return;

                manager.StepTick();
                nextTickDeadline += manager.tickInterval;
            }


        }

        public static TickManager Instance { get; private set; }

        public int ticksPerSecond = 60;

        public double tickInterval;
        public double accumulator;

        // l'avantage d utiliser une liste pour le déclenchement des FixedTick() dans les autres objets, c'est qu'on a un ordre clair entre eux. sinon on sait pas lequel frame le premier
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
            accumulator = 0;
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
            if (Instance == this)
                Instance = null;
        }

        private void RecomputeTickInterval()
        {
            ticksPerSecond = Mathf.Max(1, ticksPerSecond);
            tickInterval = 1.0 / ticksPerSecond;
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

            for (int i = 0, n = listeners.Count; i < n; i++)
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
            if (!Application.isPlaying) return;

            if (pauseState == UnityEditor.PauseState.Unpaused)
            {
                accumulator = 0.0;
                runner.Reset(tickInterval);
            }
        }


#endif
    }

    // IFixedTick EST LE CONTRAT que tout système doit impl"menter pour participer à la boucle logique
    // force les héritant à impléménter ses fonctions comme FixedTick()
    public interface IFixedTick
    {
        void FixedTick();
    }
}
