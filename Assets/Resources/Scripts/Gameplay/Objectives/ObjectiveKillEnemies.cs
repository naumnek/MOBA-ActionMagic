using System;
using UnityEngine;
using Platinum.Info;

namespace Platinum.CustomEvents
{
    public class ObjectiveKillEnemies : Objective
    {
        [Tooltip("Chose whether you need to kill every enemies or only a minimum amount")]
        public bool MustKillAllEnemies = true;

        [Tooltip("If MustKillAllEnemies is false, this is the amount of enemy kills required")]
        public int KillsToCompleteObjective = 5;

        [Tooltip("Start sending notification about remaining enemies when this amount of enemies is left")]
        public int NotificationEnemiesRemainingThreshold = 3;

        int m_KillTotal;
        private ActorsManager m_ActorsManager;

        private void Awake()
        {
            m_ActorsManager = FindObjectOfType<ActorsManager>();
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<DieEvent>(OnDieEvent);
        }
        
        protected override void Start()
        {
            base.Start();

            EventManager.AddListener<DieEvent>(OnDieEvent);

            // set a title and description specific for this type of objective, if it hasn't one
            if (string.IsNullOrEmpty(Title))
                Title = "Eliminate " + (MustKillAllEnemies ? "all the" : KillsToCompleteObjective.ToString()) +
                        " enemies";

            if (string.IsNullOrEmpty(Description))
                Description = GetUpdatedCounterAmount();
        }

        void OnDieEvent(DieEvent evt)
        {
            if (IsCompleted) return;

            m_KillTotal++;
            int remainingEnemyCount = m_ActorsManager.GetRemainingPlayerEnemy();

            if (MustKillAllEnemies)
                KillsToCompleteObjective = remainingEnemyCount + m_KillTotal;

            print("Убить: " + KillsToCompleteObjective);
            int targetRemaining = MustKillAllEnemies ? remainingEnemyCount : KillsToCompleteObjective - m_KillTotal;
            print("Моб убит: " + targetRemaining);

            // update the objective text according to how many enemies remain to kill
            if (targetRemaining == 0)
            {
                CompleteObjective(string.Empty, GetUpdatedCounterAmount(), "Objective complete : " + Title);
            }
            else if (targetRemaining == 1)
            {
                string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                    ? "One enemy left"
                    : string.Empty;
                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
            else
            {
                // create a notification text if needed, if it stays empty, the notification will not be created
                string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                    ? targetRemaining + " enemies to kill left"
                    : string.Empty;

                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
        }

        string GetUpdatedCounterAmount()
        {
            return m_KillTotal + " / " + KillsToCompleteObjective;
        }
    }
}