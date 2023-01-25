using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Platinum.UI
{
    public class MapSelect : MonoBehaviour
    {
        public int SceneBuildIndex;
        public GameObject MapPressedIcon;
        public List<Sprite> MapBackgrounds;
        public bool buttonPressed { get; private set; }
        public int Seed { get; private set; }

        private void Awake()
        {
            MapBackgrounds = new List<Sprite>();
        }

        public void AddBackground(Sprite background)
        {
            MapBackgrounds.Add(background);
        }
        
        public void SetSeed(int newSeed)
        {
            Seed = newSeed;
        }
        
        public Sprite GetRandomBackground()
        {
            int indexBackground = Random.Range(0, MapBackgrounds.Count);
            return MapBackgrounds[indexBackground];
        }

        public void SetStatePressed(bool state)
        {
            buttonPressed = state;
            MapPressedIcon.SetActive(state);
        }
    }
}
