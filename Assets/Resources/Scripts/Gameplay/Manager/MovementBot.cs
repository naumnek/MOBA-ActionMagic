using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Platinum.AI;

namespace Platinum.Controller
{
    public class MovementBot : MonoBehaviour
    {
        [Header("General")] 
        public bool AgentMovement;
        public LayerMask GroundCastLayer;
        
        [Header("References")] 
        public NavMeshAgent PrefabNavMeshAgent;
        
        [Header("NavAgent Jump")]
        public float ObstacleOffsetY = 0.75f;
        public float ObstacleCastLength = 3f;
        public float RadiusActiveOffMeshLink = 5f;
        
        [Header("NavAgent Move")]
        public float OrientationSpeed = 30f;
        public float AgentRangeActive = 3f;
        public float AgentVelocityMagnitude = 4f;
        public float RadiusNavRandomSearch = 10.0f;
        
        [Header("Patrols")] 
        public Transform PatrolsContainer;

        private NavMeshAgent navMeshAgent;
        private Transform AgentTransform;
        private Transform BodyTransform;
        private Vector3 offMeshLinkPosition;
        private Vector3 normalObstaclePosition;
        private float agentDistance;
        private Movement _movement;
        
        private int pathNumber = -1;
        private List<Transform> Patrols;

        private MoveKeycode moveKeycode;

        private void Awake()
        {
        }
        
        private void Update()
        {
        }
    }
}