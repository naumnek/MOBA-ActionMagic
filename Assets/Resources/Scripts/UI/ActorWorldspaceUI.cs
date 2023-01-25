using System;
using TMPro;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;
using Platinum.Controller;
using Platinum.CustomInput;
using Platinum.Info;
using Platinum.CustomEvents;
using Platinum.Weapon;
using System.Linq;

namespace Platinum.UI
{
    public class ActorWorldspaceUI : MonoBehaviour
    {
        [Header("General")]
        public bool DisableInfoBar;
        public bool HideFullHealthBar;
        public Transform BodyWorldspaceUI;
        
        [Header("Place References")]
        public GameObject RequredCircleCanvas;
        public GameObject LookCanvas;
        public LookRect[] LookRect;
        
        [Header("General References")]
        public TMP_Text NickName;
        public GameObject TeamIcon;
        public Transform InfoBarPivot;
        [Header("Health References")]
        public GameObject HealthBarPivot;
        public Image HealthBarFillImage;
        [Header("Mana References")]
        public GameObject ManaBarPivot;
        public Image ManaBarFillImage;

        private ControllerBase m_ControllerOwner;
        private ControllerBase m_ControllerPlayer;
        private Transform m_MainCamera;
        private MoveKeycode m_MoveKeycode;
        
        private bool controllable;
        private int Affiliation = 0;
        private int PlayerAffiliation = 0;
        private LookRect currentLookRect;
        private bool activation;

        private void Awake()
        {
            EventManager.AddListener<EndSpawnEvent>(OnPlayerSpawnEvent);
            
            for (int i = 0; i < LookRect.Length; i++)
            {
                LookRect[i].transform.gameObject.SetActive(false);
                LookRect[i].startRotation = LookRect[i].transform.rotation;
            }
            LookCanvas.SetActive(false);
            
            m_ControllerOwner = GetComponentInParent<ControllerBase>();
            m_ControllerOwner.onSwitchedToWeapon += OnSwitchWeapon;
            m_ControllerOwner.onDie += OnDie;
            m_ControllerOwner.onRespawn += OnRespawn;
            
            if (!BodyWorldspaceUI) BodyWorldspaceUI = transform;
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<EndSpawnEvent>(OnPlayerSpawnEvent);
        }

        private void OnPlayerSpawnEvent(EndSpawnEvent evt)
        {
            m_MainCamera = Camera.main.transform;

            m_ControllerPlayer = evt.LoadManager.ControllerPlayer;
            PlayerAffiliation = m_ControllerPlayer.Actor.Info.Affiliation.Number;
            
            //isOwnerActor = m_ControllerPlayer.Actor.Info.ActorID == m_ControllerOwner.Actor.Info.ActorID;
            Affiliation = m_ControllerOwner.Actor.Info.Affiliation.Number;
            
            NickName.text = m_ControllerOwner.Actor.Info.PlayerInfo.Nickname;

            Color UIColor = PlayerAffiliation == Affiliation ? Color.green : Color.red;
            UIColor.a = HealthBarFillImage.color.a;

            HealthBarFillImage.color = UIColor;
            //NickName.color = UIColor;
            RequredCircleCanvas.SetActive(m_ControllerOwner.isUser);
            InfoBarPivot.gameObject.SetActive(!m_ControllerOwner.isUser && !DisableInfoBar);
        }


        void Update()
        {
            m_MoveKeycode = PlayerInputHandler.MoveKeycode;
            if (activation)
            {
                UpdateHealthBar(m_ControllerOwner.Health);
                UpdateManaBar(m_ControllerOwner.Arsenal.activeWeapon.activeBullet);
                // rotate health bar to face the camera/player
                InfoBarPivot.LookAt(m_MainCamera);
                //Debug.Log(m_ControllerBase.Actor.name);
                BodyWorldspaceUI.position = m_ControllerOwner.Movement.Body.position;
            }
            
            if (m_ControllerOwner.isUser)
            {
                Vector2 lookInput = m_MoveKeycode.look;
                bool active = lookInput.magnitude > 1f;
                
                Vector3 imagePos = currentLookRect.transform.position;

                LookCanvas.SetActive(active);
                float yawMove = Mathf.Atan2(lookInput.x, lookInput.y) * Mathf.Rad2Deg;
                Quaternion newRotation =  Quaternion.Euler(0,0,yawMove);
                currentLookRect.transform.rotation = currentLookRect.startRotation * newRotation;
            }
        }
        private void UpdateHealthBar(Health health)
        {
            // update health bar value
            HealthBarFillImage.fillAmount = health.Ratio;
            // hide health bar if needed
            if (HideFullHealthBar)
            {
                HealthBarPivot.gameObject.SetActive(health.isFullHealth);
            }
        }

        private void UpdateManaBar(BulletBase bullet)
        {
            // update health bar value
            ManaBarFillImage.fillAmount = bullet.ammoRatio;
        }

        private void OnSwitchWeapon(WeaponController weapon)
        {
            for (int i = 0; i < LookRect.Length; i++)
            {
                bool active = LookRect[i].Type == weapon.LookType;
                LookRect[i].transform.gameObject.SetActive(active);
                currentLookRect = active ? LookRect[i] : currentLookRect;
            }
        }
        
        void OnRespawn()
        {
            BodyWorldspaceUI.gameObject.SetActive(true);
            activation = true;
        }
        
        void OnDie()
        {
            BodyWorldspaceUI.gameObject.SetActive(false); 
            activation = false;
        }
    }
    
    [Serializable]
    public struct LookRect
    {
        public RectTransform transform;
        public WeaponLookType Type;
        [HideInInspector]
        public Quaternion startRotation;
    }
}