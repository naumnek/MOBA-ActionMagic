using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Platinum.Controller;
using Platinum.Settings;
using Platinum.CustomEvents;
using Platinum.Weapon;

namespace Platinum.UI
{
    public class SwitchItemMenu : MonoBehaviour
    {
        [Header("General")]
        [Tooltip("Root GameObject of the menu used to toggle its activation")]
        public GameObject MenuRoot;

        [Header("SwitchItems List Panel")]
        public GameObject ItemListContent;
        public Transform GridLayotMachine;
        public Transform GridLayotShotgun;
        public Transform GridLayotRifle;
        public GameObject WeaponsElementPrefab;
        public Button CloseItemMenuButton;
        public List<WeaponController> PaidWeapons { get; private set; }
        public UnityAction WeaponLimitReached;

        private WeaponItem[] ItemList;

        private Dictionary<string, WeaponItem> cachedRoomList;
        private Dictionary<string, GameObject> roomListEntries;

        public GameSettings gameSettings { get; private set; }
        public bool IsActive { get; private set; } = true;
        private ControllerBase m_ControllerPlayer;
        private GameFlowManager m_FlowManager;
        private List<WeaponAttributes> m_WeaponsAttributes;

        private static SwitchItemMenu instance;
        public static SwitchItemMenu GetInstance() => instance;

        private void OnDestroy()
        {
            EventManager.RemoveListener<EndSpawnEvent>(OnEndSpawnEvent);
            EventManager.AddListener<RefreshMatchEvent>(OnRefreshMatchEvent);
        }

        private void Awake()
        {
            instance = this;

            EventManager.AddListener<EndSpawnEvent>(OnEndSpawnEvent);
            EventManager.AddListener<RefreshMatchEvent>(OnRefreshMatchEvent);

            m_WeaponsAttributes = new List<WeaponAttributes>();
            cachedRoomList = new Dictionary<string, WeaponItem>();
            roomListEntries = new Dictionary<string, GameObject>();
            PaidWeapons = new List<WeaponController> { };
        }

        private void OnEndSpawnEvent(EndSpawnEvent evt)
        {
            m_FlowManager = evt.LoadManager.GameFlowManager;
            gameSettings = evt.LoadManager.gameSettings;
            
            m_ControllerPlayer = evt.LoadManager.ControllerPlayer;

            ItemList = gameSettings.ItemSettings.GetRequredItems();
            IsActive = ItemList.Length > 0;

            for (int i = 0; i < ItemList.Length; i++)
            {
                m_WeaponsAttributes.Add(ItemList[i].Attributes);
            }

            Initialize();
        }

        private void Initialize()
        {
            //WeaponsLists.Add(Instantiate(WeaponsListPrefab, ItemListContent.transform).transform);c
            OnRoomListUpdate(ItemList);
        }

        private void OnRefreshMatchEvent(RefreshMatchEvent evt)
        {
            ResetItems();
            StartCoroutine(MaxWaitPaid());
        }

        public void OnRoomListUpdate(WeaponItem[] itemList)
        {
            ClearRoomListView();

            UpdateCachedRoomList(itemList);
            UpdateRoomListView();
        }

        private void ClearRoomListView()
        {
            foreach (GameObject entry in roomListEntries.Values)
            {
                DestroyImmediate(entry.gameObject);
            }

            roomListEntries.Clear();
        }

        public void ResetItems()
        {
            IsActive = true;

            OnRoomListUpdate(ItemList);
            //m_FlowManager.SetItemListSwitch(true);
        }

        private void UpdateCachedRoomList(WeaponItem[] roomList)
        {
            foreach (WeaponItem info in roomList)
            {
                // Remove menu from cached menu list if it got closed, became invisible or was marked as removed
                if (info.RemovedFromList || info.IsUnvisibly)
                {
                    if (cachedRoomList.ContainsKey(info.Name()))
                    {
                        cachedRoomList.Remove(info.Name());
                    }

                    continue;
                }

                // Update cached menu info
                if (cachedRoomList.ContainsKey(info.Name()))
                {
                    cachedRoomList[info.Name()] = info;
                }
                // Add new menu info to cache
                else
                {
                    cachedRoomList.Add(info.Name(), info);
                }
            }
        }

        private void UpdateRoomListView()
        {
            foreach (WeaponItem info in cachedRoomList.Values)
            {
                Transform weaponGridSlot = GridLayotMachine;
                switch (info.weapon.LookType)
                {
                    case (WeaponLookType.Circle):
                        weaponGridSlot = GridLayotMachine;
                        break;
                    case (WeaponLookType.Line):
                        weaponGridSlot = GridLayotShotgun;
                        break;
                    case (WeaponLookType.Projector):
                        weaponGridSlot = GridLayotRifle;
                        break;
                }
                GameObject entry = Instantiate(WeaponsElementPrefab, weaponGridSlot);
                //entry.transform.localScale = Vector3.one;

                ItemListSwitch ItemListSwitch = entry.GetComponent<ItemListSwitch>();
                ItemListSwitch.ItemChooseButton.onClick.AddListener
                    (delegate () { m_FlowManager.AudioWeaponButtonClick.Play(); });

                ItemListSwitch.Initialize(info, this, IsActive);

                roomListEntries.Add(info.Name(), entry);
            }
        }

        public void GiveItemPlayer(WeaponItem weaponItemInfo, ItemListSwitch itemSwitch)
        {
            if (!weaponItemInfo.IsPaid && !weaponItemInfo.IsBlocked && !m_ControllerPlayer.Health.IsDead)
            {
                if (IsActive)
                {
                    for (int i = 0; i < PaidWeapons.Count; i++)
                    {
                        m_ControllerPlayer.Arsenal.RemoveWeapon(PaidWeapons[i]);
                        PaidWeapons.RemoveAt(i);
                    }
                    foreach (WeaponItem info in ItemList)
                    {
                        info.Reset();
                    }
                }

                PaidWeapons.Add(weaponItemInfo.weapon);
                CloseItemMenuButton.gameObject.SetActive(true);
                m_ControllerPlayer.Arsenal.AddWeapon(weaponItemInfo.weapon.WeaponName);

                weaponItemInfo.OnPaid();

                IsActive = PaidWeapons.Count < gameSettings.MatchSettings.MaxPaidWeapons;
                if(!IsActive) WeaponLimitReached?.Invoke();

                OnRoomListUpdate(ItemList);
            }
        }

        private IEnumerator MaxWaitPaid()
        {
            yield return new WaitForSeconds(gameSettings.MatchSettings.TimeUntilStoreLock);
            IsActive = false;
            OnRoomListUpdate(ItemList);
        }

        public void CloseItemMenu()
        {
            m_FlowManager.SetItemListSwitch(false);
        }
    }
}