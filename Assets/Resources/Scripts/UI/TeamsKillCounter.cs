using System;
using System.Collections.Generic;
using TMPro;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;
using Platinum.CustomEvents;
using Platinum.Info;

namespace Platinum.UI
{
    public struct KillCounterUI
    {
        public TMP_Text TeamnameText;
        public TMP_Text Counter;
        public Color NotificationColor;
    }
    
    public class TeamsKillCounter : MonoBehaviour
    {
        public NotificationHUDManager NotificationHUDManager;

        [Tooltip("The text field displaying the team counter")]
        public TMP_Text PlayerNickname;
        public Slider FriendlyKillCounter;
        public Color FriendlyTeamNotificationColor;
        public Color EnemyTeamNotificationColor;

        private List<KillCounterUI> KillCounters;
        private ActorsManager m_ActorsManager;

        private void OnDestroy()
        {
            EventManager.RemoveListener<DieEvent>(OnDieEvent);
            EventManager.RemoveListener<EndSpawnEvent>(OnEndSpawnEvent);
            EventManager.RemoveListener<RefreshMatchEvent>(OnRefreshMatchEvent);
        }

        private void Awake()
        {
            KillCounters = new List<KillCounterUI> { };
            EventManager.AddListener<DieEvent>(OnDieEvent);
            EventManager.AddListener<EndSpawnEvent>(OnEndSpawnEvent);
            EventManager.AddListener<RefreshMatchEvent>(OnRefreshMatchEvent);
        }
        private void OnRefreshMatchEvent(RefreshMatchEvent evt)
        {
            //m_DieActor = null;
        }

        private void OnEndSpawnEvent(EndSpawnEvent evt)
        {
            PlayerNickname.text = evt.LoadManager.ControllerPlayer.Actor.Info.PlayerInfo.Nickname;
            m_ActorsManager = evt.LoadManager.ActorsManager;
            //Fix int countTeams = PhotonNetwork.CurrentRoom.CountTeams + 1;
            /*for (int i = 0; i < m_LoadManager.SettingsManager.RequredRoom.CountTeams; i++)
            {
                KillCounters.Add();
            }*/
        }
        
        public void OnDieEvent(DieEvent evt)
        {
/*
            Actor lastActor = evt.LastHitActor;

            int team = lastActor.Info.Affiliation;
            TeamsKillScores[team] += 1;
            FriendlyKillCounter.value = TeamsKillScores[FriendlyAffiliation];
            EnemyKillCounter.value = TeamsKillScores[EnemyAffiliation];
*/
            /*
            if(team == FriendlyAffiliation)
                FriendlyCounter.text = TeamsKillScores[team].ToString();         
            else
                EnemyCounter.text = TeamsKillScores[team].ToString();

            NotificationHUDManager.OnTeamsKill(evt.killed.NickName + " killed by " + evt.killer.NickName,
                team == FriendlyAffiliation ? FriendlyTeamNotificationColor : EnemyTeamNotificationColor);
            */
        }
    }
}
