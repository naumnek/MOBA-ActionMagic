using Platinum.Settings;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Platinum.UI
{
    public class ItemListSwitch : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("General")]
        public TMP_Text ItemNameText;
        public Image ItemIcon;
        public Image TypeItemIcon;
        public float MultiplierItemIcon = 0.5f;
        public Button ItemChooseButton;
        public Sprite InactiveSpriteButton;
        public GameObject BlackMask;
        public GameObject ImagePaid;
        public GameObject ImageBlocked;
        public GameObject Highlighted;

        [Header("Attributes")]
        public UIAttribute MaxBullets;
        public UIAttribute Damage;
        public UIAttribute BulletSpeed;
        public UIAttribute BulletSpreadAngle;
        public UIAttribute BulletsPerShoot;
        public float SpeedValueBar = 0.5f;
        List<UIAttribute> WeaponsAttribute;

        private Image ChooseButtonImage;
        private Sprite DefaultSpriteButton;
        private string roomName;
        private WeaponItem _mWeaponItem;
        private SwitchItemMenu m_ItemMenu;
        private float time = 0f;
        private bool IsSetValue;
        private bool HasActive;
        private bool IsItemClick;

        public void OnPointerExit(PointerEventData eventData)
        {
            Highlighted.SetActive(false);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsItemClick) Highlighted.SetActive(true);

            for (int i = 0; i < WeaponsAttribute.Count; i++)
            {
                time = 0;
                while (time < 1)
                {
                    time += Time.deltaTime * SpeedValueBar;
                    WeaponsAttribute[i].LoadingBar.value = time * WeaponsAttribute[i].Value;
                    /*if (WeaponsAttribute.All(w => w.LoadingBar.value >= w.Value))
                    {
                        IsSetValue = false;
                    }*/
                }
            }
        }

        public void OnItemClicked()
        {
            IsItemClick = true;
            Highlighted.SetActive(false);
            m_ItemMenu.GiveItemPlayer(_mWeaponItem, this);
        }

        public void Initialize(WeaponItem weaponItem, SwitchItemMenu ItemMenu, bool isActive)
        {
            HasActive = isActive && !weaponItem.IsPaid && !weaponItem.IsBlocked;

            BlackMask.SetActive(!HasActive);
            ImageBlocked.SetActive(weaponItem.IsBlocked);
            ImagePaid.SetActive(weaponItem.IsPaid);

            WeaponsAttribute = new List<UIAttribute>
            { MaxBullets, Damage, BulletSpeed, BulletSpreadAngle, BulletsPerShoot};

            ChooseButtonImage = ItemChooseButton.GetComponent<Image>();
            DefaultSpriteButton = ChooseButtonImage.sprite;

            ItemChooseButton.interactable = HasActive;
            ChooseButtonImage.sprite =
                HasActive ?
                DefaultSpriteButton :
                InactiveSpriteButton;

            roomName = weaponItem.Name();
            _mWeaponItem = weaponItem;
            m_ItemMenu = ItemMenu;

            ItemNameText.text = weaponItem.Name();
            Sprite SpriteItemIcon = weaponItem.weapon.WeaponIcon;
            ItemIcon.sprite = SpriteItemIcon;

            ItemIcon.GetComponent<RectTransform>().sizeDelta =
                new Vector2(SpriteItemIcon.rect.width * MultiplierItemIcon, SpriteItemIcon.rect.height * MultiplierItemIcon);

            MaxValueAttributes maxValueAttributes = ItemMenu.gameSettings.ItemSettings.MaxWeaponAttributes;

            MaxBullets.SetValue(weaponItem.Attributes.MaxBullets, maxValueAttributes.MaxBullets);
            Damage.SetValue(weaponItem.Attributes.Damage, maxValueAttributes.Damage);
            BulletSpeed.SetValue(weaponItem.Attributes.BulletSpeed, maxValueAttributes.BulletSpeed);
            BulletSpreadAngle.SetValue(weaponItem.Attributes.SpreadAngle, maxValueAttributes.BulletSpreadAngle);
            BulletsPerShoot.SetValue(weaponItem.Attributes.BulletsPerShoot, maxValueAttributes.BulletsPerShoot);
        }
    }
    [System.Serializable]
    public class UIAttribute
    {
        public Slider LoadingBar;
        public TMP_Text TextValue;
        public float Value { get; private set; }
        public bool FullLoadingBar;

        public void SetValue(float newValue, float maxValue)
        {
            LoadingBar.maxValue = maxValue;
            Value = newValue;
            TextValue.text = newValue.ToString();
        }
    }
}
