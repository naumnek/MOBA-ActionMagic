using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Platinum.Settings
{
    [CreateAssetMenu(menuName = "MusicSettings")]
    public class MusicSettings : ScriptableObject
    {
        [field: SerializeField]
        public MusicInfo[] MusicList { get; private set; }

        public AudioClip[] GetMusic(SceneType sceneType, MusicType musicType)
        {
            List<AudioClip> music = new List<AudioClip> { };
            if (MusicList == null) return music.ToArray();
            
            for (int i = 0; i < MusicList.Length; i++)
            {
                if (MusicList[i].SceneType == sceneType && MusicList[i].MusicType == musicType)
                {
                    music.Add(MusicList[i].Audio);
                }
            }
            return music.ToArray();
        }
    }

    public enum MusicType
    {
        Sad,
        Happy,
        Battle,
        Other,
    }

    [Serializable]
    public class MusicInfo
    {
        public string Name;
        public AudioClip Audio;
        public MusicType MusicType;
        public SceneType SceneType;
    }
}