using System;
using UnityEngine;
using UnityEngine.Events;
using Platinum.Info;
using Platinum.CustomEvents;
using Platinum.CustomInput;
using Platinum.Settings;
using Platinum.UI;
using Platinum.Weapon;

namespace Platinum.Controller
{
    public abstract class ControllerBase : MonoBehaviour
    {
        public bool isUser { get; protected set; }
        public Actor Actor { get; private set; }
        public Health Health { get; private set; }
        public Damageable Damageable { get; private set; }
        public Arsenal Arsenal { get; private set; }
        public CharacterAvatar CharacterAvatar { get; private set; }
        public Movement Movement { get; private set; }
        public Animator Animator { get; private set; }
        public Transform centerMapPoint { get; private set; }

        protected bool ServerPause;
        public bool IsHitEnemy { get; private set; }
        protected WeaponKeycode  weaponKeycode;
        protected MoveKeycode moveKeycode;
        protected bool controllable;
        protected MatchSettings matchSettings;
        
        public UnityAction onRespawn;
        public UnityAction<float, Actor> onDamaged;
        public UnityAction<float> onHealed;
        public UnityAction onDie;
        public UnityAction<BulletBase> onReload;
        public UnityAction<WeaponController> onShoot;
        public UnityAction<WeaponController> onSwitchedToWeapon;
        
        protected void Awake()
        {
            EventManager.AddListener<GamePauseEvent>(OnGamePauseEvent);
            EventManager.AddListener<EndSpawnEvent>(OnEndSpawnEvent);
            
            Damageable = GetComponent<Damageable>();
            Health = GetComponent<Health>();
            Actor = GetComponent<Actor>();
            CharacterAvatar = GetComponent<CharacterAvatar>();
            Movement = GetComponentInChildren<Movement>();
            Arsenal = GetComponent<Arsenal>();
            Animator =  GetComponentInChildren<Animator>();

            CharacterAvatar.onEndDieEffect += OnEndDieEffect;
            Health.onFullHealthAfterDeath += OnFullHealthAfterDeath;
            Health.onDamaged += OnDamaged;
            Health.onHealed += OnHealed;
            Health.onDie += OnDie;
            Arsenal.onShoot += OnShoot;
            Arsenal.onReload += OnReload;
            Arsenal.onSwitchedToWeapon += OnSwitchedToWeapon;
        }

        protected virtual void Update()
        {
            if (!controllable || ServerPause)
            {
                moveKeycode = new MoveKeycode();
                weaponKeycode = new WeaponKeycode();
            }
            
            Arsenal.SetWeaponState(weaponKeycode);
            Movement.SetMovementState(moveKeycode);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<GamePauseEvent>(OnGamePauseEvent);
            EventManager.RemoveListener<EndSpawnEvent>(OnEndSpawnEvent);
        }

        protected virtual void OnReload(BulletBase bullet)
        {
            onReload?.Invoke(bullet);
        }
        
        protected virtual void OnShoot(WeaponController weapon)
        {
            onShoot?.Invoke(weapon);
        }

        protected virtual void OnSwitchedToWeapon(WeaponController weapon)
        {
            onSwitchedToWeapon?.Invoke(weapon);
        }
        
        protected virtual void OnHealed(float amount)
        {
            onHealed?.Invoke(amount);
        }
        
        protected virtual void OnDamaged(float amount, Actor actor)
        {
            onDamaged?.Invoke(amount, actor);
        }

        protected virtual void OnEndDieEffect()
        {
            ActorInfo actorInfo = Actor.Info;
            Movement.Body.position = actorInfo.SpawnPosition; 
            Movement.Body.rotation = actorInfo.SpawnRotation;
        }

        protected virtual void OnFullHealthAfterDeath()
        {
            OnRespawn();
        }
        
        protected virtual void OnDie()
        {
            controllable = false;
            onDie?.Invoke();
            Health.BoostHeal(matchSettings.MultiplerBoostDelayAfterDeath, matchSettings.MultiplerBoostAmountAfterDeath);
        }

        protected virtual void OnRespawn()
        {
            controllable = true;
            onRespawn?.Invoke();
        }
        
        private void OnGamePauseEvent(GamePauseEvent evt)
        {
            ServerPause = evt.ServerPause;
        }

        protected virtual void OnEndSpawnEvent(EndSpawnEvent evt)
        {
            centerMapPoint = evt.LoadManager.centerMap;
            matchSettings = evt.LoadManager.gameSettings.MatchSettings;
            OnRespawn();
        }
    }
}
    
public enum Authority
{
    Bot,
    Player,
    OwnerPlayer,
}