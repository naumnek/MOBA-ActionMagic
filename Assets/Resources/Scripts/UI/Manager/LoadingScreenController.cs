using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Platinum.Scene;
using Platinum.CustomEvents;
using System.Collections.Generic;

namespace Platinum.UI
{

    public class LoadingScreenController : MonoBehaviour
    {
        [FormerlySerializedAs("MenuBackground")] [Header("MainMenu")] 
        public Sprite[] defaultBackgroundSprites;
        
        [Header("References")]
        public AnimationClip ScreenVisibility;
        public AnimationClip ScreenUnvisibility;

        [Range(0.1f, 1f)]
        public float BarFillSpeed = 0.5f;
        [Range(0, 1f)]
        public float BarFillStartLoadScene = 0.99f;
        public TMP_Text ValueLoading;
        public string TitleValueLoading;
        public Image MapBackground;
        public Image LoadingBar;
        //PRIVATE
        private Animator ScreenAnimator;
        private float time = 0f;
        private LoadSceneManager loadSceneManager;
        private Sprite currentBackground;
        private bool isPlayAnimation;
        

        public void Awake() //запускаем самый первый процесс
        {
            loadSceneManager = GetComponent<LoadSceneManager>();
            ScreenAnimator = GetComponentInChildren<Animator>();
            currentBackground = defaultBackgroundSprites[Random.Range(0, defaultBackgroundSprites.Length)];
            loadSceneManager.onSceneLoaded += OnSceneLoaded;
        }

        public void OnSceneLoaded()
        {
            if(loadSceneManager.currentScene != 0)ScreenAnimator.Play(ScreenUnvisibility.name);
        }
        
        public void AnimationScreenVisibility(StateScreen screen)
        {
            switch (screen)
            {
                case (StateScreen.Visibly):
                    isPlayAnimation = true;
                    break;
                case (StateScreen.Unvisibly):
                    isPlayAnimation = false;
                    EventManager.Broadcast(Events.EndScreenUnvisibility);
                    break;
            }
        }
        
        public void StartScreenVisibility(MapSelect map)
        {
            LoadingBar.fillAmount = 0;
            ValueLoading.text = "0%";
            if (map.MapBackgrounds != null && map.MapBackgrounds.Count > 0)
            {
                MapBackground.sprite = map.GetRandomBackground();
            }
            else
            {
                int randomIndex = Random.Range(0,defaultBackgroundSprites.Length); 
                MapBackground.sprite = defaultBackgroundSprites[randomIndex];
            }

            ScreenAnimator.Play(ScreenVisibility.name);
        }
        void Update()
        {
            /*if (loadSceneManager.LoadCurrentScene)
            {
                if (time < loadSceneManager.loadingSceneOperation.progress)
                {
                    //print("Load: " + time + "/" + loadingSceneOperation.progress + "/" + BarFillStartLoadScene);
                    time += Time.deltaTime * BarFillSpeed;
                    LoadingBar.fillAmount = time;
                    ValueLoading.text = (Mathf.RoundToInt(time * 100)).ToString() + "%";
                }
                if (time > BarFillStartLoadScene)
                {
                }
            }*/
            if (isPlayAnimation && loadSceneManager.loadingSceneOperation != null && !loadSceneManager.loadingSceneOperation.isDone)
            {
                float progress = loadSceneManager.loadingSceneOperation.progress;
                ValueLoading.text = (Mathf.RoundToInt(progress * 100)).ToString() + "%";
                if (time >= BarFillStartLoadScene)
                {
                    if(progress >= 0.9f)
                    {
                        loadSceneManager.EndLoadSceneAsync();
                    }
                }
                else
                {
                    time += Time.deltaTime * BarFillSpeed;
                    LoadingBar.fillAmount = time;
                }
            }
        }
    }

}


