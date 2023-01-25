using System.Collections.Generic;
using TMPro;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Random = UnityEngine.Random;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif
using Platinum.Settings;
using Platinum.Scene;

public enum Arrow
{
    Left,
    Right
}
namespace Platinum.UI
{
    public class MenuController : MonoBehaviour
    {
        [Header("General")] 
        public Animator playerAnimator;
        public SkinnedMeshRenderer PlayerSkinMesh;
        public GameSettings gameSettings;
        public CanvasGroup[] allMenu;
        public AudioMixer MusicMixer;
        public AudioSource MusicSource;
        [Header("Login Menu")]
        public CanvasGroup LoginMenu;
        public TMP_InputField InputUsername;
        [Header("PlayerStats")]
        public TMP_Text Username;
        public TMP_Text Coin;
        //public Slider ScoreSlider;
        //public TMP_Text Level;
        [Header("Start Menu")]
        public MapSelect StartMap;
        public CanvasGroup StartMenu;
        public TMP_Text GameVersionText;
        public string GameVersionTitle = "Version: ";
        public Color versionColor;
        //PRIVATE
        private LoadSceneManager loadSceneManager;
        private Sprite m_DefaultSpriteLoadMapButton;
        private MapSelect m_CurrentMap;
        private AudioClip[] WinMusics;
        private AudioClip[] LoseMusics;
        private int NumberMusic;
        private PlayerInfo OwnerInfo;

        private void Start()
        {
            WinMusics = gameSettings.MusicSettings.GetMusic(SceneType.Menu, MusicType.Happy);
            LoseMusics = gameSettings.MusicSettings.GetMusic(SceneType.Menu, MusicType.Sad);
            loadSceneManager = FindObjectOfType<LoadSceneManager>();
            GameVersionText.text = GameVersionTitle + ColorTypeConverter.ToRGBHex(versionColor) + Application.version;
            PlayerSkinMesh.materials = gameSettings.CustomizationSettings.GetCurrentSkin().Materials;

            OwnerInfo = gameSettings.PlayerSettings.OwnerPlayerInfo;
            if (OwnerInfo.Nickname == "")
            {
                InputUsername.text = gameSettings.PlayerSettings.CreateOwnerInfo().Nickname;
                LoginMenu.alpha = 1f;
            }
            else
            {
                InputUsername.text = OwnerInfo.Nickname;
                EndLogin();
            }
            
            ResetCursorPosition();
        }

        private void EndLogin()
        {
            LoadPlayerData();
            SetMusic();
            SelectMenu(StartMenu);
        }

        public void ArrowsCharacterButton(int arrow)
        {
            switch (arrow)
            {
                case 0:
                    PlayerSkinMesh.materials = gameSettings.CustomizationSettings.LeftSkin().Materials;
                    break;
                case 1:
                    PlayerSkinMesh.materials = gameSettings.CustomizationSettings.RightSkin().Materials;
                    break;
            }
            playerAnimator.SetTrigger("Land");
        }

        public void OkSelectSkinButton()
        {
            gameSettings.CustomizationSettings.SetCurrentIndexSkin();
            gameSettings.PlayerSettings.SetSkin(gameSettings.CustomizationSettings.GetCurrentIndexSkin());
        }
        
        public void LoginButton()
        {
            string nickname = InputUsername.text;
            gameSettings.PlayerSettings.SetNickname(nickname);
            EndLogin();
        }
        
        public void FastStartButton() //загрузка уровня
        {
            Debug.Log("Menu: LoadScene");

            LoadScene(StartMap);
        }
        
        private void ResetCursorPosition()
        {
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Mouse.current.WarpCursorPosition(screenCenterPoint);
            InputState.Change(Mouse.current.position, screenCenterPoint);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void SetMusic()
        {
            if (!MusicSource || WinMusics.Length == 0) return;

            if (loadSceneManager != null && loadSceneManager.matchResult == MatchResult.Win)
            {
                NumberMusic = UnityEngine.Random.Range(0, WinMusics.Length);
                MusicSource.clip = WinMusics[NumberMusic];
            }
            else
            {
                NumberMusic = UnityEngine.Random.Range(0, LoseMusics.Length);
                MusicSource.clip = LoseMusics[NumberMusic];
            }
            if (MusicSource.clip.name == "Cafofo - AMB - Muffled Pop Music") MusicSource.volume = 1;
            MusicSource.Play();
        }

        private void LoadPlayerData()
        {
            Username.text = InputUsername.text;
            Coin.text = OwnerInfo.Coin.ToString();
            //Level.text = OwnerInfo.Level.ToString();
            //ScoreSlider.value = gameSettings.PlayerSettings.GetRatioScore;
        }

        public void SelectMap(MapSelect map)
        {
            m_CurrentMap = !m_CurrentMap ? map : m_CurrentMap;
            m_CurrentMap.SetStatePressed(false);
            map.SetStatePressed(true);
            m_CurrentMap = map;
        }
        
        public void LoadScene(MapSelect map) //загрузка уровня
        {
            foreach (CanvasGroup copy in allMenu)
            {
                copy.alpha = 0f;
            }

            LoadSceneManager.LoadScene(map);
        }

        public void SelectMenu(CanvasGroup menu) //открыть главное меню и закрыть все остальные
        {
            foreach (CanvasGroup copy in allMenu)
            {
                if (copy != menu) copy.alpha = 0f;
                if (copy != menu) copy.blocksRaycasts = false;
            }
            menu.alpha = 1f;
            menu.blocksRaycasts = true;
        }

        public void OpenWindowMenu(CanvasGroup menu) //открыть главное меню и закрыть все остальные
        {
            foreach (CanvasGroup copy in allMenu)
            {
                if (copy != menu) copy.blocksRaycasts = false;
            }
            menu.alpha = 1f;
            menu.blocksRaycasts = true;
        }

        public void Exit() //выход из игры
        {
            Application.Quit();
        }
    }
}

