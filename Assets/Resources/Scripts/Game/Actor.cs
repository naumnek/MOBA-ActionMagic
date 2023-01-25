using System;
using Platinum.Controller;
using Platinum.CustomEvents;
using UnityEngine;
using UnityEngine.Events;

namespace Platinum.Info
{
    // This class contains general information describing an actor (player or enemies).
    // It is mostly used for AI detection logic and determining if an actor is friend or foe
    
    public class Actor : MonoBehaviour
    {
        [Header("General")]
        public Transform AimPoint;

        //public string[] ListNickName = new string[] {"Alex", "Steve", "Aloha", "LegalDepartment", "Bot1234" };

        public ActorInfo Info { get; private set; }
        
        public UnityAction<Actor> OnRemove;
        public ControllerBase ControllerBase { get; private set; }

        private void Awake()
        {
            ControllerBase = GetComponent<ControllerBase>();
            
            EventManager.AddListener<KillEvent>(OnKillEvent);
        }
        
        void OnDestroy()
        {
            OnRemove?.Invoke(this);
            EventManager.RemoveListener<KillEvent>(OnKillEvent);
        }

        private void OnKillEvent(KillEvent evt)
        {
            if (evt.killer.Info.ActorID == Info.ActorID)
            {
                Info.AddKill(evt.killed.Info.ActorID);
            }
        }

        public void InitializeActor(ActorInfo newInfo)
        {
            if (Info != null)
            {
                Debug.LogError($"Вы пытаетесь присвоить {Info.PlayerInfo.Nickname} данные {newInfo.PlayerInfo.Nickname}");
                return;
            }
            Info = newInfo;
        }
    }
}