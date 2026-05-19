using UnityEngine;
using System;
using UnityEditor.PackageManager.Requests;

namespace FixedEngine
{
    public struct FixedTimer<TFormat> where TFormat : struct, IFixedPointFormat
    {
        public string Label;

        // Q8.4 ou. Q8.8 c'est le TFormat qui découle du IFixedPointFormat
        private FixedPoint<TFormat> m_duration;
        private FixedPoint<TFormat> m_elapsed;
        private FixedPoint<TFormat> m_lastElapsed;


        public bool Loop;
        private int m_repeatCount;
        public int RepeatCount;

        public bool IsPaused { get; private set; }
        private bool m_wasFinishedLastFrame;

        public Action OnFinish;


        // --- CALCUL DES PROPRIETES ---

        public bool IsInitialized => m_duration.Raw > 0;
        public bool IsFinished => !Loop && RepeatCount <= 0 && m_elapsed >= m_duration;
        public bool IsFinishedNow => m_elapsed >= m_duration;
        public bool DoneThisFrame => !IsPaused && !m_wasFinishedLastFrame && IsFinishedNow;
        public bool IsRunning => !IsPaused && !IsFinished;
        public bool HasJustLooped => Loop && DoneThisFrame && (RepeatCount == 0 || m_repeatCount == 0);

        public FixedPoint<TFormat> Remaining => m_duration - m_elapsed;
        public FixedPoint<TFormat> Elapsed => m_elapsed;
        public float Progress01 => Math.Clamp(m_elapsed.ToFloat() / m_duration.ToFloat(), 0.0f, 1.0f);


        // --- CONSTRUCTEUR ---

        public FixedTimer(float durationSeconds, bool loop = false, int repeatCount = 0, string label = "")
        {
            Label = label;
            m_duration = FixedPoint<TFormat>.FromFloat(durationSeconds);
            m_elapsed = FixedPoint<TFormat>.FromFloat(0f);
            m_lastElapsed = m_elapsed;
            Loop = loop;
            m_repeatCount = repeatCount;
            RepeatCount = repeatCount;
            OnFinish = null;
            IsPaused = false;
            m_wasFinishedLastFrame = false;

            if(m_duration.Raw <= 0)
            {
                Debug.Log($"[FixedTimer<{typeof(TFormat).Name}>] [{label}] Timer créé avec une durée nulle ou négative");
            }
        }

        
        // --- COMMANDES ---

        public void Restart(float newDurationSeconds)
        {
            m_duration = FixedPoint<TFormat>.FromFloat(newDurationSeconds);
            Reset();
        }

        public void Restart() => Reset();

        public void Reset()
        {
            m_elapsed = FixedPoint<TFormat>.FromFloat(0f);
            m_lastElapsed = m_elapsed;
            m_repeatCount = RepeatCount;
            IsPaused = false;
            m_wasFinishedLastFrame = false;
        }

        public void Stop()
        {
            IsPaused = true;
            m_elapsed = m_duration;
            m_wasFinishedLastFrame = true;
        }

        public void Pause() => IsPaused = true;

        public void Resume() => IsPaused = false;

        public void SetDuration(float seconds)
        {
            m_duration = FixedPoint<TFormat>.FromFloat(seconds);
        }

        public void SetDuration(FixedPoint<TFormat> duration)
        {
            m_duration = duration;
        }

        public void Update(FixedPoint<TFormat> deltaTime)
        {
            if (!IsInitialized) return;

            if (IsPaused) return;

            m_lastElapsed = m_elapsed;
            m_elapsed += deltaTime;

            if(IsFinishedNow && !m_wasFinishedLastFrame)
            {
                OnFinish?.Invoke();

                if(Loop || RepeatCount > 0)
                {
                    m_elapsed = FixedPoint<TFormat>.FromFloat(0f);

                    if(RepeatCount > 0)
                    {
                        m_repeatCount--;
                    }

                    m_wasFinishedLastFrame = !(RepeatCount == 0 || m_repeatCount > 0);
                }
                else
                {
                    m_wasFinishedLastFrame = true;
                }
            }
            else if (!IsFinishedNow)
            {
                m_wasFinishedLastFrame = false;
            }


        }

        public bool Triggered(float secondsThreshold)
        {
            var threshold = FixedPoint<TFormat>.FromFloat(secondsThreshold);

            return !IsPaused && m_elapsed >= threshold && m_lastElapsed < threshold;
        }
    }
}


