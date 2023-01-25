using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platinum.CustomEvents;
using Platinum.Info;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Platinum.Level
{
    public class CenterMapCircle : MonoBehaviour
    {
        [Header("General")]
        public float updateInterval = 5f;
        public LayerMask playerCastLayer;

        [Header("References")] 
        public RectTransform rectCircle;
        
        public ActorsManager actorsManager { get; private set; }
        public UnityAction<int> onUpdateTeamScore;
        private float lastTimeUpdate;
        private float circlyRadius;
        private bool endGame;
        private int[] teamsScore;

        private void OnDestroy()
        {
            EventManager.RemoveListener<EndSpawnEvent>(OnEndSpawnEvent);
        }

        private void Awake()
        {
            EventManager.AddListener<EndSpawnEvent>(OnEndSpawnEvent);
            circlyRadius = rectCircle.sizeDelta.x / 2;
        }

        private void Update()
        {
            if (actorsManager == null || lastTimeUpdate > Time.time - updateInterval || endGame) return;

            lastTimeUpdate = Time.time;

            Vector3 pos = transform.position;
            foreach (RaycastHit hit in Physics.SphereCastAll(pos,circlyRadius,pos,circlyRadius,playerCastLayer))
            {
                int teamNumber = hit.collider.GetComponentInParent<Actor>().Info.Affiliation.Number;
                teamsScore[teamNumber] += 1;
            }

            if (teamsScore.Max() == 0) return;
            int winTeam = -1, maxValue = int.MinValue;
            for (int i = 0; i < teamsScore.Length; i++)
            {
                if (teamsScore[i] > maxValue)
                {
                    maxValue = teamsScore[i];
                    winTeam = i;
                    teamsScore[i] = 0;
                }
            }

            actorsManager.AddScoreTeam(winTeam, 1);
            onUpdateTeamScore.Invoke(winTeam);
            
            if (actorsManager.AffiliationsInfo.Any(t => t.Score >= 100f))
            {
                endGame = true;
                WinTeamEvent evt = Events.WinTeamEvent;
                evt.Team = actorsManager.AffiliationsInfo[winTeam].Number;
                EventManager.Broadcast(evt);
            }
        }

        private void OnEndSpawnEvent(EndSpawnEvent evt)
        {
            actorsManager = evt.LoadManager.ActorsManager;
            teamsScore = new int[actorsManager.AffiliationsInfo.Count];
        }
    }
}