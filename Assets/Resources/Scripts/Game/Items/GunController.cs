using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

namespace Platinum.Weapon
{
    public class GunController : WeaponController
    {
        [Tooltip("The name that will be displayed in the UI for this weapon")]
        public WeaponType Type = WeaponType.Rifle;
        
        [Header("References")]
        public AnimationClip WeaponAnimation;
        
        [Header("Internal References")]
        [Tooltip("Tip of the weapon, where the projectiles are shot")]
        public List<Renderer> WeaponRenderer = new List<Renderer> { };
        public Rigidbody WeaponRigidbody;
        public Collider WeaponCollider;

        [Header("Shoot")]
        [Tooltip("Angle for the cone in which the bullets will be shot randomly (0 means no spread at all)")]
        public float BulletSpreadAngle = 0f;
        
        [Header("Shell")]
        [Tooltip("Bullet Shell Casing")]
        public GameObject ShellCasing;
        [Tooltip("Weapon Ejection Port for physical ammo")]
        public Transform EjectionPort;
        [Tooltip("Force applied on the shell")]
        [Range(0.0f, 5.0f)] public float ShellCasingEjectionForce = 2.0f;
        [Tooltip("Maximum number of shell that can be spawned before reuse")]
        [Range(1, 30)] public int ShellPoolSize = 1;
        
        [Header("Audio")]
        public bool UseContinuousShootSound = false;
        public AudioClip ContinuousShootStartSfx;
        public AudioClip ContinuousShootLoopSfx;
        public AudioClip ContinuousShootEndSfx;
        
        private Queue<Rigidbody> m_PhysicalAmmoPool;
        public bool IsCooling { get; private set; }
        AudioSource m_ContinuousShootAudioSource = null;
        public GunBullet ActiveGunBullet { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            ActiveGunBullet = activeBullet.GetComponent<GunBullet>();
        }
        
        private void Start()
        {
            if (ActiveGunBullet.HasPhysical)
            {
                m_PhysicalAmmoPool = new Queue<Rigidbody>(ShellPoolSize);

                for (int i = 0; i < ShellPoolSize; i++)
                {
                    GameObject shell = Instantiate(ShellCasing, transform);
                    shell.SetActive(false);
                    m_PhysicalAmmoPool.Enqueue(shell.GetComponent<Rigidbody>());
                }
            }

            if (UseContinuousShootSound)
            {
                m_ContinuousShootAudioSource = gameObject.AddComponent<AudioSource>();
                m_ContinuousShootAudioSource.playOnAwake = false;
                m_ContinuousShootAudioSource.clip = ContinuousShootLoopSfx;
                m_ContinuousShootAudioSource.outputAudioMixerGroup =
                    AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponShoot);
                m_ContinuousShootAudioSource.loop = true;
            }
        }

        protected override void Update()
        {
            base.Update();
            UpdateContinuousShootSound();
        }
        
        public override Vector3 GetShotDirectionWithinSpread(Vector3 shootDirection, int bulletNumber)
        {
            float spreadAngleRatio = BulletSpreadAngle / 180f;
            Vector3 spreadWorldDirection = Vector3.Slerp(shootDirection, UnityEngine.Random.insideUnitSphere,
                spreadAngleRatio);
            
            return spreadWorldDirection;
        }
        
        void UpdateContinuousShootSound()
        {
            if (UseContinuousShootSound)
            {
                if (m_WantsToShoot && ActiveGunBullet.currentBullets >= 1f)
                {
                    if (!m_ContinuousShootAudioSource.isPlaying)
                    {
                        ShootAudioSource.PlayOneShot(ShootSfx);
                        ShootAudioSource.PlayOneShot(ContinuousShootStartSfx);
                        m_ContinuousShootAudioSource.Play();
                    }
                }
                else if (m_ContinuousShootAudioSource.isPlaying)
                {
                    ShootAudioSource.PlayOneShot(ContinuousShootEndSfx);
                    m_ContinuousShootAudioSource.Stop();
                }
            }
        }
        
        protected override void ShootShell()
        {
            Rigidbody nextShell = m_PhysicalAmmoPool.Dequeue();

            nextShell.transform.position = EjectionPort.transform.position;
            nextShell.transform.rotation = EjectionPort.transform.rotation;
            nextShell.gameObject.SetActive(true);
            nextShell.transform.SetParent(null);
            nextShell.collisionDetectionMode = CollisionDetectionMode.Continuous;
            nextShell.AddForce(nextShell.transform.up * ShellCasingEjectionForce, ForceMode.Impulse);

            m_PhysicalAmmoPool.Enqueue(nextShell);
        }
        
        protected override void PlayShootSFX()
        {
            // play shoot SFX
            if (ShootSfx && !UseContinuousShootSound)
            {
                ShootAudioSource.PlayOneShot(ShootSfx);
            }
        }
        
        public void SetRigidbody(bool active)
        {
            WeaponCollider.enabled = active;
            WeaponRigidbody.isKinematic = !active;
        }
    }
    public enum WeaponType
    {
        Rifle,
        Pistol,
        ShotGun,
        MiniGun,
        Machine,
        MachineGun,
        Grenade,
        Rocket,
    }

    [System.Serializable]
    public struct CrosshairData
    {
        [Tooltip("The image that will be used for this weapon's crosshair")]
        public Sprite CrosshairSprite;

        [Tooltip("The size of the crosshair image")]
        public int CrosshairSize;

        [Tooltip("The color of the crosshair image")]
        public Color CrosshairColor;
    }
}