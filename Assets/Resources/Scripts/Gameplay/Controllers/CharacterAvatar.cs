using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Platinum.Settings;
using Platinum.Info;
using Platinum.CustomEvents;
using UnityEngine.Events;

namespace Platinum.Controller
{
    [Serializable]
    public class UnityAnimationEvent : UnityEvent<string>{};
    public class CharacterAvatar : MonoBehaviour
    {
        [Header("General Settings")]
        public bool DisableCustomization;
        public bool InstantDie;

        [Header("General References")]
        public Transform SkeletonHips;
        public SkinnedMeshRenderer CharacterSkinned;
        public Collider[] RagdollColliders;
        public Rigidbody[] RagdollRigidbodys;

        [Header("Die Effect")] 
        public AnimationClip deathAnimation;
        public float sinkingSpeed = 10f;
        public float immersionDepth = 3f;
        public float delayStartDieEffect = 3f;
        public AnimationCurve fadeIn;

        [Header("Sounds")]
        [Tooltip("Sound played when recieving damages")]
        public AudioClip DamageTick;

        [Header("VFX")]
        [Tooltip("The VFX prefab spawned when the enemy dies")]
        public GameObject DeathVfxPrefab;

        [Tooltip("The point at which the death VFX is spawned")]
        public Transform DeathVfxSpawnPoint;

        public AudioSource AudioSource { get; private set; }
        public AnimationEventDispatcher AnimationEventDispatcher { get; private set; }
        private Animator Animator;

        private GameSettings gameSettings;
        private ActorsManager m_ActorsManager;
        private ControllerBase m_ControllerBase;
        public UnityAction onEndDieEffect;

        protected Vector3 defaultHipsPosition;
        private PlayerSaves m_PlayerSaves;
        private bool m_WasDamagedThisFrame;
        private bool startDieEffect;
        private Transform body;
        private Vector3 positionPlaceDeath;
        public List<Collider> SelfColliders { get; private set; }
        
        private void OnDestroy()
        {
            EventManager.RemoveListener<EndSpawnEvent>(OnEndSpawnEvent);
        }
        

        // Start is called before the first frame update
        private void Awake()
        {
            EventManager.AddListener<EndSpawnEvent>(OnEndSpawnEvent);
            
            SelfColliders = new List<Collider>();
            SelfColliders.AddRange(GetComponentsInChildren<Collider>());

            AudioSource = GetComponentInChildren<AudioSource>();
            AnimationEventDispatcher = GetComponentInChildren<AnimationEventDispatcher>();
            m_ControllerBase = GetComponent<ControllerBase>();
            
            defaultHipsPosition = SkeletonHips.position;
            
            m_ControllerBase.onDie += OnDie;
            m_ControllerBase.onDamaged += OnDamaged;
            m_ControllerBase.onRespawn += OnRespawn;
        }

        private void Start()
        {
            body = m_ControllerBase.Movement.Body;
            Animator = m_ControllerBase.Animator;

            if (RagdollRigidbodys.Length > 0)
            {
                for (int i = 0; i < RagdollRigidbodys.Length; i++)
                {
                    RagdollRigidbodys[i].isKinematic = true;
                }
            }
        }

        private void Update()
        {
            m_WasDamagedThisFrame = false;
            if (startDieEffect) DieEffect();
        }

        private void StartDieEffect()
        {
            if (DeathVfxPrefab)
            {
                var vfx = Instantiate(DeathVfxPrefab, DeathVfxSpawnPoint.position, Quaternion.identity);
                Destroy(vfx, delayStartDieEffect);
            }
            positionPlaceDeath = body.position;
            startDieEffect = true;
        }
        
        private void DieEffect()
        {
            if (positionPlaceDeath.y - body.position.y < immersionDepth)
            {
                Vector3 deltaPosition = Vector3.zero;
                deltaPosition -= body.up;
                body.position += deltaPosition * sinkingSpeed * Time.deltaTime;
            }
            else
            {
                Animator.SetTrigger("endDeath");
                EndDieEffect();
            }
        }

        private void EndDieEffect()
        {
            startDieEffect = false;
            CharacterSkinned.gameObject.SetActive(false);
            onEndDieEffect?.Invoke();;
        }
        
        public void UpdateSkin(Material[] skin)
        {
            if (!DisableCustomization)
            {
                CharacterSkinned.materials = skin;
            }
        }

        void OnDamaged(float damage, Actor hitActor)
        {
            // play the damage tick sound
            if (AudioSource && DamageTick && !m_WasDamagedThisFrame)
            {
                //AudioSource.PlayOneShot(DamageTick);
            }

            m_WasDamagedThisFrame = true;
        }

        private void OnDie()
        {
            foreach (Collider selfCollider in SelfColliders)
            {
                selfCollider.enabled = false;
            }
            // spawn a particle system when dying
            if (DeathVfxPrefab)
            {
            }

            for (int i = 0; i < RagdollRigidbodys.Length; i++)
            {
                RagdollRigidbodys[i].isKinematic = false;
            }
            
            if (InstantDie)
            {
                EndDieEffect();
            }
            else
            {
                //CharacterSkinned.material = DissolveMaterial;
                Animator.Play(deathAnimation.name);
                Invoke(nameof(StartDieEffect),delayStartDieEffect);
            }
        }
        
        public void OnRespawn()
        {
            foreach (Collider selfCollider in SelfColliders)
            {
                selfCollider.enabled = true;
            }
            if (RagdollRigidbodys.Length > 0)
            {
                for (int i = 0; i < RagdollRigidbodys.Length; i++)
                {
                    RagdollRigidbodys[i].isKinematic = true;
                }
            }
            
            CharacterSkinned.gameObject.SetActive(true);
        }
        
        private void OnEndSpawnEvent(EndSpawnEvent evt)
        {
            gameSettings = evt.LoadManager.gameSettings;
            m_PlayerSaves = gameSettings.PlayerSettings.PlayerSaves;
            CustomizationSettings customization = gameSettings.CustomizationSettings;
            Mesh model = customization.GetRandomModel();
            Material[] materials = customization.GetRandomSkin().Materials;

            if (m_ControllerBase.isUser && !DisableCustomization)
            {
                model = customization.GetModel(customization.SkinType, customization.GetCurrentIndexModel());
                materials = customization.GetSkin(customization.SkinType, m_PlayerSaves.Skin).Materials;
            }
            SetSkin(model, materials);
        }

        private void SetSkin(Mesh model, Material[] materials)
        {
            CharacterSkinned.sharedMesh = model;
            CharacterSkinned.materials = materials;
        }
    }
}
