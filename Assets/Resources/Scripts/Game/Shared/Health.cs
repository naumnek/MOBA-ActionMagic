using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Platinum.Settings;
using Platinum.CustomEvents;

namespace Platinum.Info
{
    public class Health : MonoBehaviour
    {
        [Header("General")]

        [Header("Health")]
        [Tooltip("Maximum amount of health")] 
        public float MaxHealth = 100f;
        [Tooltip("Delay after the last shot before starting to reload")]
        public float regeneratDelay = 2f;
        [Tooltip("Amount of health regeneraion per regeneratDelay")]
        public float healthRegenerationRate = 1f;
        
        [Header("OnDie")]
        [Tooltip("Time invulnorable")] 
        public float DelayInvulnerable = 2f;

        [Header("Damage")]
        [Tooltip("Health ratio at which the critical health vignette starts appearing")]
        public float CriticalHealthRatio = 0.3f;
        [Tooltip("Multiplier to apply to the received damage")]
        public float DamageMultiplier = 1f;
        [Range(0, 1)]
        [Tooltip("Multiplier to apply to self damage")]
        public float SensibilityToSelfdamage = 0.5f;

        public UnityAction<float, Actor> onDamaged;
        public UnityAction<float> onHealed;
        public UnityAction onFullHealthAfterDeath;
        public UnityAction onDie;

        public float CurrentHealth { get; set; }
        public bool CanPickup() => CurrentHealth < MaxHealth;

        public float Ratio => CurrentHealth / MaxHealth;
        public bool IsCritical() => Ratio <= CriticalHealthRatio;

        public bool invulnerable { get; private set; } = false;
        public bool isFullHealth => CurrentHealth >= MaxHealth;
        public void DisableInvulnerable()
        {
            invulnerable = false;
        }

        public bool IsDead { get; private set; }
        public bool IsInstantDead { get; private set; }
        public Actor Actor { get; private set; }
        private Actor lastHitActor;
        private float lastUpdateTime;
        private float boostDelay = 1f;
        private float boostAmount = 1f;

        private void Awake()
        {
            Actor = GetComponent<Actor>();
            CurrentHealth = MaxHealth;
            lastUpdateTime = Time.time;
        }

        void Start()
        {
            Actor.ControllerBase.onRespawn += OnRespawn;
        }

        private void Update()
        {
            if (CurrentHealth < MaxHealth && lastUpdateTime + (regeneratDelay * boostDelay) < Time.time)
            {
                lastUpdateTime = Time.time;
                float resultHealth = CurrentHealth + (healthRegenerationRate * boostAmount);
                CurrentHealth = resultHealth > MaxHealth ? MaxHealth : resultHealth;
                if (IsDead && CurrentHealth >= MaxHealth)
                {
                    onFullHealthAfterDeath?.Invoke();
                }
            }
        }

        public void BoostHeal(float newBoostDelay, float newBoostAmount)
        {
            bool isNull = newBoostDelay * newBoostAmount == 0;
            boostDelay = isNull ? 1f : newBoostDelay;
            boostAmount = isNull ? 1f : newBoostAmount;
        }
        
        public void Heal(float healAmount)
        {
            float healthBefore = CurrentHealth;
            CurrentHealth += healAmount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            // call OnHeal action
            float trueHealAmount = CurrentHealth - healthBefore;
            if (trueHealAmount > 0f)
            {
                onHealed?.Invoke(trueHealAmount);
            }
        }

        public void InflictDamage(float damage, bool isExplosionDamage, Actor hitActor)
        {
            var totalDamage = damage;

            // skip the crit multiplier if it's from an explosion
            if (!isExplosionDamage)
            {
                totalDamage *= DamageMultiplier;
            }

            // potentially reduce damages if inflicted by self
            if (Actor == hitActor)
            {
                totalDamage *= SensibilityToSelfdamage;
            }

            // apply the damages
            TakeDamage(totalDamage, hitActor);
        }

        public bool TakeDamage(float damage, Actor hitActor)
        {
            if (invulnerable) damage = 0.01f;
            float healthBefore = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            // call OnDamage action
            float trueDamageAmount = healthBefore - CurrentHealth;
            if (trueDamageAmount > 0f && hitActor)
            {
                lastHitActor = hitActor;
                onDamaged?.Invoke(trueDamageAmount, hitActor);
            }

            return HandleDeath();
        }

        public void Kill()
        {
            IsInstantDead = true;
            CurrentHealth = 0f;

            //call OnDamage action
            onDamaged?.Invoke(MaxHealth, null);
            HandleDeath();
        }

        bool HandleDeath()
        {
            // call OnDie action
            if (!IsDead && CurrentHealth <= 0f)
            {
                IsDead = true;
                invulnerable = true;

                DieEvent ke = Events.DieEvent;
                ke.Actor = Actor;
                ke.LastHitActor = lastHitActor;
                EventManager.Broadcast(ke);

                onDie?.Invoke();
            }
            return IsDead;
        }

        private void OnRespawn()
        {
            CurrentHealth = MaxHealth;
            IsInstantDead = false;
            IsDead = false;
            BoostHeal(0, 0);
            Invoke(nameof(DisableInvulnerable),DelayInvulnerable);
        }
    }
}