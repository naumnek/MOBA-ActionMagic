using UnityEngine;
using UnityEngine.Events;
using Platinum.CustomInput;
using Platinum.Controller;

namespace Platinum.Weapon
{
    [RequireComponent(typeof(AudioSource))]
    public class Jetpack : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Audio source for jetpack sfx")]
        public AudioSource AudioSource;

        [Tooltip("Particles for jetpack vfx")] public ParticleSystem[] JetpackVfx;

        [Header("Parameters")]
        [Tooltip("Whether the jetpack is unlocked at the begining or not")]
        public bool IsJetpackUnlockedAtStart = false;

        [Tooltip("The strength with which the jetpack pushes the player up")]
        public float JetpackAcceleration = 7f;

        [Range(0f, 1f)]
        [Tooltip(
            "This will affect how much using the jetpack will cancel the gravity value, to start going up faster. 0 is not at all, 1 is instant")]
        public float JetpackDownwardVelocityCancelingFactor = 1f;

        [Header("Durations")]
        [Tooltip("Time it takes to consume all the jetpack fuel")]
        public float ConsumeDuration = 1.5f;

        [Tooltip("Time it takes to completely refill the jetpack while on the ground")]
        public float RefillDurationGrounded = 2f;

        [Tooltip("Time it takes to completely refill the jetpack while in the air")]
        public float RefillDurationInTheAir = 5f;

        [Tooltip("Delay after last use before starting to refill")]
        public float RefillDelay = 1f;

        [Header("Audio")]
        [Tooltip("Sound played when using the jetpack")]
        public AudioClip JetpackSfx;

        bool m_CanUseJetpack;
        Movement m_Movement;
        float m_LastTimeOfUse;
        private MoveKeycode MoveKeycode;

        // stored ratio for jetpack resource (1 is full, 0 is empty)
        public float CurrentFillRatio { get; private set; }
        public bool IsJetpackUnlocked { get; private set; }

        public bool IsPlayergrounded() => m_Movement.CharacterController.isGrounded;

        public UnityAction<bool> OnUnlockJetpack;

        void Start()
        {
            IsJetpackUnlocked = IsJetpackUnlockedAtStart;

            m_Movement = GetComponent<Movement>();

            CurrentFillRatio = 1f;

            AudioSource.clip = JetpackSfx;
            AudioSource.loop = true;
        }

        void Update()
        {
            MoveKeycode = PlayerInputHandler.MoveKeycode;
            // jetpack can only be used if not grounded and jump has been pressed again once in-air
            if (IsPlayergrounded())
            {
                m_CanUseJetpack = false;
            }
            else if (PlayerInputHandler.MoveKeycode.jump)
            {
                m_CanUseJetpack = true;
            }

            // jetpack usage
            bool jetpackIsInUse = m_CanUseJetpack && IsJetpackUnlocked && CurrentFillRatio > 0f &&
                                  PlayerInputHandler.MoveKeycode.jump;
            if (jetpackIsInUse)
            {
                // store the last time of use for refill delay
                m_LastTimeOfUse = Time.time;

                float totalAcceleration = JetpackAcceleration;

                // cancel out gravity
                /*
                totalAcceleration += m_Character.GravityDownForce;

                if (m_Character.CharacterController.CharacterVelocity.y < 0f)
                {
                    // handle making the jetpack compensate for character's downward velocity with bonus acceleration
                    totalAcceleration += ((-m_Character.CharacterVelocity.y / Time.deltaTime) *
                                          JetpackDownwardVelocityCancelingFactor);
                }

                // apply the acceleration to character's velocity
                m_Character.CharacterVelocity += Vector3.up * totalAcceleration * Time.deltaTime;

                // consume fuel
                CurrentFillRatio = CurrentFillRatio - (Time.deltaTime / ConsumeDuration);

                for (int i = 0; i < JetpackVfx.Length; i++)
                {
                    var emissionModulesVfx = JetpackVfx[i].emission;
                    emissionModulesVfx.enabled = true;
                }

                if (!AudioSource.isPlaying)
                    AudioSource.Play();
                */
            }
            else
            {
                // refill the meter over time
                if (IsJetpackUnlocked && Time.time - m_LastTimeOfUse >= RefillDelay)
                {
                    float refillRate = 1 / (m_Movement.CharacterController.isGrounded
                        ? RefillDurationGrounded
                        : RefillDurationInTheAir);
                    CurrentFillRatio = CurrentFillRatio + Time.deltaTime * refillRate;
                }

                for (int i = 0; i < JetpackVfx.Length; i++)
                {
                    var emissionModulesVfx = JetpackVfx[i].emission;
                    emissionModulesVfx.enabled = false;
                }

                // keeps the ratio between 0 and 1
                CurrentFillRatio = Mathf.Clamp01(CurrentFillRatio);

                if (AudioSource.isPlaying)
                    AudioSource.Stop();
            }
        }

        public bool TryUnlock()
        {
            if (IsJetpackUnlocked)
                return false;

            OnUnlockJetpack.Invoke(true);
            IsJetpackUnlocked = true;
            m_LastTimeOfUse = Time.time;
            return true;
        }
    }
}