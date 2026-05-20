using System;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Rendering;

namespace FixedEngine
{
    public struct FixedTimer<TFormat> where TFormat : struct, IFixedPointFormat
    {
        public string Label;
        private FixedPoint<TFormat> m_duration;
        private FixedPoint<TFormat> m_elapsed;
        private FixedPoint<TFormat> m_lastElapsed;

        public bool Loop;
        public int RepeatCount;
        private int m_repeatCount;

        public bool IsPaused { get; private set; }
        private bool m_wasFinishedLastFrame;

        public Action OnFinish;

        public bool isInitialized => m_duration.Raw > 0;

        public bool isFinished => !Loop && RepeatCount <= 0 && m_elapsed >= m_duration;

        public bool isFinishedNow => m_elapsed >= m_duration;
        public bool DoneThisFrame => !isFinished && !m_wasFinishedLastFrame;

        public bool isRunning => !isFinished && !IsPaused;

        public bool HasJusteLooped => Loop && DoneThisFrame && (RepeatCount == 0 || m_repeatCount > 0);

        public FixedPoint<TFormat> Remaining => m_duration - m_elapsed;

        public FixedPoint<TFormat> Elapsed => m_elapsed;

        public float Progress01 => Math.Clamp(m_elapsed.ToFloat() / m_duration.ToFloat(), 0.0f, 1.0f);

        public FixedTimer(float durationSeconds, bool loop = false, int repeatCount = 0, string label = "")
        {
            Label = label;
            m_duration = FixedPoint<TFormat>.FromFloat(durationSeconds);
            m_elapsed = FixedPoint<TFormat>.FromFloat(0f);
            m_lastElapsed = m_elapsed;
            Loop = loop;
            RepeatCount = repeatCount;
            this.m_repeatCount = repeatCount;
            OnFinish = null;
            IsPaused = false;
            m_wasFinishedLastFrame = false;

            if (m_duration.Raw <= 0)
            {
                Debug.LogWarning($"[FixedTimer] Initialized with non-positive duration: {durationSeconds} seconds. Timer will be considered finished.");
            }

        }

        public void Restart(float newDurationSeconds)
        {
            m_duration = FixedPoint<TFormat>.FromFloat(newDurationSeconds);
            Reset();
        }

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
            if (!isInitialized) return;

            if (IsPaused) return;

            m_lastElapsed = m_elapsed;
            m_elapsed += deltaTime;

            m_wasFinishedLastFrame = isFinished;

            if (isFinishedNow && !m_wasFinishedLastFrame)
            {
                OnFinish?.Invoke();

                if (Loop || RepeatCount > 0)
                {
                    m_elapsed = FixedPoint<TFormat>.FromFloat(0.0f);
                    if (RepeatCount > 0)
                    {
                        RepeatCount--;
                    }
                    m_wasFinishedLastFrame = !(RepeatCount == 0 || m_repeatCount > 0);
                }
                else
                {
                    m_wasFinishedLastFrame = true;
                }

            }
            else if (!isFinishedNow)
            {
                m_wasFinishedLastFrame = false;
            }
        }

        public bool Triggered(float secondsThresholds)
        {
            var threshold = FixedPoint<TFormat>.FromFloat(secondsThresholds);
            return !IsPaused && m_elapsed >= threshold && m_lastElapsed < threshold;
        }
    }
}