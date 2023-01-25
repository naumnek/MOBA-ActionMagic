using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;

namespace Platinum.Settings
{
    [CreateAssetMenu(menuName = "PlayerSettings")]
    public class PlayerSettings : ScriptableObject
    {
        [field: Header("Global")]
        [field: SerializeField]
        public PlayerInfo DefaultPlayerInfo { get; private set; }
        
        [field: SerializeField]
        public float[] ScoreLict { get; private set; }
        
        [field: Header("User")]
        [field: SerializeField]
        public PlayerInfo OwnerPlayerInfo { get; private set; }
        
        [field: SerializeField]
        public PlayerSaves PlayerSaves { get; private set; }
        
        public float GetRatioScore => OwnerPlayerInfo.Score / ScoreLict[OwnerPlayerInfo.Level];

        public PlayerInfo CreateOwnerInfo()
        {
            string newName = GenerateNickName();
            PlayerInfo info = DefaultPlayerInfo;
            info.Nickname = newName;
            return info;
        }
        
        public PlayerInfo GeneratePlayerInfo()
        {
            string newName = GenerateNickName();
            PlayerInfo info = DefaultPlayerInfo;
            info.Nickname = newName;
            return info;
        }
        
        private string GenerateNickName() => DefaultPlayerInfo.Nickname + Random.Range(100, 1000);

        public void SetSkin(int skin)
        {
            PlayerSaves saves = PlayerSaves;
            saves.Skin = skin;
            PlayerSaves = saves;
        }
        public void SetNickname(string name)
        {
            PlayerInfo info = OwnerPlayerInfo;
            info.Nickname = name;
            OwnerPlayerInfo = info;
        }
        
        public void AddScore(float score)
        {
            PlayerInfo info = OwnerPlayerInfo;
            info.Score += score;
            float ruquredScore = ScoreLict.FirstOrDefault(s => s >= info.Score);
            for (;info.Score >= ruquredScore;)
            {
                info.Score -= ruquredScore;
                info.Level++;
                ScoreLict.First(s => s >= info.Score);
            }

            OwnerPlayerInfo = info;
        }
        public void AddKillAmount(int amountKill)
        {
            PlayerInfo info = OwnerPlayerInfo;
            info.AmountKill += amountKill;
            OwnerPlayerInfo = info;
        }
        public void AddCoin(int coin)
        {
            PlayerInfo info = OwnerPlayerInfo;
            info.Coin += coin;
            OwnerPlayerInfo = info;
        }

        public void SetPlayerSaves(PlayerSaves settings)
        {
            PlayerSaves = settings;
        }
    }
    
    [Serializable]
    public struct PlayerInfo
    {
        public string Nickname;
        public int AmountKill;
        public int Coin;
        public float Score;
        public int Level;
    }

    [Serializable]
    public struct PlayerSaves
    {
        public int Skin;
        public MusicInfo Music;
        public float MusicVolume;
        public float LookSensitivity;
        public Vector2 CrosshairPosition;
        public float CameraDistance;
        public float SkillSize;
        public ShadowQuality Shadows;
        public GraphicsLevel Quality;
        public bool Framerate;
        public bool VisiblyTrailBullet;
    }

    public enum GraphicsLevel
    {
        Fastest,
        Fast,
        Simple,
        Good,
        Beautiful,
        Fantastic
    }
}
