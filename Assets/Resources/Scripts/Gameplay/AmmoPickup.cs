using UnityEngine;
using Platinum.Controller;
using Platinum.CustomEvents;
using Platinum.Weapon;

namespace Unity.FPS.Gameplay
{
    public class AmmoPickup : Pickup
    {
        [Tooltip("Weapon those bullets are for")]
        public WeaponController weapon;

        [Tooltip("Number of bullets the player gets")]
        public int BulletCount = 10;

        protected override void OnPicked(ControllerBase controller)
        {
            Arsenal arsenal = controller.Arsenal;
            if (arsenal)
            {
                WeaponController weapon = arsenal.HasWeapon(this.weapon);

                if (weapon != null && weapon.activeBullet.currentBullets < weapon.activeBullet.maxBullets)
                {
                    weapon.activeBullet.AddCarriablePhysicalBullets(BulletCount);

                    AmmoPickupEvent evt = Events.AmmoPickupEvent;
                    evt.ItemID = weapon.ItemID;
                    EventManager.Broadcast(evt);

                    PlayPickupFeedback();
                    Destroy(gameObject);
                }
            }
        }
    }
}
