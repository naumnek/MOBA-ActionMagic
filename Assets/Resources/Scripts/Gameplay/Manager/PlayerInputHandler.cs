using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif
using Platinum.Player;
using Platinum.Scene;
using Platinum.Settings;
using Platinum.UI;
using Platinum.CustomEvents;
using Platinum.Info;

namespace Platinum.CustomInput
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("General")] 
        public bool UnlockedCursor;
        
        //[Header("References")] 
        //public UIVirtualJoystick UIVirtualJoystickMove;
        
        public MoveState MoveState { get; private set; }
        
        public static MoveKeycode MoveKeycode;
        public static WeaponKeycode WeaponKeycode;
        
        private int getNumber;
        
        public Vector2 movearrow { get; private set; }
        
        public bool SelectWeapon;
        public bool tab;
        public bool isFocus { get; private set; }
        public bool click { get; private set; }
        public bool aimode { get; private set; }
        public bool ToggleTexture { get; private set; }
        public bool SpectatorMode { get; private set; }
        public bool HideCursor { get; private set; }
        public bool LockCameraPosition { get; private set; }
        public bool HideGameHUD { get; private set; }
        public bool HideInfoMenu { get; private set; }

#if !UNITY_IOS || !UNITY_ANDROID
        [Header("Mouse Cursor Settings")]
        public bool cursorInputForLook = true;
#endif
        
        public InputPlayerAssets InputPlayerAssets;
        private GameFlowManager m_GameFlowManager;
        private Health playerHealth;

        private bool IgnorePause;
        private bool StartScene;
        private bool ServerPause = true;
        private bool MenuPause = false;
        private bool HasPause { get { return MenuPause || ServerPause; } }
        
        private static PlayerInputHandler instance;
        public static PlayerInputHandler GetInstance() => instance;

