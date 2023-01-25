using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Platinum.Controller.AI;
using UnityEngine;
using UnityEngine.Events;
using Platinum.Info;
using Platinum.Weapon;
using Random = UnityEngine.Random;

namespace Platinum.Controller.AI
{
    public class AiModule : MonoBehaviour
    {
        [SerializeField] bool DisablePatrol = true;
        [SerializeField] float updateCommand = 0.5f;
        [SerializeField] float delayLostCommand = 5f;
        
        public UnityAction<ControllerCommand> sendControllerCommand;
        
        public DetectionModule DetectionModule { get; private set; }
        private ControllerBot ControllerBot;

        private int indexCurrentCommand;
        private List<ControllerCommand> controllerCommands;
        private List<int> tempCommandsID;
        private List<int> staticCommandsID;
        private float timeFindTarget;

        private bool activate;
        private float lastTimeUpdateCommand;
        private ControllerCommand lastSendControllerCommand;

        private void Awake()
        {
            controllerCommands = new List<ControllerCommand>();
            tempCommandsID = new List<int>();
            staticCommandsID = new List<int>();
            
            ControllerBot = GetComponent<ControllerBot>();
            DetectionModule = GetComponent<DetectionModule>();
            
            DetectionModule.onDetectedTarget += OnDetectedTarget;
            ControllerBot.onDamaged += OnDamaged;
            ControllerBot.onRespawn += OnRespawn;
            ControllerBot.onDie += OnDie;
        }

        private void Update()
        {
            if (lastTimeUpdateCommand + updateCommand <= Time.time)
            {
                TimeLostCommand();
            }
        }

        private void TimeLostCommand()
        {
            lastTimeUpdateCommand = Time.time;
            for(int i = 0; i < tempCommandsID.Count; i++)
            {
                int id = tempCommandsID[i];
                ControllerCommand command = controllerCommands.Find(c => c.ID == id);
                
                if (command.time + delayLostCommand < Time.time || !command.Target.isActive)
                {
                    RemoveCommand(id);
                }
            }
        }

        private void RemoveCommand(int id)
        {
            ControllerCommand command = controllerCommands.Find(c => c.ID == id);

            switch (command.Type)
            {
                case TypeCommand.Static:
                    staticCommandsID.Remove(id);
                    break;
                case TypeCommand.Time:
                    tempCommandsID.Remove(id);
                    break;
            }
            controllerCommands.Remove(command);
            
            if (id == lastSendControllerCommand.ID)
            {
                SendControllerCommand();
            }
        }

        private void SendControllerCommand()
        {
            if (controllerCommands.Count == 0) return;
            lastSendControllerCommand = controllerCommands.OrderBy(x => x.priority).Last();
            sendControllerCommand?.Invoke(lastSendControllerCommand);
        }

        private void CreateControllerCommand(ControllerCommand newCommand)
        {
            ControllerCommand[] cloneCommands = controllerCommands.Where(c => IsTarget(c.Target, newCommand.Target)).ToArray();
            if (cloneCommands.Any(c => c.Target.TypeRange == newCommand.Target.TypeRange)) return;
                    
            Debug.Log("target: " + newCommand.Target.TypeRange);
            
            ControllerCommand[] copyCommandsPriority = cloneCommands.Where
                (c => c.State != newCommand.State || c.priority < newCommand.priority).ToArray();
            
            foreach (ControllerCommand command in copyCommandsPriority)
            {
                Debug.Log("Remove: " + command.State + " | " + command.priority); 
                RemoveCommand(command.ID);
            }
            
            Debug.Log("target: " + newCommand.Target.TypeRange);
            newCommand.time = Time.time;
            newCommand.ID = controllerCommands.Count == 0 ? 0 : controllerCommands.Max(c => c.ID) + 1;
            //newCommand.ID = controllerCommands.Count;
            controllerCommands.Add(newCommand);
            
            switch (newCommand.Type)
            {
                case TypeCommand.Time:
                    tempCommandsID.Add(newCommand.ID);
                    break;
                case TypeCommand.Static:
                    staticCommandsID.Add(newCommand.ID);
                    break;
            }

            if (lastSendControllerCommand.priority >= newCommand.priority) return;
            SendControllerCommand();
        }

