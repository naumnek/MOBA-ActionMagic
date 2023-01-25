using Platinum.Controller;
using UnityEngine;
using UnityEngine.UI;
using Platinum.CustomEvents;
using Platinum.Info;

namespace Platinum.UI
{
    public class FeedbackFlashHUD : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Image component of the flash")]
        public Image FlashImage;

        [Tooltip("CanvasGroup to fade the damage flash, used when recieving damage end healing")]
        public CanvasGroup FlashCanvasGroup;

        [Tooltip("CanvasGroup to fade the critical health vignette")]
        public CanvasGroup VignetteCanvasGroup;

        [Header("Damage")]
        [Tooltip("Color of the damage flash")]
        public Color DamageFlashColor;

        [Tooltip("Color of the damage flash when invulnerable")]
        public Color InvulnerableFlashColor;

        [Tooltip("Duration of the damage flash")]
        public float DamageFlashDuration;

        [Tooltip("Max alpha of the damage flash")]
        public float DamageFlashMaxAlpha = 1f;

        [Header("Critical health")]
        [Tooltip("Max alpha of the critical vignette")]
        public float CriticaHealthVignetteMaxAlpha = .8f;

        [Tooltip("Frequency at which the vignette will pulse when at critical health")]
        public float PulsatingVignetteFrequency = 4f;

        [Header("Heal")]
        [Tooltip("Color of the heal flash")]
        public Color HealFlashColor;

        [Tooltip("Duration of the heal flash")]
        public float HealFlashDuration;

        [Tooltip("Max alpha of the heal flash")]
        public float HealFlashMaxAlpha = 1f;

        bool m_FlashActive;
        float m_LastTimeFlashStarted = Mathf.NegativeInfinity;
        Health m_PlayerHealth;
        GameFlowManager m_GameFlowManager;

        private bool Pause = true;

        private void OnDestroy()
        {
            EventManager.RemoveListener<EndSpawnEvent>(OnPlayerSpawnEvent);
            EventManager.RemoveListener<GamePauseEvent>(OnGamePauseEvent);
        }

        private void Awake()
        {
            EventManager.AddListener<GamePauseEvent>(OnGamePauseEvent);
            EventManager.AddListener<EndSpawnEvent>(OnPlayerSpawnEvent);
        }

        private void OnGamePauseEvent(GamePauseEvent evt)
        {
            Pause = evt.ServerPause;
        }

        private void OnPlayerSpawnEvent(EndSpawnEvent evt)
        {
            m_GameFlowManager = evt.LoadManager.GameFlowManager;

            // Subscribe to player damage events
            ControllerBase controller = evt.LoadManager.ControllerPlayer;
            m_PlayerHealth = controller.Health;

            controller.onDamaged += OnTakeDamage;
            controller.onHealed += OnHealed;

            Pause = false;
        }


        void Update()
        {
            if (!Pause)
            {
                if (m_PlayerHealth != null && m_PlayerHealth.IsCritical() && !m_PlayerHealth.IsDead)
                {
                    VignetteCanvasGroup.gameObject.SetActive(true);
                    float vignetteAlpha =
                        (1 - (m_PlayerHealth.CurrentHealth / m_PlayerHealth.MaxHealth /
                              m_PlayerHealth.CriticalHealthRatio)) * CriticaHealthVignetteMaxAlpha;

                    if (m_GameFlowManager.GameIsEnding)
                        VignetteCanvasGroup.alpha = vignetteAlpha;
                    else
                        VignetteCanvasGroup.alpha =
                            ((Mathf.Sin(Time.time * PulsatingVignetteFrequency) / 2) + 0.5f) * vignetteAlpha;
                }
                else
                {
                    VignetteCanvasGroup.gameObject.SetActive(false);
                }


                if (m_FlashActive)
                {
                    float normalizedTimeSinceDamage = (Time.time - m_LastTimeFlashStarted) / DamageFlashDuration;

                    if (normalizedTimeSinceDamage < 1f)
                    {
                        float flashAmount = DamageFlashMaxAlpha * (1f - normalizedTimeSinceDamage);
                        FlashCanvasGroup.alpha = flashAmount;
                    }
                    else
                    {
                        FlashCanvasGroup.gameObject.SetActive(false);
                        m_FlashActive = false;
                    }
                }
            }
        }

        void ResetFlash()
        {
            m_LastTimeFlashStarted = Time.time;
            m_FlashActive = true;
            FlashCanvasGroup.alpha = 0f;
            FlashCanvasGroup.gameObject.SetActive(true);
        }

        void OnTakeDamage(float dmg, Actor hitActor)
        {
            ResetFlash();
            if (m_PlayerHealth.invulnerable) FlashImage.color = InvulnerableFlashColor;
            else FlashImage.color = DamageFlashColor;
        }

        void OnHealed(float amount)
        {
            ResetFlash();
            FlashImage.color = HealFlashColor;
        }
    }
}