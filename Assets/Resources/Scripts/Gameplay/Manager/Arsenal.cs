using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Platinum.Settings;
using Platinum.Info;
using Platinum.CustomEvents;
using Platinum.Weapon;

namespace Platinum.Controller
{
    public class Arsenal : MonoBehaviour
    {
        [Header("General")]
        public bool AutoSwitchNewWeapon = true;
        
        [Header("References")]
        [Tooltip("Parent transform where all weapon will be added in the hierarchy")]
        public Transform WeaponPivot;

        public int ActiveWeaponIndex { get; private set; }

        public UnityAction<BulletBase> onReload;
        public UnityAction<WeaponController> onShoot;
        public UnityAction<WeaponController> onSwitchedToWeapon;
        public UnityAction<WeaponController, int> onAddedWeapon;
        public UnityAction<WeaponController, int> OnRemovedWeapon;

        public WeaponController[] WeaponSlots { get; private set; }  // 9 available weapon slots
        public WeaponController activeWeapon { get; private set; }
        private WeaponController[] WeaponsList;

        private bool ServerPause = true;
        private bool MenuPause = false;

        private ActorsManager m_ActorsManager;
        private ControllerBase m_ControllerBase;
        private GameSettings gameSettings;
        private Animator m_Animator;
        private WeaponController[] StartingWeapons;
        
        protected WeaponKeycode weaponKeycode;
        
        public bool hasFired { get; private set; }

        private void OnDestroy()
        {
            EventManager.RemoveListener<MenuPauseEvent>(OnMenuPauseEvent);
            EventManager.RemoveListener<EndSpawnEvent>(OnEndSpawnEvent);
            EventManager.RemoveListener<GamePauseEvent>(OnGamePauseEvent);
        }

        private void Awake()
        {
            WeaponSlots = new WeaponController[9];

            EventManager.AddListener<MenuPauseEvent>(OnMenuPauseEvent);
            EventManager.AddListener<EndSpawnEvent>(OnEndSpawnEvent);
            EventManager.AddListener<GamePauseEvent>(OnGamePauseEvent);

            m_ControllerBase = GetComponent<ControllerBase>();
            m_Animator =  GetComponentInChildren<Animator>();
            m_ControllerBase.Health.onDie += OnDie;
            m_ControllerBase.onRespawn += OnRespawn;
        }

        public void SetWeaponState(WeaponKeycode newKeycode)
        {
            weaponKeycode = newKeycode;
            activeWeapon.HandleReloadWeapon(newKeycode.reload);
            HasShoot(newKeycode.shootDirection);
        }
        
        private void HasShoot(Vector2 direction)
        {
            if (activeWeapon.HandleShootTarget(direction))
            {
                m_ControllerBase.Movement.SetStoppedMove(activeWeapon.StoppedMoveAfterShot);
                m_ControllerBase.Movement.SetDirection(direction);
                m_Animator.Play(activeWeapon.AttackAnimation.name);
                onShoot?.Invoke(activeWeapon);
                if(activeWeapon.AutomaticReload) onReload?.Invoke(activeWeapon.activeBullet);
            }
        }

        public void ResetWeapons()
        {
            WeaponController[] activeWeapons = WeaponSlots.Where(w => w != null).ToArray();
            for (int i = 0; i < activeWeapons.Length; i++)
            {
                RemoveWeapon(activeWeapons[i]);
            }

            ActiveWeaponIndex = -1;

            // Add starting weapons
            for (int i = 0; i < StartingWeapons?.Length; i++)
            {
                AddWeapon(StartingWeapons[i].WeaponName);
            }
        }

        private void OnRespawn()
        {
            ResetWeapons();
        }

        private void OnDie()
        {
            ServerPause = true;
        }

        private void OnEndSpawnEvent(EndSpawnEvent evt)
        {
            gameSettings = evt.LoadManager.gameSettings;
            WeaponsList = gameSettings.ItemSettings.GetRequredWeapons();
            
            //StartingWeapons = gameSettings.ItemSettings.ExceptNotRequredWeapon(StartingWeapons);
            StartingWeapons = gameSettings.MatchSettings.StartingWeapons;
            OnRespawn();

            ServerPause = false;
        }

        private void OnGamePauseEvent(GamePauseEvent evt)
        {
            ServerPause = evt.ServerPause;
        }
        private void OnMenuPauseEvent(MenuPauseEvent evt)
        {
            MenuPause = evt.MenuPause;
        }

        public bool IsHitEnemy { get; private set; } = false;
        public void OnHitEnemy(int affiliation)
        {
            IsHitEnemy = affiliation != m_ControllerBase.Actor.Info.Affiliation.Number;
        }

        public void ResetHitEnemy()
        {
            IsHitEnemy = false;
        }

        // Iterate on all weapon slots to find the next valid weapon to switch to
        public void SwitchWeapon(bool ascendingOrder)
        {
            int newWeaponIndex = -1;
            int closestSlotDistance = WeaponSlots.Length;
            for (int i = 0; i < WeaponSlots.Length; i++)
            {
                // If the weapon at this slot is valid, calculate its "distance" from the active slot index (either in ascending or descending order)
                // and select it if it's the closest distance yet
                if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
                {
                    int distanceToActiveIndex = GetDistanceBetweenWeaponSlots(ActiveWeaponIndex, i, ascendingOrder);

                    if (distanceToActiveIndex < closestSlotDistance)
                    {
                        closestSlotDistance = distanceToActiveIndex;
                        newWeaponIndex = i;
                    }
                }
            }

            // Handle switching to the new weapon index
            SwitchToWeaponIndex(newWeaponIndex);
        }

