using System;
using System.Collections.Generic;
using System.Linq;
using Platinum.Settings;
using UnityEngine;
using Random = UnityEngine.Random;
using Platinum.Settings;

namespace Platinum.Info
{
    public class ActorsManager : MonoBehaviour
    {
        [SerializeField] private Transform[] TeamSpawnpoints;

        public int PlayerAffiliation { get; private set; }

        public List<AffiliationInfo> AffiliationsInfo { get; private set; }
        public Actor[] Actors { get; private set; }
        public Dictionary<int,List<Actor>> ActorsTeam { get; private set; }

        private List<ActorInfo> ListActorsInfo;
        private List<TeamSpawnpoint> ListTeamSpawnpoints;
        private MatchSettings matchInfo;
        private int lastActorID;

        private GameSettings gameSettings;
        private static ActorsManager instance;

        private void Awake()
        {
            instance = this;

            ActorsTeam = new Dictionary<int, List<Actor>>();
            AffiliationsInfo = new List<AffiliationInfo>();
            ListActorsInfo = new List<ActorInfo>();
            ListTeamSpawnpoints = new List<TeamSpawnpoint>();
            foreach (Transform container in TeamSpawnpoints)
            {
                TeamSpawnpoint teamSpawnpoint = new TeamSpawnpoint();
                teamSpawnpoint.SetInfo(container);
                ListTeamSpawnpoints.Add(teamSpawnpoint);
            }
        }

        public void AddScoreTeam(int affiliation, int score)
        {
            AffiliationsInfo[affiliation].AddScore(score);
        }

        public int GetRemainingPlayerEnemy()
        {
            int playerEnemys = 0;
            for (int i = 0; i < AffiliationsInfo.Count; i++)
            {
                if (i == PlayerAffiliation) continue;
                playerEnemys += AffiliationsInfo[i].Members.Count;
            }

            return playerEnemys;
        }

        public static Actor GetRandomEnemyActor(int affiliation)
        {
            Actor[] enemys = instance.Actors.Where(a => a.Info.Affiliation.Number != affiliation).ToArray();
            Actor enemy = enemys[Random.Range(0, enemys.Length)];
            return enemy;
        }

        public static Actor[] GetEnemyActors(int affiliation)
        {
             //Debug.Log($"GetEnemyActors: {instance.Actors[instance.room.GetMaxCountMembers() - 1].Info.Affiliation.Number}");
             return instance.Actors.Where(a => a.Info.Affiliation.Number != affiliation).ToArray();
        }

        private PlayerInfo GetPlayerInfo(Authority authority)
        {
            PlayerInfo newPlayerInfo = gameSettings.PlayerSettings.GeneratePlayerInfo();

            switch (authority)
            {
                case (Authority.OwnerPlayer):
                    newPlayerInfo = gameSettings.PlayerSettings.OwnerPlayerInfo;
                    break;
            }

            return newPlayerInfo;
        }

        private int GetAffiliation()
        {
            return AffiliationsInfo.OrderBy(a => a.Members.Count).FirstOrDefault().Number;

            for (int i = 0; i < AffiliationsInfo.Count; i++)
            {
                if (AffiliationsInfo[i].Members == null || AffiliationsInfo[i].Members.Count < matchInfo.PlayersPerTeam)
                {
                    return i;
                }
            }

            return Random.Range(0, matchInfo.CountTeams);
        }

        public int GetSpawnID(int affiliation) => AffiliationsInfo[affiliation].Members.Count;


        public void AddActor(Actor actor)
        {
            Actors[actor.Info.ActorID] = actor;
            actor.OnRemove += RemoveActor;
            ActorsTeam[actor.Info.Affiliation.Number].Add(actor);
        }
        
        public ActorInfo CreateActorInfo(Authority authority)
        {
            PlayerInfo playerInfo = GetPlayerInfo(authority);
            int affiliation = GetAffiliation();
            if (authority == Authority.OwnerPlayer) PlayerAffiliation = affiliation;

            ActorInfo actorInfo = new ActorInfo();
            actorInfo.SetInfo(playerInfo, AffiliationsInfo[affiliation], lastActorID, GetSpawnID(affiliation), authority);
            
            ListActorsInfo.Add(actorInfo);
            lastActorID++;
            
#if !UNITY_EDITOR
            Debug.Log("Actor Info: " + lastActorID + " / " + playerInfo.Nickname + " / " + actorInfo.Affiliation.Number);
#endif
            
            return actorInfo;
        }

        public void RemoveActor(Actor actor)
        {
#if !UNITY_EDITOR
            Debug.Log("RemoveActor: " + actor.Info.PlayerInfo.Nickname + " | " + actor.Info.ActorID);
#endif
        }

        public void SetSettingsManager(GameSettings gameSettings)
        {
            this.gameSettings = gameSettings;
            matchInfo = this.gameSettings.MatchSettings;
            Actors = new Actor[matchInfo.GetMaxCountMembers()];

            for (int i = 0; i < matchInfo.CountTeams; i++)
            {
                AffiliationInfo team = new AffiliationInfo();
                team.SetOptions(i,ListTeamSpawnpoints[i]);
                AffiliationsInfo.Add(team);
                
                ActorsTeam.Add(i, new List<Actor>());
            }
        }
    }

    public class AffiliationInfo
    {
        public int Number { get; private set; }
        public int Score { get; private set; }
        public TeamSpawnpoint Spawnpoint { get; private set; }
        public List<ActorInfo> Members { get; private set; }

        public void SetOptions(int number, TeamSpawnpoint spawnpoint)
        {
            Number = number;
            Spawnpoint = spawnpoint;
            Members =  new List<ActorInfo>();
        }

        public void AddScore(int score)
        {
            Score += score;
        }

        public void AddMember(ActorInfo member)
        {
            Members.Add(member);
        }
    }

    public class ActorInfo
    {
        public PlayerInfo PlayerInfo { get; private set; }
        public AffiliationInfo Affiliation { get; private set; }
        public int ActorID { get; private set; }
        public int SpawnID { get; private set; }
        public List<int> KillList { get; private set; }
        public Authority Authority { get; private set; }
        public Vector3 SpawnPosition { get; private set; }
        public Quaternion SpawnRotation { get; private set; }
        
        public void SetInfo(PlayerInfo playerInfo, AffiliationInfo affiliation, int actorID, int teamID, Authority authority)
        {
            affiliation.AddMember(this);

            PlayerInfo = playerInfo;
            Affiliation = affiliation;
            ActorID = actorID;
            SpawnID = teamID;
            Authority = authority;
            KillList = new List<int>();

            Transform spawn = Affiliation.Spawnpoint.GetPoint(teamID);
            SpawnPosition = spawn.position;
            SpawnRotation = spawn.rotation;
        }
        public void AddKill(int actorID)
        {
            KillList.Add(actorID);
        }
    }

    public struct TeamSpawnpoint
    {
        public Transform Container { get; private set; }
        public List<Transform> Points { get; private set; }

        public Transform GetPoint(int teamID)
        {
            if (teamID >= Points.Count) teamID = Random.Range(0, Points.Count);
            return Points[teamID];
        }
        
        public void SetInfo(Transform container)
        {
            Container = container;
            Points = new List<Transform>();
            for (int i = 0; i < container.childCount; i++)
            {
                Points.Add(container.GetChild(i));
            }
        }
    }
}

