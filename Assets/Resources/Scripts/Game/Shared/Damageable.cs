using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Platinum.Info
{
    public enum BodyLimb
    {
        Hand,
        Leg,
        Head,
        Spine,
    }

    [Serializable]
    public struct DollBodyLimb
    {
        public string Name;
        public BodyLimb BodyLimb;
        public Collider Limb;
        public float DamageMultiplier;
    }

    public class Damageable : MonoBehaviour
    {
        [Tooltip("Multiplier to apply to the received damage")]
        public float DamageMultiplier = 1f;

        [Range(0, 1)]
        [Tooltip("Multiplier to apply to self damage")]
        public float SensibilityToSelfdamage = 0.5f;

        public DollBodyLimb[] DollBodyLimbList;

        public Health Health { get; private set; }
        public Actor Actor { get; private set; }

        void Awake()
        {
            Actor = GetComponent<Actor>();
            Health = GetComponent<Health>();
            /*for (int i = 0; i < DollBodyLimbList.Length; i++)
            {
                SelfColliders.Add(DollBodyLimbList[i].Limb); 
            }*/
        }

        public bool InflictDamage(float damage, bool isExplosionDamage, Actor hitActor, Collider hit)
        {
            float currentDamageMultiplier = DamageMultiplier + DollBodyLimbList.Where(d => d.Limb == hit).FirstOrDefault().DamageMultiplier;
            if (Health)
            {
                var totalDamage = damage;

                // skip the crit multiplier if it's from an explosion
                if (!isExplosionDamage)
                {
                    totalDamage *= currentDamageMultiplier;
                }

                // potentially reduce damages if inflicted by self
                if (Health.Actor == hitActor)
                {
                    totalDamage *= SensibilityToSelfdamage;
                }

                // apply the damages
                return Health.TakeDamage(totalDamage, hitActor);
            }
            return false;
        }

        public bool AreaInflictDamage(float damage, bool isExplosionDamage, Actor hitActor)
        {
            if (Health)
            {
                var totalDamage = damage;

                // skip the crit multiplier if it's from an explosion
                if (!isExplosionDamage)
                {
                    totalDamage *= DamageMultiplier;
                }

                // potentially reduce damages if inflicted by self
                if (Health.Actor == hitActor)
                {
                    totalDamage *= SensibilityToSelfdamage;
                }

                // apply the damages
                return Health.TakeDamage(totalDamage, hitActor);
            }
            return false;
        }
    }
}