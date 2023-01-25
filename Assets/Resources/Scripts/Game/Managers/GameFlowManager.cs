using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Platinum.Controller;
using Platinum.Settings;
using Platinum.CustomInput;
using Platinum.CustomEvents;
using UnityEngine.Events;

namespace Platinum.UI
{
    public class GameFlowManager : MonoBehaviour
    {
        [Header("References")]
        public SwitchItemMenu SwitchItemMenu;
        public FramerateCounter FramerateCounter;
        public TeamsKillCounter TeamsKillCounter;
        
        [Header("Statictics")]
        [SerializeField] private bool DisableStaticticsPanel;
        public CanvasGroup StaticticsPanel;
        public TMP_Text ResultTitle;
        public TMP_Text CoinCounter;
        public Slider ScoreSlider;
        public TMP_Text LevelCounter;
        
        [Header("UI")]
        public CanvasGroup[] AllMenu;
        public CanvasGroup InGameMenu;
        public CanvasGroup TabMenu;
        public CanvasGroup InfoMenu;
        public CanvasGroup PlayerHUD;
        public CanvasGroup JoysticksCanvas;
        public CanvasGroup SettingsWindow;
        public CanvasGroup ItemListSwitch;
        public GameObject FeedbackFlashCanvas;
        public GameObject GeneratorInfo;
        
        [Header("Parameters")]
        public AudioSource AudioWeaponButtonClick;
        public AudioSource AudioButtonClick;
        public AudioSource MusicSource;

        [Tooltip("Duration of the fade-to-black at the end of the game")]
        public float EndMatchAlphaDelay = 2.5f;


        [Tooltip("The canvas group of the fade-to-black screen")]
        public CanvasGroup EndGameFadeCanvasGroup;

        [Header("Win")]

        [Tooltip("Duration of delay before the fade-to-black, if winning")]
        public float DelayBeforeFadeToBlack = 4f;

        [Tooltip("Win game message")]
        public string WinGameMessage;
        [Tooltip("Duration of delay before the win message")]
        public float DelayBeforeWinMessage = 2f;

        [Tooltip("Sound played on win")] public AudioClip VictorySound;
        [Tooltip("Sound played on defeat")] public AudioClip DefeatSound;

        public bool GameIsEnding { get; private set; }
        public bool HasMenuFocused { get; private set; }

        float m_TimeLoadEndGameScene;
        private bool ServerPause = true;
        private ControllerBase m_ControllerPlayer;
        private GameSettings gameSettings;
        private PlayerInputHandler m_PlayerInputHandler;
        private bool MenuPause = true;
        private AudioSource m_AudioSource;
        private bool firstStart;
        
        private static GameFlowManager instance;

        void OnDestroy()
        {
            EventManager.RemoveListener<EndSpawnEvent>(OnPlayerSpawnEvent);
            EventManager.RemoveListener<MenuPauseEvent>(OnMenuPauseEvent);
            EventManager.RemoveListener<GamePauseEvent>(OnGamePauseEvent);
            EventManager.RemoveListener<RefreshMatchEvent>(OnRefreshMatchEvent);
            EventManager.RemoveListener<StartGenerationEvent>(OnStartGenerationEvent);
            //EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
        }

        void Awake()
        {
            instance = this;
            
            EventManager.AddListener<RefreshMatchEvent>(OnRefreshMatchEvent);
            EventManager.AddListener<EndSpawnEvent>(OnPlayerSpawnEvent);
            EventManager.AddListener<MenuPauseEvent>(OnMenuPauseEvent);
            EventManager.AddListener<GamePauseEvent>(OnGamePauseEvent);
            EventManager.AddListener<StartGenerationEvent>(OnStartGenerationEvent);
            
            m_AudioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            SwitchItemMenu.WeaponLimitReached += OnWeaponLimitReached;
        }

        private void OnWeaponLimitReached()
        {
            m_PlayerInputHandler.SelectWeapon = false;
            SetItemListSwitch(false);
        }

        public void EndMatchEffect()
        {
            StartCoroutine(WaitFade(1));
        }

