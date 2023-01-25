using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Platinum.Info;
using Platinum.CustomEvents;
using Platinum.Controller.AI;
using Random = UnityEngine.Random;

namespace Platinum.Controller
{
    public class ControllerBot : ControllerBase
    {
        [Header("General")] 
        public bool ForceAgentMovement;
        public LayerMask GroundCastLayer;
        
        [Header("References")] 
        public NavMeshAgent PrefabNavMeshAgent;

        [Header("AI")] 
        public Vector2 randomAttackDelay;
        
        [Header("NavAgent Jump")]
        public float ObstacleOffsetY = 0.75f;
        public float ObstacleCastLength = 3f;
        public float RadiusActiveOffMeshLink = 5f;
        
        [Header("NavAgent Move")]
        public float OrientationSpeed = 30f;
        public float AgentRangeActive = 5f;
        public float AgentVelocityMagnitude = 1f;
        public float RadiusNavRandomSearch = 10.0f;
        
        private NavMeshAgent navMeshAgent;
        private Transform AgentTransform;
        private Transform BodyTransform;
        private Vector3 offMeshLinkPosition;
        private Vector3 normalObstaclePosition;
        private float agentDistance;
        
        private int pathNumber = -1;
        private Transform PatrolsContainer;
        private List<Transform> Patrols;
        
        public AiModule AiModule { get; private set; }
        
        private ControllerCommand currentCommand;
        private float lastDelayAttack;


        private void Awake()
        {
            base.Awake();
            
            AiModule = GetComponent<AiModule>();
            
            Patrols = new List<Transform>();
            if (PatrolsContainer)
            {
                for (int i = 0; i < PatrolsContainer.childCount; i++)
                {
                    Patrols.Add(PatrolsContainer.GetChild(i));
                }
            }
            
            AiModule.sendControllerCommand += NewControllerState;
        }

        private void Start()
        {
            BodyTransform = Movement.Body;
            
            navMeshAgent = Instantiate(PrefabNavMeshAgent, BodyTransform.position, BodyTransform.rotation, this.transform);
            AgentTransform = navMeshAgent.transform;
            navMeshAgent.angularSpeed = OrientationSpeed * 10f;
        }
        
        protected override void Update()
        {
            UpdateControllerState();
            UpdateMovement();
            
            base.Update();
        }
        
        private void UpdateControllerState()
        {
            
            switch (currentCommand.State)
            {
                case(ControllerState.Patrol):
                    break;
                case(ControllerState.Follow):
                    if (currentCommand.Target.isActive)
                    {
                        SetNavDestination(currentCommand.Target.targetPosition);
                    }
                    else
                    {
                        SetNavDestination(BodyTransform.position);
                    }
                    break;
                case(ControllerState.Attack):
                    Vector3 heading = currentCommand.Target.targetPosition - BodyTransform.position;
                    SetNavDestination(heading);
                    // This is the overground heading.
                    //Vector2 direction = DirectionToTarget(currentCommand.Target.Position);
                    //moveKeycode.look = new Vector2(heading.x, heading.z);
                    lastDelayAttack -= Time.deltaTime;
                    Vector2 shootDirection = new Vector2(heading.x, heading.z);
                    moveKeycode.look = shootDirection;
                    if (lastDelayAttack < 0 && currentCommand.Target.isActive)
                    {
                        lastDelayAttack = Arsenal.activeWeapon.DelayBetweenShots + Random.Range(randomAttackDelay.x, randomAttackDelay.y);
                    }
                    else
                    {     
                        shootDirection = Vector2.zero;
                    }
                    weaponKeycode.shootDirection = shootDirection;
                    break;
            }
        }

        private void UpdateMovement()
        {
            agentDistance = Vector3.Distance(AgentTransform.position, BodyTransform.position);
            bool IsActiveAgent = agentDistance <= AgentRangeActive && Movement.CharacterController.isGrounded;

            bool isMove;
            if (ForceAgentMovement)
            {
                BodyTransform.position = AgentTransform.position;
            }
            else
            {
                navMeshAgent.enabled = IsActiveAgent;
                if (!IsActiveAgent)
                {
                    AgentTransform.position = BodyTransform.position;
                }
            }
            
            isMove = navMeshAgent.velocity.magnitude >= AgentVelocityMagnitude;
            moveKeycode.move.x = isMove ? navMeshAgent.velocity.x : 0;
            moveKeycode.move.y = isMove ? navMeshAgent.velocity.z : 0;
            moveKeycode.MoveState = isMove ? MoveState.Walk : MoveState.Idle;

            if (navMeshAgent.isOnOffMeshLink)
            {
                if (IsObstacleToDirection(Vector3.forward * ObstacleCastLength).collider)
                {
                    float linkDistance = Vector3.Distance(offMeshLinkPosition, navMeshAgent.transform.position);
                    moveKeycode.jump = linkDistance < RadiusActiveOffMeshLink;
                }
            }
        }

        private void NewControllerState(ControllerCommand newCommand)
        {
            currentCommand = newCommand;
            Debug.Log("currentCommand: " + currentCommand.Target.targetPosition + " | " + currentCommand.State);
            //Debug.Log("NewControllerState: " + newCommand.State);
        }

        public Vector2 DirectionToTarget(Vector3 lookPosition)
        {
            
            Quaternion targetRotation = BodyTransform.rotation;

            Vector3 lookDirection = Vector3.ProjectOnPlane(lookPosition - BodyTransform.position, Vector3.up).normalized;
            if (lookDirection.sqrMagnitude != 0f)
            {
                targetRotation = Quaternion.LookRotation(lookDirection);
                return targetRotation.eulerAngles;
            }
            return Vector2.zero;
        }
        
        public RaycastHit IsObstacleToDirection(Vector3 direction)
        {
            Vector3 startPos = Movement.Body.position;
            Vector3 endPos = Movement.Body.TransformPoint(direction);
            startPos.y += ObstacleOffsetY;
            endPos.y += ObstacleOffsetY;
            RaycastHit castInfo = RaycastGround(startPos, endPos);
            Debug.DrawLine(startPos, endPos, castInfo.collider != null ? Color.blue : Color.cyan, 0.05f, false);
            return castInfo;
        }
        
        private RaycastHit RaycastGround(Vector3 origin, Vector3 target)
        {
            if (Physics.Linecast(origin, target, out RaycastHit cast, GroundCastLayer))
            {
                return cast;
            }
            return new RaycastHit();
        }

        public void SetNavDestination(Vector3 destination)
        {
            if (navMeshAgent && navMeshAgent.enabled)
            {
                navMeshAgent.SetDestination(destination);
            }
        }

        protected override void OnDie()
        {
            base.OnDie();
            currentCommand = new ControllerCommand();
        }
        protected override void OnRespawn()
        {
            base.OnRespawn();
            controllable = !matchSettings.PeacifulMode;
        }
    }
}

                
// Direction from point 1 to point 2
//Vector3 direction = (enemyPosition - MovementBot.CharacterBody.position).normalized;
                
// Rotation to look at point 2
//Quaternion newRotation = Quaternion.LookRotation(direction);
//MovementBot.UpdateRotation(newRotation);
                
/*
Quaternion targetRotation = CharacterBody.rotation;
Vector3 lookDirection = Vector3.ProjectOnPlane(lookPosition - CharacterBody.position, Vector3.up).normalized;
if (lookDirection.sqrMagnitude != 0f)
{
    targetRotation = Quaternion.LookRotation(lookDirection);
}
float yawLook = targetRotation.eulerAngles.y;
*/