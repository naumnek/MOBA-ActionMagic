using Platinum.Controller;

namespace Platinum.Weapon
{
    public class JetpackPickup : Pickup
    {
        protected override void OnPicked(ControllerBase controller)
        {
            var jetpack = controller.GetComponent<Jetpack>();
            if (!jetpack)
                return;

            if (jetpack.TryUnlock())
            {
                PlayPickupFeedback();
                Destroy(gameObject);
            }
        }
    }
}