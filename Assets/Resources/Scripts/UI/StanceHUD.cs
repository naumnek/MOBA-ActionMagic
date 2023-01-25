using Unity.FPS.Game;
using Platinum.Controller;
using UnityEngine;
using UnityEngine.UI;
using Platinum.CustomInput;

namespace Platinum.UI
{
    public class StanceHUD : MonoBehaviour
    {
        [Tooltip("Image component for the stance sprites")]
        public Image StanceImage;

        [Tooltip("Sprite to display when standing")]
        public Sprite StandingSprite;

        [Tooltip("Sprite to display when crouching")]
        public Sprite CrouchingSprite;

        void Update()
        {
            OnStanceChanged(PlayerInputHandler.MoveKeycode.MoveState == MoveState.Crouch);
        }

        void OnStanceChanged(bool crouched)
        {
            StanceImage.sprite = crouched ? CrouchingSprite : StandingSprite;
        }
    }
}