        private void ReactionToTarget(DetectionTarget target)
        {
            //Debug.Log("CheckTarget: " + detection.Position);

            ControllerCommand newCommand = new ControllerCommand();
            newCommand.Target = target;
            newCommand.Type = TypeCommand.Time;
            
            switch (target.TypeTarget)
            {
                case TypeTarget.Enemy:
                    ReactionToEnemy(target,ref newCommand);
                    break;
                case TypeTarget.Place:
                    ReactionToPlace(target,ref newCommand);
                    break;
            }
            CreateControllerCommand(newCommand);
        }
        
        private bool IsTarget(DetectionTarget target, DetectionTarget newTarget)
        {
            //int targetID = target.TargetEnemy.actor.Info.ActorID;
            //int newTargetID = newTarget.TargetEnemy.actor.Info.ActorID;
            return target.TypeTarget == newTarget.TypeTarget && target.ID == newTarget.ID;
        }

        private void ReactionToPlace(DetectionTarget detection, ref ControllerCommand command)
        {
            switch (detection.TypeRange)
            {
                case TypeRange.Mind:
                    command.Type = TypeCommand.Static;
                    command.State = ControllerState.Follow;
                    command.priority = 1;
                    break;
                default:
                    return;
            }
            
        }
        
        private Vector3 GetRandomPosition(Vector3 target)
        {
            float range = ControllerBot.Arsenal.activeWeapon.bulletPrefab.projectilePrefab.MaxShotDistance;
            range = range - (range / 4);
            Vector3 rX = Vector3.right * Random.Range(-range,range);
            Vector3 rZ = Vector3.forward * Random.Range(-range,range);

            return target + rX + rZ;
        }
        
        private void ReactionToEnemy(DetectionTarget detection, ref ControllerCommand command)
        { 
            switch (detection.TypeRange)
            {
                case TypeRange.Mind:
                    command.Type = TypeCommand.Static;
                    command.State = ControllerState.Follow;
                    command.priority = 1;
                    break;
                case TypeRange.Random:
                case TypeRange.Sight:
                    command.State = ControllerState.Follow;
                    command.priority = 2;
                    break;
                case TypeRange.Attack:
                    command.State = ControllerState.Attack;
                    command.priority = 3;
                    break;
                default:
                    return;
            }
        }

        protected void OnDie()
        {
            Debug.Log("AiModule: Die");
            controllerCommands.Clear();
            tempCommandsID.Clear();
            staticCommandsID.Clear();
            lastSendControllerCommand = new ControllerCommand();
        }
        
        protected void OnRespawn()
        {
            DetectionTarget detection = new DetectionTarget();
            detection.placePosition = ControllerBot.centerMapPoint.position;
            detection.TypeTarget = TypeTarget.Place;
            detection.TypeRange = TypeRange.Mind;
            ReactionToTarget(detection);
        }

        private void OnDetectedTarget(DetectionTarget newDetectionTarget)
        {
            ReactionToTarget(newDetectionTarget);
        }

        private void OnDamaged(float amount, Actor actor) { }
        }
}

public struct ControllerCommand
{
    public DetectionTarget Target;
    public TypeCommand Type;
    public ControllerState State;
    public int priority;
    public float time;
    public int ID;
}

public enum TypeCommand
{
    Time,
    Static
}
public enum ControllerState
{
    None,
    Patrol,
    Follow,
    Attack,
}
public enum TypeTarget
{
    Enemy,
    Item,
    Place
}

public enum TypeRange
{
    Random,
    Mind,
    Sight,
    Attack
}

