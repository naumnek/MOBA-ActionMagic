using Platinum.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Platinum.Weapon;
using Platinum.CustomEvents;

namespace Platinum.UI
{
    public class AmmoCounter : MonoBehaviour
    {
        [Tooltip("CanvasGroup to fade the ammo UI")]
        public CanvasGroup CanvasGroup;
        public RectTransform Background;

        [Tooltip("Image for the weapon icon")]
        public Image WeaponImage;

        public Color WeaponDefaultColor;
        public Color WeaponReloadColor;

        [Tooltip("Text for Weapon index")]
        public TMP_Text WeaponIndexText;

        [Tooltip("Image for the weapon infinity ammo icon")]
        public Image CarriedInfinityAmmoIcon;

        [Tooltip("Text for Bullet Counter")]
        public TMP_Text CurrentBulletsCounter;

        [Tooltip("Text for Bullet Supple Counter")]
        public TMP_Text CarriedAmmoCounter;

        [Header("Selection")]
        [Range(0, 1)]
        [Tooltip("Opacity when weapon not selected")]
        public float UnselectedOpacity = 0.5f;

        [Tooltip("Scale when weapon not selected")]
        public Vector3 UnselectedScale;

        [Tooltip("Sharpness for the fill ratio movements")]
        public float AmmoFillMovementSharpness = 20f;

        public int WeaponCounterIndex { get; set; }

        Arsenal m_Asenal;
        GunController weapon;
        private Vector3 StartScale;
        private Vector2 m_DefaultSizeWeaponIcon;

        
        void OnAmmoPickup(AmmoPickupEvent evt)
        {
            if (evt.ItemID == weapon.ItemID)
            {
                CarriedAmmoCounter.text = weapon.activeBullet.GetCarriedAmmo().ToString();
            }
        }

        private void SetSizeIcon(RectTransform icon)
        {
            Vector2 iconSize = icon.sizeDelta;

            for (Vector2 constSize = iconSize; checkIcon(iconSize);)
            {
                iconSize -= constSize / 20f;
            }
            icon.sizeDelta = iconSize;

            RectTransform rectCounter = GetComponent<RectTransform>();
            float newWightCounter = rectCounter.sizeDelta.x - (m_DefaultSizeWeaponIcon.x - iconSize.x);
            float newHeightCounter = rectCounter.sizeDelta.y - (m_DefaultSizeWeaponIcon.y - iconSize.y);

            rectCounter.sizeDelta = new Vector2(newWightCounter, newHeightCounter);

        }

        private bool checkIcon(Vector2 iconSize) =>
            iconSize.x > m_DefaultSizeWeaponIcon.x * 1.1f || iconSize.y > m_DefaultSizeWeaponIcon.y * 1.1f;

        public void Initialize(WeaponController weapon, Arsenal arsenal)
        {
            GunController newWeapon = weapon.GetComponent<GunController>();
            EventManager.AddListener<AmmoPickupEvent>(OnAmmoPickup);
            m_Asenal = arsenal;
            m_DefaultSizeWeaponIcon = WeaponImage.rectTransform.sizeDelta;
            StartScale = transform.localScale;
            UnselectedScale = StartScale * 0.8f;

            weapon = newWeapon;
            WeaponCounterIndex = arsenal.ActiveWeaponIndex;

            Sprite weaponIcon = weapon.WeaponIcon;
            WeaponImage.sprite = weaponIcon;
            WeaponImage.SetNativeSize();
            SetSizeIcon(WeaponImage.rectTransform);

            if (newWeapon.InfinityAmmo)
            {
                CarriedAmmoCounter.gameObject.SetActive(false);
                CarriedInfinityAmmoIcon.gameObject.SetActive(true);
            }
            else
                CurrentBulletsCounter.text = newWeapon.ActiveGunBullet.GetCurrentBullets().ToString();
            CarriedAmmoCounter.text = newWeapon.activeBullet.GetCarriedAmmo().ToString();

            WeaponIndexText.text = (WeaponCounterIndex + 1).ToString();
        }

        void Update()
        {
            if (!m_Asenal) return;
            
            //AmmoFillImage.fillAmount = Mathf.Lerp(AmmoFillImage.fillAmount, currenFillRatio, Time.deltaTime * AmmoFillMovementSharpness);

            CurrentBulletsCounter.text = weapon.ActiveGunBullet.GetCurrentBullets().ToString();
            CarriedAmmoCounter.text = weapon.activeBullet.GetCarriedAmmo().ToString();

            bool isActiveWeapon = weapon == m_Asenal.activeWeapon;

            CanvasGroup.alpha = Mathf.Lerp(CanvasGroup.alpha, isActiveWeapon ? 1f : UnselectedOpacity,
                Time.deltaTime * 10);
            transform.localScale = Vector3.Lerp(transform.localScale, isActiveWeapon ? StartScale : UnselectedScale,
                Time.deltaTime * 10);

            WeaponImage.color =
                weapon.activeBullet.GetCarriedAmmo() > 0 && weapon.ActiveGunBullet.GetCurrentBullets() == 0 && weapon.IsWeaponActive ?
                WeaponReloadColor : WeaponDefaultColor;
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<AmmoPickupEvent>(OnAmmoPickup);
        }
    }
}