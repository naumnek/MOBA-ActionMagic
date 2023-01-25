using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBullet : BulletBase
{
        [Header("Ammo")]
        [Tooltip("Maximum amount of bullets in a clip")]
        public float maxAmmo = 30f;
        
        [Tooltip("Start number of bullets in a clip")]
        public float startAmmo = 30f;
        public float currentAmmo { get; private set; }
        public int GetCurrentAmmo() => Mathf.FloorToInt(currentAmmo);
        

        protected override void UpdateBullets()
        {
            // reloads weapon over time
            float resultAmmo = currentAmmo - bulletsReloadRate;
            if (resultAmmo >= 0)
            {
                currentAmmo = resultAmmo;
                base.UpdateBullets();;
            }
        }

        public override void SetCurrentAmmo(bool InfinityAmmo)
        {
            base.SetCurrentAmmo(InfinityAmmo);
            currentAmmo = startAmmo;
        }
}
