using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Platinum.Settings;
using Platinum.Scene;
using Platinum.CustomEvents;

public enum ButtonArrow
{
    Left,
    Right,
    Up,
    Down
}

public class SettingsWindow : MonoBehaviour
{

    [Header("General")]
    public GameSettings gameSettings;

    [Header("Match Settings")]
    public Toggle DisableInstanceHitToggle;
    public Toggle PeacifulModeToggle;
    public TMP_InputField SeedInputField;

    [Header("Player Settings")]

    public Toggle FramerateToggle;
    public Toggle VisiblyTrailBulletToggle;
    public TMP_Text ShadowsText;
    public TMP_Text GraphicsQualityText;
    public Slider MusicVolumeSlider;
    public Slider LookSensitivitySlider;
    public Slider CrosshairVerticalSlider;
    public Slider CrosshairHorizontalSlider;
    public Slider CameraDistanceSlider;
    public Slider SkillSizeSlider;
    public TMP_InputField SwitchSkinInputField;
    public TMP_InputField SwitchMusicInputField;

    [Header("Other Settings")]

    public GameObject ShadowsPanel;
    public GameObject QualityPanel;
    public AudioMixer MusicMixer;

    [Header("Old Settings")]
    public Toggle FullScreenToggle;
    public TMP_Dropdown ScreenResolutionDropdown;

    private List<AudioClip> AllMusics;
    private int NumberMusic;
    private LoadManager m_LoadManager;
    private PlayerSaves m_PlayerSaves;
    private CanvasGroup m_CanvasGroup;

    #region EVENT

    private void Awake()
    {
        AllMusics = new List<AudioClip>();
        EventManager.AddListener<EndSpawnEvent>(OnEndSpawnEvent);
        m_CanvasGroup = GetComponent<CanvasGroup>();
        m_PlayerSaves = gameSettings.PlayerSettings.PlayerSaves;
        LoadPlayerSettings();
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<EndSpawnEvent>(OnEndSpawnEvent);
    }

    private void OnEndSpawnEvent(EndSpawnEvent evt)
    {
        m_LoadManager = evt.LoadManager;
        ApplyGameplayPlayerSaves();
    }

    #endregion

    #region LOAD_SAVE_APPLY

    private void LoadPlayerSettings()
    {
        GetPlayerPrefs(nameof(m_PlayerSaves.MusicVolume), MusicVolumeSlider, ref m_PlayerSaves.MusicVolume);
        GetPlayerPrefs(nameof(m_PlayerSaves.LookSensitivity), LookSensitivitySlider, ref m_PlayerSaves.LookSensitivity);
        GetPlayerPrefs(nameof(m_PlayerSaves.CrosshairPosition), CrosshairHorizontalSlider, CrosshairVerticalSlider, ref m_PlayerSaves.CrosshairPosition);
        GetPlayerPrefs(nameof(m_PlayerSaves.CameraDistance), CameraDistanceSlider, ref m_PlayerSaves.CameraDistance);
        GetPlayerPrefs(nameof(m_PlayerSaves.SkillSize), SkillSizeSlider, ref m_PlayerSaves.SkillSize);
        GetPlayerPrefs(nameof(m_PlayerSaves.Shadows), ref m_PlayerSaves.Shadows);
        GetPlayerPrefs(nameof(m_PlayerSaves.Quality), ref m_PlayerSaves.Quality);
        GetPlayerPrefs(nameof(m_PlayerSaves.Framerate), FramerateToggle, ref m_PlayerSaves.Framerate);
        GetPlayerPrefs(nameof(m_PlayerSaves.VisiblyTrailBullet), VisiblyTrailBulletToggle, ref m_PlayerSaves.VisiblyTrailBullet);
        GetPlayerPrefs(nameof(m_PlayerSaves.Skin), SwitchSkinInputField, ref m_PlayerSaves.Skin);
        GetPlayerPrefs(nameof(m_PlayerSaves.Music), SwitchMusicInputField, ref m_PlayerSaves.Music.Name);

        gameSettings.PlayerSettings.SetPlayerSaves(m_PlayerSaves);

        ApplyPlayerSaves();
        ApplyGameplayPlayerSaves();
    }