        // Switches to the given weapon index in weapon slots if the new index is a valid weapon that is different from our current one
        public void SwitchToWeaponIndex(int newWeaponIndex)
        {
            if (newWeaponIndex != ActiveWeaponIndex && newWeaponIndex >= 0)
            {
                // Handle case of switching to a valid weapon for the first time (simply put it up without putting anything down first)
                if (activeWeapon == null)
                {
                    ActiveWeaponIndex = newWeaponIndex;

                    WeaponController newSkill = GetWeaponAtSlotIndex(newWeaponIndex);
                    activeWeapon = WeaponSlots[ActiveWeaponIndex];
                    bool isFirstShow = newWeaponIndex == 0;
                    if (activeWeapon.skill)
                    {
                        Debug.Log(m_Animator.name);
                        Debug.Log(activeWeapon.name);
                        Debug.Log(activeWeapon.skill.name);
                        m_Animator.SetFloat("castSpeed", activeWeapon.skill.CastSpeed);
                    }
                    newSkill.ShowWeapon(true,isFirstShow);
                    
                    onSwitchedToWeapon?.Invoke(newSkill);
                }
            }
        }

        public WeaponController HasWeapon(WeaponController prefab)
        {
            if (prefab == null) return null;
            // Checks if we already have a weapon coming from the specified prefab
            for (var index = 0; index < WeaponSlots.Length; index++)
            {
                var w = WeaponSlots[index];
                if (w != null && w.WeaponName == prefab.WeaponName)
                {
                    return w;
                }
            }

            return null;
        }
        
        private int weaponSlot;

        public bool AddWeapon(string weaponName)
        {
            for (int i = 0; i < WeaponsList.Length; i++)
            {
                // if we already hold this weapon type (a weapon coming from the same source prefab), don't add the weapon
                if (WeaponsList[i].WeaponName == weaponName && HasWeapon(WeaponsList[i]) != null) return false;
            }

            // search our weapon slots for the first free one, assign the weapon to it, and return true if we found one. Return false otherwise
            for (int i = 0; i < WeaponSlots.Length; i++)
            {
                // only add the weapon if the slot is free
                if (WeaponSlots[i] == null)
                {
                    weaponSlot = i;
                    GameObject weapon = gameSettings.ItemSettings.GetRequredWeapons().Where(w => w.WeaponName == weaponName).FirstOrDefault().gameObject;
                    // spawn the weapon prefab as child of the weapon socket
                    GameObject weaponObject = Instantiate(weapon, WeaponPivot.position, WeaponPivot.rotation);
                    InitWeapon(weaponObject);
                    return true;
                }
            }

            return false;
        }

        // Adds a weapon to our inventory
        public bool InitWeapon(GameObject weaponObject)
        {
            // spawn the weapon prefab as child of the weapon socket
            WeaponController weaponInstance = weaponObject.GetComponent<WeaponController>();

            weaponInstance.OnHit += OnHitEnemy;

            weaponInstance.transform.parent = WeaponPivot;
            weaponInstance.transform.localPosition = Vector3.zero;
            weaponInstance.transform.localRotation = Quaternion.identity;
            weaponInstance.transform.localScale = new Vector3(1, 1, 1);

            // Set owner to this gameObject so the weapon can alter projectile/damage logic accordingly
            weaponInstance.SetOwner(m_ControllerBase, weaponSlot);

            WeaponSlots[weaponSlot] = weaponInstance;

            onAddedWeapon?.Invoke(weaponInstance, weaponSlot);

            // Handle auto-switching to weapon if no weapons currently
            if (activeWeapon == null || m_ControllerBase.Arsenal.AutoSwitchNewWeapon)
            {
                SwitchWeapon(true);
            }

            return true;
        }

        public bool RemoveWeapon(WeaponController skillInstance)
        {

            // Look through our slots for that weapon
            for (int i = 0; i < WeaponSlots.Length; i++)
            {
                // when weapon found, remove it
                if (WeaponSlots[i] && WeaponSlots[i].WeaponName == skillInstance.WeaponName)
                {
                    WeaponController removedSkill = WeaponSlots[i];
                    WeaponSlots[i] = null;

                    if (OnRemovedWeapon != null)
                    {
                        OnRemovedWeapon.Invoke(removedSkill, i);
                    }

                    removedSkill.OnHit -= OnHitEnemy;
                    Destroy(removedSkill.gameObject);

                    // Handle case of removing active weapon (switch to next weapon)
                    if (i == ActiveWeaponIndex)
                    {
                        SwitchWeapon(true);
                    }

                    return true;
                }
            }

            return false;
        }

        public WeaponController GetWeaponAtSlotIndex(int index)
        {
            // find the active weapon in our weapon slots based on our active weapon index
            if (index >= 0 &&
                index < WeaponSlots.Length)
            {
                return WeaponSlots[index];
            }

            // if we didn't find a valid active weapon in our weapon slots, return null
            return null;
        }

        // Calculates the "distance" between two weapon slot indexes
        // For example: if we had 5 weapon slots, the distance between slots #2 and #4 would be 2 in ascending order, and 3 in descending order
        int GetDistanceBetweenWeaponSlots(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
        {
            int distanceBetweenSlots = 0;

            if (ascendingOrder)
            {
                distanceBetweenSlots = toSlotIndex - fromSlotIndex;
            }
            else
            {
                distanceBetweenSlots = -1 * (toSlotIndex - fromSlotIndex);
            }

            if (distanceBetweenSlots < 0)
            {
                distanceBetweenSlots = WeaponSlots.Length + distanceBetweenSlots;
            }

            return distanceBetweenSlots;
        }
    }
}
