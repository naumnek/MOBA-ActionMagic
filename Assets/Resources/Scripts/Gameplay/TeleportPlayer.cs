using Unity.FPS.Game;
using UnityEngine;
using Platinum.Info;

namespace Platinum.Controller
{
    // Debug script, teleports the player across the map for faster testing
    public class TeleportPlayer : MonoBehaviour
    {
        public KeyCode ActivateKey = KeyCode.F12;

        ControllerPlayer _mControllerPlayer;

        void Awake()
        {
            _mControllerPlayer = FindObjectOfType<ControllerPlayer>();
            DebugUtility.HandleErrorIfNullFindObject<CharacterAvatar, TeleportPlayer>(
                _mControllerPlayer, this);
        }

        void Update()
        {
            if (Input.GetKeyDown(ActivateKey))
            {
                _mControllerPlayer.Movement.Body.SetPositionAndRotation(transform.position, transform.rotation);
                Health playerHealth = _mControllerPlayer.Health;
                if (playerHealth)
                {
                    playerHealth.Heal(999);
                }
            }
        }

    }
}