using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platinum.Weapon;

namespace Platinum.Settings
{
    [CreateAssetMenu(menuName = "MatchSettings")]
    public class MatchSettings : ScriptableObject
    {
        [field: SerializeField]
        public int MainMenuBuildIndex { get; private set; }
        
        [field: SerializeField]
        public int CountTeams { get; private set; }
        
        [field: SerializeField]
        public float ShopRange  { get; private set; }
        
        [field: SerializeField]
        public float TimeUntilStoreLock  { get; private set; }
        
        [field: SerializeField]
        public float MaxPaidWeapons  { get; private set; }
        
        [field: SerializeField]
        public float RespawnDelay  { get; private set; }
        
        [field: SerializeField]
        public float MultiplerBoostDelayAfterDeath { get; private set; }
        
        [field: SerializeField]
        public float MultiplerBoostAmountAfterDeath { get; private set; }
        
        [field: SerializeField]
        public bool PeacifulMode { get; private set; }
        
        [field: SerializeField]
        public int Seed;
        
        [field: SerializeField]
        public WeaponController[] StartingWeapons { get; private set; }
        
        [field: SerializeField]
        public MembersList[] MembersList { get; private set; }
        
        public int PlayersPerTeam => GetMaxCountMembers() / CountTeams;
        public int GetMaxCountMembers()
        {
            int count = 0;
            foreach (MembersList list in MembersList)
            {
                count += list.Count;
            }
            return count;
        }
    }
    
    [System.Serializable]
    public struct MembersList
    {
        public Authority Authority;
        public GameObject Prefab;
        public int Count;
    }
    public enum SceneType
    {
        Game,
        Menu,
    }
}