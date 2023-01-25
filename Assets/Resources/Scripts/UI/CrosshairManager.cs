using Platinum.Controller;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;
using Platinum.Scene;
using Platinum.Weapon;

[System.Serializable]
public struct CrosshairData
{
    [Tooltip("The image that will be used for this weapon's crosshair")]
    public Sprite CrosshairSprite;

    [Tooltip("The size of the crosshair image")]
    public int CrosshairSize;

    [Tooltip("The color of the crosshair image")]
    public Color CrosshairColor;
}
namespace Platinum.UI
{
    public class CrosshairManager : MonoBehaviour
    {
        /*
        public Image CrosshairImage;
        public Sprite NullCrosshairSprite;
        public float CrosshairUpdateshrpness = 5f;

        bool m_WasPointingAtEnemy;
        bool m_WasHitEnemy;
        RectTransform m_CrosshairRectTransform;
        CrosshairData m_CrosshairDataDefault;
        CrosshairData m_CrosshairDataTarget;
        CrosshairData m_CrosshairDataHitTarget;

        CrosshairData m_CurrentCrosshair;
        Transform m_WeaponCamera;
        private bool ServerPause = true;
        private Arsenal m_Arsenal;
        private PlayerController m_PlayerController;

        private void Start()
        {
            LoadManager loadManager = LoadManager.Instance;
            m_PlayerController = loadManager.PlayerController;
            m_Arsenal = m_PlayerController.Arsenal;

            OnWeaponChanged(m_Arsenal.ActiveWeapon);

            m_Arsenal.OnSwitchedToWeapon += OnWeaponChanged;
            m_WeaponCamera = loadManager.ThirdPersonController.MainCamera.transform;

            ServerPause = false;
        }

        void Update()
        {
            if (ServerPause) return;
            UpdateCrosshairPointingAtEnemy(false);
            m_WasPointingAtEnemy = m_PlayerController.IsPointingAtEnemy;
            m_WasHitEnemy = m_Arsenal.IsHitEnemy;
        }

        bool PointingAtEnemy => m_PlayerController.IsPointingAtEnemy;
        bool HitEnemy => m_Arsenal.IsHitEnemy;

        void UpdateCrosshairPointingAtEnemy(bool force)
        {
            if (m_CrosshairDataDefault.CrosshairSprite == null) return;

            if (force || m_WasHitEnemy)
            {
                m_Arsenal.ResetHitEnemy();
                SetCrosshair(m_CrosshairDataHitTarget);
            }
            if ((force || !m_WasPointingAtEnemy) && PointingAtEnemy && !m_WasHitEnemy)
            {
                SetCrosshair(m_CrosshairDataTarget);
            }
            else if ((force || m_WasPointingAtEnemy || m_WasHitEnemy) && !PointingAtEnemy && !HitEnemy)
            {
                SetCrosshair(m_CrosshairDataDefault);
            }

            CrosshairImage.color = Color.Lerp(CrosshairImage.color, m_CurrentCrosshair.CrosshairColor,
                Time.deltaTime * CrosshairUpdateshrpness);

            m_CrosshairRectTransform.sizeDelta = Mathf.Lerp(m_CrosshairRectTransform.sizeDelta.x,
                m_CurrentCrosshair.CrosshairSize,
                Time.deltaTime * CrosshairUpdateshrpness) * Vector2.one;
        }

        private void SetCrosshair(CrosshairData crosshair)
        {
            m_CurrentCrosshair = crosshair;
            CrosshairImage.sprite = m_CurrentCrosshair.CrosshairSprite;
            m_CrosshairRectTransform.sizeDelta = m_CurrentCrosshair.CrosshairSize * Vector2.one;
        }

        void OnWeaponChanged(WeaponController newWeapon)
        {
            if (newWeapon)
            {
                CrosshairImage.enabled = true;
                m_CrosshairDataDefault = newWeapon.CrosshairDataDefault;
                m_CrosshairDataTarget = newWeapon.CrosshairDataTargetInSight;
                m_CrosshairDataHitTarget = newWeapon.CrosshairDataHitTarget;
                m_CrosshairRectTransform = CrosshairImage.GetComponent<RectTransform>();
                DebugUtility.HandleErrorIfNullGetComponent<RectTransform, CrosshairManager>(m_CrosshairRectTransform,
                    this, CrosshairImage.gameObject);
            }
            else
            {
                if (NullCrosshairSprite)
                {
                    CrosshairImage.sprite = NullCrosshairSprite;
                }
                else
                {
                    CrosshairImage.enabled = false;
                }
            }

            UpdateCrosshairPointingAtEnemy(true);
        }
        */
    }
}