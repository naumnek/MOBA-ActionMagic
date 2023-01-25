using Platinum.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Platinum.CustomEvents;
using Platinum.Info;
using Platinum.Weapon;

namespace Platinum.UI
{
    public class PlayerBarManager : MonoBehaviour
    {
        [Header("Info")] 
        public TMP_Text NicknameText;
        
        [Header("Health")]
        public TMP_Text HealthAmountText;
        public Image HealthFillImage;
        public Color currentHealthColor;
        public Color maxHealthColor;
        
        [Header("Mana")]
        public TMP_Text ManaAmountText;
        public Image ManaFillImage;
        public Color currentManaColor;
        public Color maxManaColor;

        private ControllerBase controllerBase;
        private bool Pause = true;

        private void OnDestroy()
        {
            EventManager.RemoveListener<EndSpawnEvent>(OnPlayerSpawnEvent);
            EventManager.RemoveListener<GamePauseEvent>(OnGamePauseEvent);
        }
        private void Awake()
        {
            EventManager.AddListener<GamePauseEvent>(OnGamePauseEvent);
            EventManager.AddListener<EndSpawnEvent>(OnPlayerSpawnEvent);
        }

        private void OnGamePauseEvent(GamePauseEvent evt)
        {
            Pause = evt.ServerPause;
        }

        private void OnPlayerSpawnEvent(EndSpawnEvent evt)
        {
            controllerBase = evt.LoadManager.ControllerPlayer;
            NicknameText.text = controllerBase.Actor.Info.PlayerInfo.Nickname;
            
            Pause = false;
        }

        private void UpdateManaBar(BulletBase bullet)
        {
            if (!ManaFillImage) return;
            string color1 = ColorTypeConverter.ToRGBHex(currentManaColor);
            string color2 =ColorTypeConverter.ToRGBHex(maxManaColor);
            string manaAmount = color1 + bullet.currentBullets.ToString().Split(',')[0]; 
            manaAmount += " / " + color2 + bullet.maxBullets.ToString();
            // update health bar value
            ManaFillImage.fillAmount = bullet.ammoRatio;
            ManaAmountText.text = manaAmount;
        }
        private void UpdateHealthBar(Health health)
        {
            if (!HealthFillImage) return;
            string color1 = ColorTypeConverter.ToRGBHex(currentHealthColor);
            string color2 =ColorTypeConverter.ToRGBHex(maxHealthColor);
            string healthAmount = color1 + health.CurrentHealth.ToString().Split(',')[0] + " / ";
            healthAmount += color2 + health.MaxHealth.ToString();
            // update health bar value
            HealthFillImage.fillAmount = health.Ratio;
            HealthAmountText.text = healthAmount;
        }
        
        void Update()
        {
            if (!Pause && controllerBase)
            {
                if (HealthFillImage) UpdateHealthBar(controllerBase.Health);
                if (ManaFillImage) UpdateManaBar(controllerBase.Arsenal.activeWeapon.activeBullet);
            }
        }
    }
}