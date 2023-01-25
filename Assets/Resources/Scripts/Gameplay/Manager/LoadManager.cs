using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Platinum.Controller;
using Platinum.Settings;
using Platinum.CustomInput;
using Platinum.Player;
using Platinum.Info;
using Platinum.UI;
using Platinum.CustomEvents;
using Cinemachine;
using Random = UnityEngine.Random;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public class DetectPlayModeChanges
{

    static DetectPlayModeChanges()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
                // Do whatever before entering play mode
                break;
            case PlayModeStateChange.EnteredPlayMode:
                // Do whatever after entering play mode
                break;
            case PlayModeStateChange.ExitingPlayMode:
                PlayerPrefs.DeleteAll();
                // Do whatever before returning to edit mode
                break;
            case PlayModeStateChange.EnteredEditMode:
                // Do whatever after returning to edit mode
                break;
        }
    }
}
#endif
namespace Platinum.Scene
{
    public class LoadManager : MonoBehaviour
    {
        [Header("Mode")]
        public TypeLevel TypeLevel = TypeLevel.Test;
        public EndMatch endMatch = EndMatch.Instance;
        
        [Header("Settings")]
        public GameSettings gameSettings;

        public Transform centerMap;
        /// <summary>
        /// LevelGenerator seed
        /// </summary>
        public int Seed = 0;
        public FollowPlayerCamera FollowPlayerCamera { get; private set; }
        public PlayerInputHandler PlayerInputHandler { get; private set; }
        public GameFlowManager GameFlowManager { get; private set; }
        public ActorsManager ActorsManager { get; private set; }
        public ControllerBase ControllerPlayer { get; private set; }
        
        private LoadSceneManager loadSceneManager;
        public int LevelSeed { get; private set; }
        public MatchResult matchResult { get; private set; }
        private bool endSpawnBots;
        private bool endSpawnPlayer;
        private int EndMatchCount;
        private string WinnerName = "naumnek";
        private Actor DieActor;
        private Actor[] FoundActors;
        private static LoadManager instance;

        public static GameSettings GetGameSettings()
        {
            return instance.gameSettings;
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<EndScreenUnvisibility>(OnEndScreenUnvisibility);
            EventManager.RemoveListener<EndSceneEvent>(OnEndLevelEvent);
            EventManager.RemoveListener<StartGenerationEvent>(OnStartGenerationEvent);
            EventManager.RemoveListener<WinTeamEvent>(OnWinTeamEvent);
        }
        
        private void Awake()
        {
            instance = this;
            matchResult = MatchResult.None;
            
#if !UNITY_EDITOR
            Debug.Log("Name game: " + Application.productName);
            Debug.Log("Version game: " + Application.version);
            Debug.Log("Dev by: naumnek and " + Application.companyName);
#endif
            
            gameSettings.ItemSettings.ResetAllItemInfo();
            
            EventManager.AddListener<EndScreenUnvisibility>(OnEndScreenUnvisibility);
            EventManager.AddListener<EndSceneEvent>(OnEndLevelEvent);
            EventManager.AddListener<StartGenerationEvent>(OnStartGenerationEvent);
            EventManager.AddListener<WinTeamEvent>(OnWinTeamEvent);
            
            FollowPlayerCamera = FindObjectOfType<FollowPlayerCamera>();
            PlayerInputHandler = FindObjectOfType<PlayerInputHandler>();
            GameFlowManager = FindObjectOfType<GameFlowManager>();
            ActorsManager = FindObjectOfType<ActorsManager>();
            loadSceneManager = FindObjectOfType<LoadSceneManager>();
            ActorsManager.SetSettingsManager(gameSettings);
        }

        private void Start()
        {
            FindLoadSceneManager();
        }

        private void OnEndScreenUnvisibility(EndScreenUnvisibility evt)
        {
            SetMode();
        }

        private void FindLoadSceneManager()
        {
            if (loadSceneManager)
            {
                Debug.Log("Finish load scene " + loadSceneManager.currentScene);
            }
            else
            {
                gameSettings.PlayerSettings.CreateOwnerInfo();
                Debug.Log("Not found SceneController from scene: Auto start game");
                //Invoke(nameof(SetMode), 2f);
                SetMode();
            }
        }

        private void SetMode()
        {

            switch (TypeLevel)
            {
                case (TypeLevel.Moba):
                    SpawnActors();
                    FindActors();
                    break;
            }
        }


        private void EndSpawnActors()
        {
            if (FoundActors.All(a => a.Info != null))
            {
                EndSpawnEvent evt = Events.EndSpawnEvent;
                evt.LoadManager = this;
                EventManager.Broadcast(evt); 
            }
            else
            {
                Debug.LogError("Not initialize all actors");
            }
        }

