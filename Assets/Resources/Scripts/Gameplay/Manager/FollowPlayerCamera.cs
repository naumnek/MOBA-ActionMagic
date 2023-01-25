using Cinemachine;
using Platinum.Settings;
using UnityEngine;
using System.Collections;
using Platinum.CustomInput;
using Platinum.Controller;
using Platinum.Scene;
using Platinum.CustomEvents;

namespace Platinum.Player
{
    public class FollowPlayerCamera : MonoBehaviour
    {
        public Transform cameraTarget;
        public Camera ScreenSpaceCamera;
        public Camera MainCamera;
        public Camera FreeFlyCamera;
        public CinemachineVirtualCamera PlayerFollowCamera;
        public CinemachineFreeLook PlayerLockedCamera;
        
        //references
        private ControllerBase m_ControllerPlayer;
        private Transform m_FollowCameraTarget;
        private bool IsRefreshMatch;
        private CinemachineFramingTransposer m_ThirdPersonFollow;
        private GameSettings gameSettings;

        private void OnDestroy()
        {
            EventManager.RemoveListener<EndSpawnEvent>(OnEndSpawnEvent);
            //EventManager.RemoveListener<RefreshMatchEvent>(OnRefreshMatchEvent);
        }

        private void Awake()
        {
            EventManager.AddListener<EndSpawnEvent>(OnEndSpawnEvent);
            m_ThirdPersonFollow = PlayerFollowCamera.GetComponentInChildren<CinemachineFramingTransposer>();
            //Debug.Log("m_ThirdPersonFollow: " + m_ThirdPersonFollow.m_CameraDistance);
        }

        private void OnEndSpawnEvent(EndSpawnEvent evt)
        {
            gameSettings = evt.LoadManager.gameSettings;
            
            //m_CameraTarget.SetParent(m_PlayerBody);

            m_ControllerPlayer = evt.LoadManager.ControllerPlayer;
            m_ControllerPlayer.onDie += OnPlayerDie;
            m_ControllerPlayer.onRespawn += OnPlayerRespawn;

            OnPlayerRespawn();
            
            PlayerFollowCamera.Follow = cameraTarget;
            PlayerFollowCamera.LookAt = cameraTarget;
            PlayerLockedCamera.Follow = cameraTarget;
            PlayerLockedCamera.LookAt = cameraTarget;

            SetCrosshairPosition(gameSettings.PlayerSettings.PlayerSaves.CrosshairPosition);
        }

        private void LateUpdate()
        {
            //ScreenSpaceCamera.transform.position = MainCamera.transform.position;
            
            if (cameraTarget == null) return;
            //FlyCamera();
        }

        private void OnPlayerDie()
        {
            cameraTarget.SetParent(null);
        }

        private void OnPlayerRespawn()
        {
            Transform body = m_ControllerPlayer.Movement.Body;
            cameraTarget.position = body.position;
            cameraTarget.SetParent(body);
        }
        
        private void FlyCamera()
        {
            Vector3 bodyPosition = m_ControllerPlayer.Movement.Body.position;
            bodyPosition.y = 0;
            cameraTarget.position = bodyPosition;
        }

        public void SerCameraDistance(float distance)
        {
            //m_FramingFollowCamera.m_CameraDistance = distance / 10;
            if(m_ThirdPersonFollow)m_ThirdPersonFollow.m_CameraDistance = distance;
        }

        public void SetCrosshairPosition(Vector2 newPosition)
        {
            //m_CameraTarget.position = StartCameraTargetPosition * newPosition / 10;
        }

        public void SetFreeCamera(bool active)
        {
            FreeFlyCamera.enabled = active;
        }
    }
}