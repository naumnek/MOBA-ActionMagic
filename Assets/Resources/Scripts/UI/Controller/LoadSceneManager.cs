using Platinum.CustomEvents;
using Platinum.Settings;
using Platinum.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace Platinum.Scene
{
    public class LoadSceneManager : MonoBehaviour
    {

        [Header("General References")]
        public GameSettings gameSettings;

        public GameObject AllManagers;
        public int currentScene { get; private set; }

        private static LoadSceneManager instance;
        private LoadingScreenController LoadingScreenController;
        public MatchResult matchResult { get; private set; } = MatchResult.None;
        //PUBLIC GET
        public int LevelSeed { get; private set; }
        private MapSelect CurrentMap;
        public AsyncOperation loadingSceneOperation { get; private set; }
        public UnityAction onSceneLoaded;
        public bool LoadCurrentScene { get; private set; }
        
        private void Awake()
        {
            LoadSceneManager[] conductors = FindObjectsOfType<LoadSceneManager>();
            if (conductors.Length > 1)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
                EventManager.AddListener<EndSceneEvent>(OnEndLevelEvent);
                LoadingScreenController = GetComponent<LoadingScreenController>(); 
                SceneManager.sceneLoaded += OnSceneLoaded;
                AllManagers.SetActive(true);
                DontDestroyOnLoad(this);
            }
        }

        public static void LoadScene(MapSelect map)
        {
            instance.StartLoadScene(map);
        }

        private void StartLoadScene(MapSelect map)
        {
            CurrentMap = map;
            LevelSeed = CurrentMap.Seed;
            StartLoadSceneAsync();
        }

        public void RestartScene()
        {
            LoadingScreenController.StartScreenVisibility(CurrentMap);
        }

        public void StartLoadSceneAsync()
        {
            loadingSceneOperation = SceneManager.LoadSceneAsync(CurrentMap.SceneBuildIndex);
            loadingSceneOperation.allowSceneActivation = false;
            LoadingScreenController.StartScreenVisibility(CurrentMap);
            LoadCurrentScene = true;
        }

        public void EndLoadSceneAsync()
        {
            loadingSceneOperation.allowSceneActivation = true;
            LoadCurrentScene = false;
        }

        
        public int SetSeed(int seed)
        {
            return LevelSeed = seed;
        }

        public void OnEndLevelEvent(EndSceneEvent evt)
        {
            LoadManager loadManager = FindObjectOfType<LoadManager>();
            matchResult = loadManager.matchResult;
            switch (evt.State)
            {
                case (EndLevelState.LoadMenu):
                    DefaultLoad(gameSettings.MatchSettings.MainMenuBuildIndex);
                    break;
                default:
                    DefaultLoad(currentScene);
                    break;
            }
        }

        private void DefaultLoad(int index)
        {
            MapSelect map = new MapSelect();
            map.SetSeed(0);
            map.SceneBuildIndex = index;
            LevelSeed = map.Seed;
            StartLoadScene(map);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            EventManager.RemoveListener<EndSceneEvent>(OnEndLevelEvent);
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            currentScene = scene.buildIndex;
            onSceneLoaded?.Invoke();
        }
    }
}
