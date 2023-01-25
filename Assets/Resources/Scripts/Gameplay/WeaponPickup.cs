using Platinum.Controller;
using Unity.FPS.Game;
using UnityEngine;

namespace Platinum.Weapon
{
    public class WeaponPickup : Pickup
    {
        [Tooltip("The prefab for the weapon that will be added to the player on pickup")]
        public SkillController skillPrefab;

        protected override void Start()
        {
            base.Start();

            // Set all children layers to default (to prefent seeing weapons through meshes)
            foreach (Transform t in GetComponentsInChildren<Transform>())
            {
                if (t != transform)
                    t.gameObject.layer = 0;
            }
        }

        protected override void OnPicked(ControllerBase controller)
        {
            Arsenal arsenal = controller.Arsenal;
            if (arsenal)
            {
                if (arsenal.AddWeapon(skillPrefab.WeaponName))
                {
                    PlayPickupFeedback();
                    Destroy(gameObject);
                }
            }
        }
    }
}