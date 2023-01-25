using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Platinum.Controller;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;
using Platinum.Settings;
using Platinum.Info;

namespace Platinum.Weapon
{
    public abstract class WeaponController : MonoBehaviour
    {
        [Header("General")]
        public string WeaponName;
        public bool AutomaticReload = true;
        public bool InfinityAmmo;
        
        public WeaponLookType LookType;
        public WeaponShootType ShootType;
        
        [Header("References")]
        public AnimationClip AttackAnimation;
        public BulletBase bulletPrefab;
        
        [Header("Visual")]
        public ParticleSystem MuzzleFlash;
        
        [Header("Internal References")]
        [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
        public GameObject WeaponRoot;
        public Transform WeaponGunMuzzle;
        public Sprite WeaponIcon;

        [Header("Shoot Parameters")]
        public float DelayNonShootAfterEquep = 0.2f;
        public float StoppedMoveAfterShot = 0.5f;
        public float DelayBetweenShots = 0.5f;
        public int BulletsPerShot = 1;
        public int AmmoPerShot = 1;
        public float AnglePerShoot = 20f;
        
        [Header("Audio")]
        public AudioSource ShootAudioSource;
        public AudioClip ShootSfx;
        public AudioClip ChangeWeaponSfx;
        public AudioClip EmptyMagazinSfx;
        
        public ControllerBase ControllerBase { get; private set; }

        public float m_LastTimeShot { get; private set; } = Mathf.NegativeInfinity;
        protected bool m_WantsToShoot = false;
        public bool IsWeaponActive { get; private set; }
        public int ItemID { get; private set; }
        public Vector3 MuzzleWorldVelocity { get; private set; }
        public BulletBase activeBullet { get; private set; }
        public SkillController skill { get; protected set; }
        
        protected Vector3 m_LastMuzzlePosition;

        public UnityAction OnShoot;
        public UnityAction<int> OnHit;
        public event Action OnShootProcessed;
        private int arsenalSlot;

        protected virtual void Awake()
        {
            m_LastMuzzlePosition = WeaponGunMuzzle.position;
            activeBullet = Instantiate(bulletPrefab, Vector3.zero, Quaternion.identity, this.transform);
        }

        protected virtual void Update()
        {
            //ActiveBulletAmmo.Update();
            //UpdateCharge();
            //UpdateContinuousShootSound();

            if (Time.deltaTime > 0)
            {
                MuzzleWorldVelocity = (WeaponGunMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
                m_LastMuzzlePosition = WeaponGunMuzzle.position;
            }
        }
        
        public void HandleReloadWeapon(bool reload)
        {
            if (reload)
            {
                activeBullet.Reload();
            }
        }
        
        public void HitEnemy(int affiliation)
        {
            OnHit?.Invoke(affiliation);
        }
    
        public bool HandleShootTarget(Vector2 direction)
        {
            if (direction.magnitude == 0) return false;
                
            Vector3 shootDirection = new Vector3(direction.x,0,direction.y);
            
            m_WantsToShoot = true;

            switch (ShootType)
            {
                case WeaponShootType.Manual:
                    return TryShoot(shootDirection);

                case WeaponShootType.Automatic:
                    return TryShoot(shootDirection);

                default:
                    return false;
            }
        }
        
        public bool TryShoot(Vector3 target)
        {
            
            if (m_LastTimeShot + DelayBetweenShots < Time.time && (activeBullet.UseAmmo(AmmoPerShot) || InfinityAmmo))
            {
                if(AutomaticReload) activeBullet.Reload();
                HandleShoot(target);
                return true;
            }
            else
            {
                //if(ControllerBase.isUser)ShootAudioSource.PlayOneShot(EmptyMagazinSfx);
            }

            return false;
        }

        public void HandleShoot(Vector3 target)
        {
            m_LastTimeShot = Time.time;
            // spawn all bullets with random direction
            for (int i = 0; i <  BulletsPerShot; i++)
            {
                // Projectile Shoot
                Vector3 targetPosition = WeaponGunMuzzle.TransformDirection(target);
                Vector3 directionWithinSpread = GetShotDirectionWithinSpread(target, i);
                
                ProjectileBase newProjectile = Instantiate(bulletPrefab.projectilePrefab, WeaponGunMuzzle.position,
                    Quaternion.LookRotation(directionWithinSpread));
                newProjectile.transform.position = WeaponGunMuzzle.position;
                newProjectile.Shoot(this, targetPosition);
            }

            if (MuzzleFlash != null)
            {
                MuzzleFlash.Emit(1);
            }

            
            if (!InfinityAmmo)
            {
                if (activeBullet.HasPhysical) ShootShell();
                //m_CarriedAmmo--;
            }

            PlayShootSFX();

            m_LastTimeShot = Time.time;

            OnShoot?.Invoke();
            OnShootProcessed?.Invoke();
        }

        protected abstract void ShootShell();

        protected abstract void PlayShootSFX();
        
        //void PlaySFX(AudioClip sfx) => AudioUtility.CreateSFX(sfx, transform.position, AudioUtility.AudioGroups.WeaponShoot, 0.0f);
        
        public abstract Vector3 GetShotDirectionWithinSpread(Vector3 shootDirection, int bulletNumber);

        public void ShowWeapon(bool show, bool isFirstShow)
        {
            WeaponRoot.SetActive(show);

            if (arsenalSlot > 0 && show && ChangeWeaponSfx != null)
            {
                ShootAudioSource.PlayOneShot(ChangeWeaponSfx);
            }

            if (!show)
            {
                IsWeaponActive = false;
            }
            else
            {
                Invoke(nameof(WaitWeaponActive),DelayNonShootAfterEquep);
            }
        }

        private void WaitWeaponActive() => IsWeaponActive = true;
        
        public void SetOwner(ControllerBase controller, int slot)
        {
            arsenalSlot = slot;
            ControllerBase = controller;
            WeaponGunMuzzle = controller.Arsenal.WeaponPivot;
        }

        public void SetItemID(int newID)
        {
            ItemID = newID;
        }
    }
    
    
    public enum WeaponLookType
    {
        Line,
        Projector,
        Circle,
    }
    public enum WeaponShootType
    {
        Manual,
        Automatic,
    }
}