        private void FindActors()
        {
            FoundActors = FindObjectsOfType<Actor>();
            
            EndSpawnActors();
        }
        private void SpawnActors()
        {
            foreach (MembersList list in gameSettings.MatchSettings.MembersList)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    ActorInfo actorInfo = ActorsManager.CreateActorInfo(list.Authority);
                    GameObject obj = Instantiate(list.Prefab, actorInfo.SpawnPosition, actorInfo.SpawnRotation);
                    Actor actor = obj.GetComponent<Actor>();
                    
                    switch (actorInfo.Authority)
                    {
                        case(Authority.OwnerPlayer):
                            ControllerPlayer = obj.GetComponent<ControllerPlayer>();
                            break;
                        default:
                            //controller = obj.GetComponent<ControllerBot>();
                            break;
                    }
                    
                    actor.InitializeActor(actorInfo);
                    ActorsManager.AddActor(actor);
                }
            }
        }
        
        private void SpawnBotNearestWaypoint(Vector3 target, float radius)
        {
            //if (waitMoveNav == false) return;
            /*
            Vector3 point = target + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                SpawnBot(hit.position);
            }
            */
        }

        public void OnWinTeamEvent(WinTeamEvent evt)
        {
            //WinnerName = winActor.Info.PlayerInfo.Nickname;

            if (TypeLevel != TypeLevel.Moba)
            {
                DisplayMessageEvent displayMessage = Events.DisplayMessageEvent;
                displayMessage.Message = "Winner: " + WinnerName;
                EventManager.Broadcast(displayMessage);
            }

            if (ActorsManager.AffiliationsInfo.Any(t => t.Score >= 100f))
            {
                StartCoroutine(EndGameStart(evt.Team));
            }
            else
            {
                StartCoroutine(EndMatchResult());
            }
        }
        
        IEnumerator EndGameStart(int winTeam)
        {
            GamePauseEvent gpe = Events.GamePauseEvent;
            gpe.ServerPause = true;
            EventManager.Broadcast(gpe);
                
            bool isUserWin = winTeam == ControllerPlayer.Actor.Info.Affiliation.Number;
            matchResult = isUserWin ? MatchResult.Win : MatchResult.Lose;
            int result = GetMatchResult(matchResult);
            UserStatistics statistics = new UserStatistics();
            statistics.MatchResult = matchResult;
            statistics.killing = ControllerPlayer.Actor.Info.KillList.Count;
            statistics.score = (result + (statistics.killing * gameSettings.AwardsInfo.ScoreForKill));
            statistics.coins = statistics.killing * gameSettings.AwardsInfo.ScoreForKill;
                
            gameSettings.PlayerSettings.AddCoin(statistics.coins);
            gameSettings.PlayerSettings.AddScore(statistics.score);
            gameSettings.PlayerSettings.AddKillAmount(statistics.killing);
            
            yield return new WaitForSeconds(1f);

            GameFlowManager.ResultEndGame(statistics);
        }

        private int GetMatchResult(MatchResult matchResult)
        {
            switch (matchResult)
            {
                case (MatchResult.Win): return gameSettings.AwardsInfo.ScoreForWin;
                case (MatchResult.Lose): return gameSettings.AwardsInfo.ScoreForLose;
                case (MatchResult.None): return gameSettings.AwardsInfo.ScoreForExit;
            }
            return 0;
        }

        private IEnumerator EndMatchResult()
        {
            GameFlowManager.EndMatchEffect();
            StartCoroutine(InitRefreshMatch());
            if (endMatch != EndMatch.Instance)
            {
                yield return new WaitForSeconds(GameFlowManager.EndMatchAlphaDelay * 2);
            }
            DieActor = null;
        }
        
        IEnumerator InitRefreshMatch()
        {
            if (DieActor == null) yield break;
            Debug.Log("InitRefreshMatch");

            yield return new WaitForSeconds(1f + GameFlowManager.EndMatchAlphaDelay);
            EventManager.Broadcast(Events.RefreshMatchEvent);
        }

        public void OnStartGenerationEvent(StartGenerationEvent evt)
        {
            Seed = evt.Seed;
        }

        public void OnEndLevelEvent(EndSceneEvent evt)
        {
            if (loadSceneManager) return;
            
            switch (evt.State)
            {
                case(EndLevelState.LoadMenu):
                    UnityEngine.SceneManagement.SceneManager.LoadScene(gameSettings.MatchSettings.MainMenuBuildIndex);
                    break;
                default:
                    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
                    break;
            }
        }
    }
}

public struct UserStatistics
{
    public MatchResult MatchResult;
    public int killing;
    public int score;
    public int coins;
}

public enum MatchResult
{
    Win,
    Lose,
    None,
}

public enum TypeLevel
{
    Menu,
    Arena,
    Moba,
    Test,
}
    
public enum EndMatch
{
    Effect,
    Instance,
}