        bool fade() => Time.time >= m_TimeLoadEndGameScene;
        IEnumerator WaitFade(float alpha)
        {
            m_TimeLoadEndGameScene = Time.time + EndMatchAlphaDelay;
            float timeRatio;
            while (!fade())
            {
                yield return new WaitForFixedUpdate();
                timeRatio = alpha - (m_TimeLoadEndGameScene - Time.time) / EndMatchAlphaDelay;
                EndGameFadeCanvasGroup.alpha = timeRatio;
            }
        }

        private void OnRefreshMatchEvent(RefreshMatchEvent evt)
        {
            SetItemListSwitch(true);
            StartCoroutine(WaitFade(0));
            /*m_TimeLoadEndGameScene = Time.time + EndSceneLoadDelay;

            while (Time.time >= m_TimeLoadEndGameScene)
            {
                float timeRatio = 1 - (m_TimeLoadEndGameScene - Time.time) / EndSceneLoadDelay;
                EndGameFadeCanvasGroup.alpha = timeRatio;
            }*/

            // See if it's time to load the end scene (after the delay)
            //if (Time.time >= m_TimeLoadEndGameScene)
        }

        private void OnPlayerSpawnEvent(EndSpawnEvent evt)
        {
            m_PlayerInputHandler = evt.LoadManager.PlayerInputHandler;
            gameSettings = evt.LoadManager.gameSettings;
            
            m_ControllerPlayer = evt.LoadManager.ControllerPlayer;

            SetItemListSwitch(true);
            ServerPause = false;
        }

        public void ContinueButton()
        {
            SetTabMenu(false);
        }

        public void RestartButton()
        {
            StaticticsPanel.blocksRaycasts = false;
            EndSceneEvent evt = Events.EndSceneEvent;
            evt.State = EndLevelState.RestartLevel;
            EventManager.Broadcast(evt);
        }

        public void GoHomeButton()
        {
            //SetPauseMenuActivation(false);
            Debug.Log("GoHome!");
            
            ServerPause = true;
            SetTabMenu(false);
            SetActivePlayerHUD(false);
            StaticticsPanel.blocksRaycasts = false;
            GameIsEnding = false;
            EndSceneEvent evt = Events.EndSceneEvent;
            evt.State = EndLevelState.LoadMenu;
            EventManager.Broadcast(evt);
        }

        public void SetMusic()
        {
            MusicSource.Pause();
        }

        private void SetMenuAlpha(CanvasGroup menu, bool active)
        {
            if (active)
            {
                SelectMenu(menu);
            }
            else
            {
                ClosedAllMenu();
            }
        }

        public void ClosedAllMenu()
        {
            foreach (CanvasGroup copy in AllMenu)
            {
                copy.alpha = 0f;
                copy.blocksRaycasts = false;
            }
        }
        public void SelectMenu(CanvasGroup menu) //открыть главное меню и закрыть все остальные
        {
            foreach (CanvasGroup copy in AllMenu)
            {
                if (copy != menu) copy.alpha = 0f;
                if (copy != menu) copy.blocksRaycasts = false;
            }
            if (menu == InGameMenu) return;
            menu.alpha = 1f;
            menu.blocksRaycasts = true;
        }

        public void OpenWindowMenu(CanvasGroup menu) //открыть главное меню и закрыть все остальные
        {
            foreach (CanvasGroup copy in AllMenu)
            {
                if (copy != menu) copy.blocksRaycasts = false;
            }
            menu.alpha = 1f;
            menu.blocksRaycasts = true;
        }

        private void OnMenuPauseEvent(MenuPauseEvent evt)
        {
            //CursorState(evt.MenuPause);
            if (evt.MenuPause) m_AudioSource.Pause();
            else m_AudioSource.Play(); 
        }
        private void OnGamePauseEvent(GamePauseEvent evt)
        {
            ServerPause = evt.ServerPause;
        }

        public void SetInfoMenu(bool active)
        {
            InfoMenu.alpha = active ? 1f : 0f;
        }
        
        public void SetActiveGeneratorInfo()
        {
            GeneratorInfo.SetActive(true);
        }

        public void SetActivePlayerHUD(bool active)
        {
            PlayerHUD.alpha = active ? 1f : 0f;
            JoysticksCanvas.alpha = active ? 1f : 0f;
            JoysticksCanvas.blocksRaycasts = active;
            FeedbackFlashCanvas.SetActive(active);
        }