#if ENABLE_INPUT_SYSTEM


        #region MonoBehavior
        private void Awake()
        {
            instance = this;
            InputPlayerAssets = new InputPlayerAssets();
            
            EventManager.AddListener<MenuPauseEvent>(OnMenuPauseEvent);
            EventManager.AddListener<GamePauseEvent>(OnGamePauseEvent);
            EventManager.AddListener<EndSpawnEvent>(OnPlayerSpawnEvent);
        }

        private void Start()
        {
            HideInfoMenu = true;
            MoveState = MoveState.Idle;
            SetCursorState(UnlockedCursor);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<MenuPauseEvent>(OnMenuPauseEvent);
            EventManager.RemoveListener<GamePauseEvent>(OnGamePauseEvent);
            EventManager.RemoveListener<EndSpawnEvent>(OnPlayerSpawnEvent);
        }

        private void OnEnable()
        {
            InputPlayerAssets.Enable();
        }

        private void OnDisable()
        {
            InputPlayerAssets.Disable();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if(hasFocus)ResetCursorPosition();
            isFocus = hasFocus;
            CursorUpdate();
        }

        #endregion
        
        #region EVENT
        private void OnMenuPauseEvent(MenuPauseEvent evt)
        {
            MenuPause = evt.MenuPause;
            CursorUpdate();
        }
        private void OnGamePauseEvent(GamePauseEvent evt)
        {
            //ServerPause = evt.ServerPause;
        }
        private void OnPlayerSpawnEvent(EndSpawnEvent evt)
        {
            playerHealth = evt.LoadManager.ControllerPlayer.Health;
            m_GameFlowManager = evt.LoadManager.GameFlowManager;
            
            m_GameFlowManager.SetInfoMenu(false);
                
            Activate();
        }

        private void Activate()
        {
            InputPlayerAssets.Player.ChangeWeapons.performed += ctx =>
            {
                if (HasPause)
                {
                    int.TryParse(ctx.control.name, out getNumber);
                    WeaponKeycode.number = getNumber;
                }
            };
            
            InputPlayerAssets.Player.Suicide.performed += ctx =>
            {
                playerHealth.TakeDamage(playerHealth.MaxHealth, null);
            };

            InputPlayerAssets.Player.AiMode.performed += ctx =>
            {
                if (HasPause) aimode = !aimode;
            };

            InputPlayerAssets.Player.SpectatorMode.performed += ctx =>
            {
                SpectatorMode = !SpectatorMode;
                m_GameFlowManager.SetActivePlayerHUD(!SpectatorMode);
            };

            InputPlayerAssets.Player.HideInfoMenu.performed += ctx =>
            {
                HideInfoMenu = !HideInfoMenu;
                m_GameFlowManager.SetInfoMenu(m_GameFlowManager.InfoMenu.alpha == 0f);
            };
            InputPlayerAssets.Player.HideCursor.performed += ctx =>
            {
                SetCursorState(!Cursor.visible);
            };
            InputPlayerAssets.Player.StartScene.performed += ctx =>
            {
                if(StartScene == false)EventManager.Broadcast(Events.StartSceneEvent);
                StartScene = true;
            };

            InputPlayerAssets.Player.LockCameraPosition.performed += ctx =>
            {
                if (HasPause) LockCameraPosition = !LockCameraPosition;
            };

            InputPlayerAssets.Player.ToggleTexture.performed += ctx =>
            {
                if (HasPause) ToggleTexture = true;
            };

            InputPlayerAssets.Player.HideGameHUD.performed += ctx =>
            {
                if (HasPause)
                {
                    HideGameHUD = !HideGameHUD;
                    if (m_GameFlowManager)
                    {
                        m_GameFlowManager.SetActivePlayerHUD(!HideGameHUD);
                    }
                }
            };
            ServerPause = false;
        }
        #endregion

        #region ON_INPUT
        
        public void OnSelectWeapon(InputValue value)
        {
            SelectWeaponInput(value.isPressed);
        }

        public void OnTab(InputValue value)
        {
            TabInput(value.isPressed);
        }
        
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }
        public void OnMoveArrows(InputValue value)
        {
            MoveArrowsInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnShoot(InputValue value)
        {
            ShootInput(value.isPressed);
        }

        public void OnAim(InputValue value)
        {
            AimInput(value.isPressed);
        }

        public void OnReload(InputValue value)
        {
            ReloadInput(value.isPressed);
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void OnCrouch(InputValue value)
        {
            CrouchInput(value.isPressed);
        }
        #endregion

#else
	// old input sys if we do decide to have it (most likely wont)...
#endif

        #region INPUT
        
        public void SelectWeaponInput(bool newState)
        {
            SelectWeapon = ServerPause ? false : newState;
        }
        
        public void TabInput(bool newState)
        {
            tab = ServerPause ? false : newState;
        }

        public void MoveArrowsInput(Vector2 newDirection)
        {
            movearrow = HasPause ? Vector2.zero : newDirection;
            WeaponKeycode.shootDirection = movearrow;
        }

        public void LookInput(Vector2 newDirection)
        {
            MoveKeycode.look = HasPause ? Vector2.zero : newDirection;
        }

        public void ShootInput(bool newState)
        {
            bool shoot = HasPause ? false : newState;
            click = newState;
        }

        public void AimInput(bool newState)
        {
            WeaponKeycode.aim = HasPause ? false : newState;
        }

        public void JumpInput(bool newState)
        {
            MoveKeycode.jump = HasPause ? false : newState;
        }

        public void MoveInput(Vector2 newDirection)
        {
            UpdateMoveJoystickUI(newDirection);
            
            MoveKeycode.move = HasPause ? Vector2.zero : newDirection;
            if (MoveKeycode.MoveState == MoveState.Idle)
            {
                MoveKeycode.MoveState = MoveState.Walk;
            }
        }

        public void UpdateMoveJoystickUI(Vector2 direction)
        {
            //if(UIVirtualJoystickMove) UIVirtualJoystickMove.SetHandleRectPosition(direction);
        }

        public void CrouchInput(bool newState)
        {
            bool crouch = HasPause ? false : newState;
            MoveKeycode.MoveState = crouch ? MoveState.Crouch : MoveState.Idle;
        }

        public void SprintInput(bool newState)
        {
            bool sprint = HasPause ? false : newState;
            MoveKeycode.MoveState = sprint ? MoveState.Sprint : MoveState.Idle;
        }

        public void ReloadInput(bool newState)
        {
            WeaponKeycode.reload = HasPause ? false : newState;
        }
#endregion

        #region CURSOR

        public bool IsMouseInGameWindow;// { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }
        private bool isCursorUnlocked => !isFocus || MenuPause;

        private void UpdateMouseOverGameWindow()
        {
            Vector3 mousePos = Input.mousePosition;
            IsMouseInGameWindow = (0 > mousePos.x || 0 > mousePos.y || Screen.width < mousePos.x || Screen.height < mousePos.y);
        }

        private void ResetCursorPosition()
        {
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Mouse.current.WarpCursorPosition(screenCenterPoint);
            InputState.Change(Mouse.current.position, screenCenterPoint);
        }
        
        private void CursorUpdate()
        {
#if PLATFORM_ANDROID == false
            UpdateMouseOverGameWindow();
            SetCursorState(isCursorUnlocked);
#endif
        }

        private void SetCursorState(bool newState)
        {
#if PLATFORM_ANDROID == false
            //Debug.Log("SetCursorState: " + Cursor.visible + "/" + newState);
            Cursor.visible = newState;
            Cursor.lockState = newState ? CursorLockMode.None : CursorLockMode.Locked;
#endif
        }
        
        #endregion
    }
}
public struct MoveKeycode
{
    public MoveState MoveState;
    public bool jump;
    public Vector2 move;
    public Vector2 look;
    public bool RotateInMove;
}
public struct WeaponKeycode
{
    public Vector2 shootDirection;
    public bool reload;
    public int number;
    public bool aim;
}