using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Platinum.Info;

namespace Platinum.Weapon
{
    public abstract class ProjectileBase : MonoBehaviour
    {
        [Header("General")]
        public bool IgnoryPlayerSaves = true;
        //public bool ForwardMove = false;

        [Header("References")]
        [Tooltip("Area of damage. Keep empty if you don<t want area damage")]
        public DamageArea AreaOfDamage;
        public ImpactProjectileFX ImpactFXPrefab;
        
        [Header("Internal References")]
        [Tooltip("Transform representing the root of the projectile (used for accurate collision detection)")]
        public Transform Root;
        [Tooltip("Transform representing the tip of the projectile (used for accurate collision detection)")]
        public Transform Tip;

        [Header("Parameters")] 
        public float CastTime = 0.5f;
        public float Damage = 500f;
        public float Radius = 1f;
        public float RadiusAfterHit = 2f;
        public float MaxShotDistance = 15f;
        public float MaxLifeTime = 4f;
        public float onHitLifeTime = 1f;
        public LayerMask HittableLayers = -1;
        public float Speed = 10f;
        [Tooltip("Downward acceleration from gravity")]
        public float GravityDownAcceleration = 0f;

        [Tooltip("Determines if the projectile inherits the velocity that the weapon's muzzle had when firing")]
        public bool InheritWeaponVelocity = false;
        
        [Header("Debug")]
        [Tooltip("Color of the projectile radius debug view")]
        public Color RadiusColor = Color.red * 0.2f;

        public WeaponController ParentWeapon { get; private set; }
        public Actor Owner { get; private set; }

        public Vector3 InitialPosition { get; private set; }
        public Vector3 InitialDirection { get; private set; }
        public Vector3 InheritedMuzzleVelocity { get; private set; }

        public Vector3 TargetPosition { get; private set; }

        protected ProjectileBase m_ProjectileBase;
        protected List<Collider> m_IgnoredColliders;
        protected bool activate;

        public void Shoot(WeaponController weapon, Vector3 targetPosition)
        {
            if (ParentWeapon) return;
            TargetPosition = targetPosition;
            ParentWeapon = weapon;
            Owner = weapon.ControllerBase.Actor;
            InitialPosition = transform.position;
            InitialDirection = transform.forward;
            InheritedMuzzleVelocity = weapon.MuzzleWorldVelocity;

            //HittableLayers -= m_ProjectileBase.ParentWeapon.ControllerBase.Damageable.SelfLayers;

            //m_ShootTime = Time.time;
            m_IgnoredColliders = new List<Collider>();
            transform.position += InheritedMuzzleVelocity * Time.deltaTime;

            // Ignore colliders of owner
            m_IgnoredColliders.AddRange(m_ProjectileBase.gameObject.
                GetComponentsInChildren<Collider>());

            m_IgnoredColliders.AddRange(m_ProjectileBase.ParentWeapon.ControllerBase.CharacterAvatar.SelfColliders);

            if (weapon.skill) {
                
                Invoke(nameof(shoot),CastTime);
            }
            else
            {
                shoot();
            }
            //shoot();
        }

        private void shoot()
        {
            activate = true;
        }
    }
}