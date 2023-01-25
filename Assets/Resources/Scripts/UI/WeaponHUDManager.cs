using System.Collections.Generic;
using Platinum.Controller;
using Unity.FPS.Game;
using UnityEngine;
using Platinum.Weapon;
using Platinum.CustomEvents;

namespace Platinum.UI
{
    public class WeaponHUDManager : MonoBehaviour
    {
        [Tooltip("UI panel containing the layoutGroup for displaying weapon ammo")]
        public RectTransform AmmoPanel;

        [Tooltip("Prefab for displaying weapon ammo")]
        public GameObject AmmoCounterPrefab;

        Arsenal m_Arsenal;
        List<AmmoCounter> m_AmmoCounters = new List<AmmoCounter>();

        private void OnDestroy()
        {
            EventManager.RemoveListener<EndSpawnEvent>(OnPlayerSpawnEvent);
        }

        private void Awake()
        {
            EventManager.AddListener<EndSpawnEvent>(OnPlayerSpawnEvent);
        }
        
        private void OnPlayerSpawnEvent(EndSpawnEvent evt)
        {
            m_Arsenal = evt.LoadManager.ControllerPlayer.Arsenal;

            WeaponController activeWeapon = m_Arsenal.activeWeapon;
            if (activeWeapon)
            {
                AddWeapon(activeWeapon, m_Arsenal.ActiveWeaponIndex);
                ChangeWeapon(activeWeapon);
            }

            m_Arsenal.onAddedWeapon += AddWeapon;
            m_Arsenal.OnRemovedWeapon += RemoveWeapon;
            m_Arsenal.onSwitchedToWeapon += ChangeWeapon;
        }
        
        void AddWeapon(WeaponController newSkill, int weaponIndex)
        {
            GameObject ammoCounterInstance = Instantiate(AmmoCounterPrefab, AmmoPanel);
            AmmoCounter newAmmoCounter = ammoCounterInstance.GetComponent<AmmoCounter>();
            DebugUtility.HandleErrorIfNullGetComponent<AmmoCounter, WeaponHUDManager>(newAmmoCounter, this,
                ammoCounterInstance.gameObject);

            newAmmoCounter.Initialize(newSkill, m_Arsenal);

            m_AmmoCounters.Add(newAmmoCounter);
        }

        void RemoveWeapon(WeaponController newSkill, int weaponIndex)
        {
            int foundCounterIndex = -1;
            for (int i = 0; i < m_AmmoCounters.Count; i++)
            {
                if (m_AmmoCounters[i].WeaponCounterIndex == weaponIndex)
                {
                    foundCounterIndex = i;
                    Destroy(m_AmmoCounters[i].gameObject);
                }
            }

            if (foundCounterIndex >= 0)
            {
                m_AmmoCounters.RemoveAt(foundCounterIndex);
            }
        }

        void ChangeWeapon(WeaponController skill)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(AmmoPanel);
        }
    }
}