    public void SavePlayerSettingsButton() //сохраняем значения объектов в файл
    {
        PlayerPrefs.SetFloat(nameof(m_PlayerSaves.MusicVolume), MusicVolumeSlider.value);
        PlayerPrefs.SetFloat(nameof(m_PlayerSaves.LookSensitivity), LookSensitivitySlider.value);
        PlayerPrefs.SetFloat(nameof(m_PlayerSaves.CrosshairPosition) + "y", CrosshairVerticalSlider.value);
        PlayerPrefs.SetFloat(nameof(m_PlayerSaves.CrosshairPosition) + "x", CrosshairHorizontalSlider.value);
        PlayerPrefs.SetFloat(nameof(m_PlayerSaves.CameraDistance), CameraDistanceSlider.value);
        PlayerPrefs.SetFloat(nameof(m_PlayerSaves.SkillSize), SkillSizeSlider.value);
        PlayerPrefs.SetInt(nameof(m_PlayerSaves.Shadows), (int)m_PlayerSaves.Shadows);
        PlayerPrefs.SetInt(nameof(m_PlayerSaves.Quality), (int)m_PlayerSaves.Quality);
        PlayerPrefs.SetInt(nameof(m_PlayerSaves.Framerate), FramerateToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt(nameof(m_PlayerSaves.VisiblyTrailBullet), VisiblyTrailBulletToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt(nameof(m_PlayerSaves.Skin), gameSettings.CustomizationSettings.GetCurrentIndexSkin());

        gameSettings.PlayerSettings.SetPlayerSaves(m_PlayerSaves);
        ApplyPlayerSaves();
        ApplyGameplayPlayerSaves();
    }

    private void ApplyPlayerSaves()
    {
        QualitySettings.SetQualityLevel((int)m_PlayerSaves.Quality, true);
        GraphicsQualityText.text = GetQualityAtIndex((int)m_PlayerSaves.Quality);

        ShadowQuality shadow = m_PlayerSaves.Shadows;
        ShadowsText.text = shadow.ToString();
        QualitySettings.shadows = m_PlayerSaves.Shadows;
    }

    private void ApplyGameplayPlayerSaves()
    {
        if (!m_LoadManager) return;

        /*
        AllMusics.AddRange(SettingsManager.GetMusic(SceneType.Game, MusicType.Battle));

        AudioUtility.SetMasterVolume(1);
        NumberMusic = UnityEngine.Random.Range(0, AllMusics.Count);
        int l = AllMusics.Count;
        string random = "/";
        for (int i = 0; i < 10; i++)
        {
            random += UnityEngine.Random.Range(0, l);
        }

        SetMusic();
        */

        //if(m_LoadManager.VariousEffectsScene) m_LoadManager.VariousEffectsScene.SetSize(m_PlayerSaves.SkillSize);
        if(m_LoadManager.ControllerPlayer) m_LoadManager.ControllerPlayer.CharacterAvatar.UpdateSkin(gameSettings.CustomizationSettings.GetSkin(SkinType.Free, m_PlayerSaves.Skin).Materials);

        if (m_LoadManager.TypeLevel == TypeLevel.Arena)
        {
            //m_LoadManager.ThirdPersonController.SetSensitivity(m_PlayerSaves.LookSensitivity);
        }
        m_LoadManager.FollowPlayerCamera.SetCrosshairPosition(m_PlayerSaves.CrosshairPosition);
        m_LoadManager.FollowPlayerCamera.SerCameraDistance(m_PlayerSaves.CameraDistance);
        m_LoadManager.GameFlowManager.FramerateCounter.UIText.gameObject.SetActive(m_PlayerSaves.Framerate);
    }
    #endregion

    #region PlayerSettings
    public void SetFramerate()
    {
        m_PlayerSaves.Framerate = FramerateToggle.isOn;
    }
    public void SetVisiblyTrailBullet()
    {
        m_PlayerSaves.VisiblyTrailBullet = VisiblyTrailBulletToggle.isOn;
    }

    public void OpenShadowsPanel()
    {
        ShadowsPanel.SetActive(!ShadowsPanel.activeSelf);
    }
    public void SetShadows(int shadows)
    {
        ShadowsPanel.SetActive(!ShadowsPanel.activeSelf);
        ShadowQuality shadow = (ShadowQuality)shadows;
        m_PlayerSaves.Shadows = shadow;
        ShadowsText.text = shadow.ToString();
    }

    public void OpenGraphicsQualityPanel()
    {
        QualityPanel.SetActive(!QualityPanel.activeSelf);
    }
    public void SetGraphicsQuality(int quality)
    {
        QualityPanel.SetActive(!QualityPanel.activeSelf);
        string name = GetQualityAtIndex(quality);
        m_PlayerSaves.Quality = (GraphicsLevel)quality;
        GraphicsQualityText.text = name;
    }

    public void SetMusicVolume() //установка громкости звука
    {
        //if (valueText) valueText.text = (MusicVolumeSlider.value * 4).ToString();
        MusicMixer.SetFloat("musicVolume", -(15 - MusicVolumeSlider.value));
        m_PlayerSaves.MusicVolume = MusicVolumeSlider.value;
    }

    public void SetLookSensitivity()
    {
        //if (value) value.text = LookSensitivitySlider.value.ToString();
        m_PlayerSaves.LookSensitivity = LookSensitivitySlider.value;
    }

    public void SetCrosshairVertical()
    {
        m_PlayerSaves.CrosshairPosition.y = CrosshairVerticalSlider.value;
    }

    public void SetCrosshairHorizontal()
    {
        m_PlayerSaves.CrosshairPosition.x = CrosshairHorizontalSlider.value;
    }

    public void SetCameraDistance()
    {
        m_PlayerSaves.CameraDistance = CameraDistanceSlider.value;
    }
    
    public void SetSkillSize()
    {
        m_PlayerSaves.SkillSize = SkillSizeSlider.value;
    }
    #endregion

    #region OTHER

    private string GetQualityAtIndex(int index)
    {
        string[] names = QualitySettings.names;
        string quality = "Fastest";
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == names[index]) quality = names[i];
        }
        return quality;
    }