        public void SetItemListSwitch(bool active)
        {
            if (!SwitchItemMenu.IsActive || !gameSettings.ItemSettings.CheckItems()) active = false;
            //Debug.Log("SetItemListSwitch: "  + active);
            
            HasMenuFocused = active;

            float distanceFromSpawn = 
                Vector3.Distance( m_ControllerPlayer.Actor.Info.SpawnPosition,  m_ControllerPlayer.Movement.Body.position);

            //Debug.Log((m_SwitchItemMenu.FixedPanel && distanceFromSpawn > AsteroidsGame.DISTANCE_OPEN_SELECT_WEAPON) + "/" + (m_SwitchItemMenu.FixedPanel));

            if (distanceFromSpawn > gameSettings.MatchSettings.ShopRange
                || TabMenu.alpha == 1f)
                return;

            SetActiveMenu(active);
            SetMenuAlpha(ItemListSwitch, active);
        }

        public void SetTabMenuButton() => SetTabMenu(!TabMenu.blocksRaycasts);

        public void SetTabMenu(bool active)
        {
            HasMenuFocused = active;

            if (ItemListSwitch.alpha == 1f)
            {
                SetItemListSwitch(false);
                return;
            }

            SetActiveMenu(active);
            SetMenuAlpha(TabMenu, active);
        }

        private void SetActiveMenu(bool active)
        {
            SetActivePlayerHUD(!active);
            //CursorState(active);
            SetPauseMenuActivation(active);
        }

        void SetPauseMenuActivation(bool pause)
        {
            MenuPauseEvent evt = Events.MenuPauseEvent;
            evt.MenuPause = pause;
            EventManager.Broadcast(evt);
        }

        void Update()
        {
            if (ServerPause || m_PlayerInputHandler == null || GameIsEnding) return;

            if (m_PlayerInputHandler.tab)
            {
                TabMenuButton();
            }

            if (m_PlayerInputHandler.SelectWeapon)
            {
                m_PlayerInputHandler.SelectWeapon = false;
                SetItemListSwitch(ItemListSwitch.alpha == 1f ? false : true);
            }
        }

        public void UnvisiblyMenu(bool active)
        {
            SetActivePlayerHUD(active);
            SetTabMenu(active);
        }
        
        public void TabMenuButton()
        {
            m_PlayerInputHandler.tab = false;
            SetTabMenu(TabMenu.alpha == 1f ? false : true);
        }
        
        void OnStartGenerationEvent(StartGenerationEvent evt)
        {
            Debug.Log("GFM: StartGenerationEvent");
        }


        public void ResultEndGame(UserStatistics statistics)
        {
            GameIsEnding = true;
            SetActivePlayerHUD(false);
            SetTabMenu(false);
            // unlocks the cursor before leaving the scene, to be able to click buttons
            //Cursor.lockState = CursorLockMode.None;
            // Cursor.visible = true;

            // Remember that we need to load the appropriate end scene after a delay
            //EndGameFadeCanvasGroup.gameObject.SetActive(true);


            MusicSource.Pause();
            switch (statistics.MatchResult)
            {
                case (MatchResult.Win):

                    m_AudioSource.PlayOneShot(VictorySound);
                    ResultTitle.text = "VICTORY!";
                    break;
                case (MatchResult.Lose):
                    m_AudioSource.PlayOneShot(DefeatSound);
                    ResultTitle.text = "DEFEAT";
                    break;
                case (MatchResult.None):
                    m_AudioSource.PlayOneShot(DefeatSound);
                    ResultTitle.text = "WASTED";
                    break;
            }

            if (DisableStaticticsPanel)
            {
                GoHomeButton();
                return;
            }

            CoinCounter.text = statistics.coins.ToString();
            //LevelCounter.text = OwnerInfo.Level.ToString();
            //ScoreSlider.value = gameSettings.PlayerSettings.GetRatioScore;

            SetActiveMenu(true);
            SetMenuAlpha(StaticticsPanel, true);
        }
    }

    public enum EndLevelState
    {
        LoadMenu,
        RestartLevel,
    }
}