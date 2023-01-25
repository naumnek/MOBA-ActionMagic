using Platinum.Settings;
using System.Collections.Generic;
using TMPro;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Platinum.UI;
using Platinum.Scene;
using Random = UnityEngine.Random;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

public class FastWindowMenu : MonoBehaviour
{
    [Header("General")]
    public GameSettings gameSettings;
    public MapSelect Map;
    [Header("Other Menu")]
    public CanvasGroup[] allMenu;
    public CanvasGroup SettingsMenu;
    [Header("Login Menu")]
    public CanvasGroup LoginMenu;
    public TMP_InputField InputUsername;
    [Header("Start Menu")] 
    public Color versionColor;
    public TMP_Text GameVersionText;
    public string GameVersionTitle = "Version: ";
    [Header("Select Map Menu")]
    //PRIVATE
    private PlayerInfo OwnerInfo;
    
    // Start is called before the first frame update
    void Awake()
    {
        //"<#afd9e9>"
        GameVersionText.text = GameVersionTitle + ColorTypeConverter.ToRGBHex(versionColor) + Application.version;

        OwnerInfo = gameSettings.PlayerSettings.OwnerPlayerInfo;
        if (OwnerInfo.Nickname == "")
        {
            InputUsername.text = gameSettings.PlayerSettings.CreateOwnerInfo().Nickname;
            LoginMenu.alpha = 1f;
        }
        else
        {
            InputUsername.text = OwnerInfo.Nickname;
            //OkButton();
        }
            
        ResetCursorPosition();
    }
    
    private void ResetCursorPosition()
    {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Mouse.current.WarpCursorPosition(screenCenterPoint);
        InputState.Change(Mouse.current.position, screenCenterPoint);
            
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void OkButton() //загрузка уровня
    {
        Debug.Log("LoadScene");
        gameSettings.PlayerSettings.SetNickname(InputUsername.text);
        foreach (CanvasGroup copy in allMenu)
        {
            copy.alpha = 0f;
        }

        LoadSceneManager.LoadScene(Map);
    }
    
    public void SetStateWindowMenu(CanvasGroup menu) //открыть главное меню и закрыть все остальные
    {
        bool newState = !menu.blocksRaycasts;
        foreach (CanvasGroup copy in allMenu)
        {
            if (copy != menu) copy.blocksRaycasts = !newState;
        }
        menu.alpha = newState ? 1f : 0f;
        menu.blocksRaycasts = newState;
    }
}
