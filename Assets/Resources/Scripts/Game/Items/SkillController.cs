using System;
using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;
using Platinum.Settings;
using Platinum.Info;

namespace Platinum.Weapon
{

    [RequireComponent(typeof(AudioSource))]
    public class SkillController : WeaponController
    {
        [Header("Cast Parameters")]
        public float CastSpeed = 0.5f;
        public ManaBullet ActiveManaBullet { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            skill = this;
            ActiveManaBullet = activeBullet.GetComponent<ManaBullet>();
        }

        public override Vector3 GetShotDirectionWithinSpread(Vector3 shootDirection, int bulletNumber)
        {
            float angle = bulletNumber * AnglePerShoot;
            Quaternion rotate = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 spreadWorldDirection =  rotate * shootDirection;
            return spreadWorldDirection;
        }
        protected override void ShootShell(){}

        protected override void PlayShootSFX()
        {
            // play shoot SFX
            if (ShootSfx && ShootAudioSource)
            {
                ShootAudioSource.PlayOneShot(ShootSfx);
            }
        }
    }
}