    private int GetIndexQualityAtName(string name)
    {
        string[] names = QualitySettings.names;
        int quality = 0;
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == name) quality = i;
        }
        return quality;
    }

    public void SwitchSkinButton(string Arrow)
    {
        switch (Arrow)
        {
            case ("left"):
                gameSettings.CustomizationSettings.LeftSkin();
                gameSettings.CustomizationSettings.SetCurrentIndexSkin();
                m_PlayerSaves.Skin = gameSettings.CustomizationSettings.GetCurrentIndexSkin();
                UpdateNameSkin();
                break;
            case ("right"):
                gameSettings.CustomizationSettings.RightSkin();
                gameSettings.CustomizationSettings.SetCurrentIndexSkin();
                m_PlayerSaves.Skin = gameSettings.CustomizationSettings.GetCurrentIndexSkin();
                UpdateNameSkin();
                break;
        }
    }

    public void SetAlphaWindowButton()
    {
        m_CanvasGroup.alpha = m_CanvasGroup.alpha > 0 ? 0f : 1f;
    }

    public void UpdateMusicName(string name)
    {
        if (SwitchMusicInputField != null) SwitchMusicInputField.text = name;
    }

    private void UpdateNameSkin()
    {
        SwitchSkinInputField.text = gameSettings.CustomizationSettings.GetSkin(SkinType.Free, m_PlayerSaves.Skin).Name;
    }

    public void SwitchMusicButton(ButtonArrow arrow)
    {
        switch (arrow)
        {
            case (ButtonArrow.Left):
                NumberMusic--;
                SetMusic();
                break;
            case (ButtonArrow.Right):
                NumberMusic++;
                SetMusic();
                break;
        }
    }

    private void SetMusic()
    {
        if (AllMusics.Count == 0) return;

        if (NumberMusic < 0) NumberMusic = AllMusics.Count - 1;
        if (NumberMusic > AllMusics.Count - 1) NumberMusic = 0;
        //MusicSource.clip = AllMusics[NumberMusic];
        UpdateMusicName(AllMusics[NumberMusic].name);
    }
    #endregion

    private void GetPlayerPrefs(string key, ref GraphicsLevel defaultValue)
    {
        defaultValue = (GraphicsLevel)(PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int)defaultValue);
    }
    
    #region GET_PREFS
    private void GetPlayerPrefs(string key, ref ShadowQuality defaultValue)
    {
        defaultValue = (ShadowQuality)(PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int)defaultValue);
    }

    private void GetPlayerPrefs(string key, ref int defaultValue)
    {
        defaultValue = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : defaultValue;
    }

    private void GetPlayerPrefs(string key, TMP_InputField Inputfield, ref string defaultValue)
    {
        if (Inputfield == null) return;
        defaultValue = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : defaultValue;
        Inputfield.text = defaultValue;
    }

    private void GetPlayerPrefs(string key, TMP_InputField inputfield, ref int defaultValue)
    {
        if (inputfield == null) return;

        defaultValue = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : defaultValue;
        gameSettings.CustomizationSettings.SetCurrentIndexSkin(defaultValue);
        inputfield.text = gameSettings.CustomizationSettings.GetSkin(SkinType.Free, defaultValue).Name;
    }

    private void GetPlayerPrefs(string key, TMP_InputField inputfield, ref AvatarSkin defaultValue)
    {
        if (inputfield == null) return;

        int index = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
        gameSettings.CustomizationSettings.SetCurrentIndexSkin(index);
        defaultValue = gameSettings.CustomizationSettings.GetCurrentSkin();
        inputfield.text = defaultValue.Name;
    }

    private void GetPlayerPrefs(string key, Slider sliderX, Slider sliderY, ref Vector2 defaultValue)
    {
        if (sliderX == null) return;
        defaultValue.x = PlayerPrefs.HasKey(key + "x") ? PlayerPrefs.GetFloat(key + "x") : defaultValue.x;
        sliderX.value = defaultValue.x;

        if (sliderY == null) return;
        defaultValue.y = PlayerPrefs.HasKey(key + "y") ? PlayerPrefs.GetFloat(key + "y") : defaultValue.y;
        sliderY.value = defaultValue.y;
    }

    private void GetPlayerPrefs(string key, Slider slider, ref float defaultValue)
    {
        if (slider == null) return;
        defaultValue = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : defaultValue;
        slider.value = defaultValue;
    }

    private void GetPlayerPrefs(string key, Toggle toggle, ref bool defaultValue)
    {
        if (toggle == null) return;
        defaultValue = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) > 0 : defaultValue;
        toggle.isOn = defaultValue;
    }
    #endregion

}
