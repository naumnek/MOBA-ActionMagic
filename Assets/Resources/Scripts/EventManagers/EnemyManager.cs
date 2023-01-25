using System.Collections.Generic;
using UnityEngine;
using Platinum.Controller;

namespace Platinum.Info
{
    public class EnemyManager : MonoBehaviour
    {
        public List<ControllerBot> Enemies { get; private set; }
        public int NumberOfEnemiesTotal { get; private set; }
        public int NumberOfEnemiesRemaining => Enemies.Count;
        private static EnemyManager instance;

        public static EnemyManager GetInstance() => instance;

        void Awake()
        {
            instance = this;
            Enemies = new List<ControllerBot>();
        }

        public void RegisterEnemy(ControllerBot enemy)
        {
            Enemies.Add(enemy);

            NumberOfEnemiesTotal++;
        }

        public void UnregisterEnemy(ControllerBot killed)
        {
            int enemiesRemainingNotification = NumberOfEnemiesRemaining - 1;
            //enemyKilled.SpawnSection.OnEnemyInSectionKill(enemyKilled.transform);

            // removes the enemy from the list, so that we can keep track of how many are left on the map
            Enemies.Remove(killed);
        }

        private void OnDestroy()
        {

        }
    }
}