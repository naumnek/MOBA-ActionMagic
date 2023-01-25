using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Platinum.Weapon;
using Random = UnityEngine.Random;

namespace Platinum.Settings
{
    [CreateAssetMenu(menuName = "GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [field: SerializeField]
        public CustomizationSettings CustomizationSettings { get; private set; }

        [field: SerializeField]
        public PlayerSettings PlayerSettings { get; private set; }

        [field: SerializeField]
        public MatchSettings MatchSettings { get; private set; }

        [field: SerializeField]
        public ItemSettings ItemSettings { get; private set; }

        [field: SerializeField]
        public MusicSettings MusicSettings { get; private set; }
        
        [field: SerializeField]
        public AwardsInfo AwardsInfo { get; private set; }
    }
    
    [Serializable]
    public struct AwardsInfo
    {
        public int CoinForKill;
        public int ScoreForExit;
        public int ScoreForLose;
        public int ScoreForWin ;
        public int ScoreForKill;
    }
}
