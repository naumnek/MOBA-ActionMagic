using UnityEngine;
using Platinum.CustomEvents;
using Platinum.CustomInput;
using Platinum.Info;

namespace Platinum.Controller
{
    public class ControllerPlayer : ControllerBase
    {
        public bool DisableReload;
        public bool IsPointingAtEnemy { get; private set; }
        private Vector2 wasLook;
        
        private void Awake()
        {
            isUser = true;
            base.Awake();
        }
        
        protected override void Update()
        {
            moveKeycode = PlayerInputHandler.MoveKeycode;
            weaponKeycode = PlayerInputHandler.WeaponKeycode;

            if (DisableReload) weaponKeycode.reload = false;
            if (weaponKeycode.shootDirection.magnitude == 0 && wasLook.magnitude > 1f && moveKeycode.look.magnitude == 0)
            {
                weaponKeycode.shootDirection = wasLook;
                //moveKeycode.RotateInMove = true;
                wasLook = Vector2.zero;
            }
            
            wasLook = moveKeycode.look;
            
            base.Update();
        }

        private bool IsPointingEnemy(Collider hit)
        {
            return hit.GetComponentInParent<Health>();
        }
    }
}

