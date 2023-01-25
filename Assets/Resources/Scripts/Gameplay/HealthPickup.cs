using UnityEngine;
using Platinum.Controller;
using Platinum.Info;

namespace Platinum.Weapon
{
    public class HealthPickup : Pickup
    {
        [Header("Parameters")]
        [Tooltip("Amount of health to heal on pickup")]
        public float HealAmount;

        protected override void OnPicked(ControllerBase controllerc)
        {
            Health playerHealth = controllerc.Health;
            if (playerHealth && playerHealth.CanPickup())
            {
                playerHealth.Heal(HealAmount);
                PlayPickupFeedback();
                Destroy(gameObject);
            }
        }
    }
}