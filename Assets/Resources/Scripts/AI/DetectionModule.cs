using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using Platinum.Info;
using Platinum.Weapon;

namespace Platinum.Controller.AI
{
    public class DetectionModule : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] bool DetectRandomEnemy;
        
        [Header("Parameters")]
        public LayerMask enemyCastLayer;
        [Tooltip("The max distance at which the enemy can see targets")]
        [SerializeField] float DetectionRange = 100f;
        [SerializeField] float findInterval = 1f;

        public float DynamicAttackRange { get; private set; }

        public UnityAction<DetectionTarget> onDetectedTarget;
        public UnityAction<DetectionTarget> onLostDetectedTarget;

        public Vector3 LastTargetPosition{ get; private set; }
        private Transform m_AimPoint;
        private ControllerBot controllerBot;
        private int m_teamNumber;
        private float m_LastDistance;
        private bool activation;
        private float lastTimeFind;
        private Vector3 castBoxSize;

        private void Awake()
        {
            controllerBot = GetComponent<ControllerBot>();
            castBoxSize = new Vector3(DetectionRange, DetectionRange, DetectionRange);

            controllerBot.onRespawn += OnRespawn;
            controllerBot.onDie += OnDie;
            controllerBot.onSwitchedToWeapon += OnWeaponSwitched;
        }

        private void Start()
        {
            m_teamNumber = controllerBot.Actor.Info.Affiliation.Number;
            m_AimPoint = controllerBot.Actor.AimPoint;
            LastTargetPosition = m_AimPoint.position;
        }

        private void Update()
        {
            if (!activation) return;
            
            if(lastTimeFind + findInterval < Time.time) Find();
        }

        private void Find()
        {
            lastTimeFind = Time.time;

            Vector3 pos = m_AimPoint.position;
            Quaternion rot = m_AimPoint.rotation;
            foreach (var hit in Physics.BoxCastAll(pos,castBoxSize,pos,rot,0,enemyCastLayer))
            {
                Damageable damageable = hit.collider.GetComponentInParent<Damageable>();
                if (damageable && damageable.Actor.Info.Affiliation.Number != m_teamNumber && !damageable.Health.IsDead)
                {
                    DetectActor(damageable.Actor, GetRange(damageable.Actor));
                }
            }
            /*if (GetNearestEnemy(out Actor enemy, out float distance))
            {
                DetectActor(enemy, false);
            }*/
        }

        private TypeRange GetRange(Actor enemy)
        {
            float distance = GetDistance(enemy);
            if (distance <= 0) return TypeRange.Random;
                
            if (distance <= DynamicAttackRange && IsNotObstacle(enemy, distance)) return TypeRange.Attack;
            
            if (distance <= DetectionRange) return TypeRange.Sight;
            return TypeRange.Random;
        }

        private bool IsNotObstacle(Actor enemy, float distance)
        {
            Vector3 startPos = m_AimPoint.position;
            Vector3 direction = enemy.AimPoint.position - startPos;
            
           /* bool isHit = Physics.Linecast(startPos, enemy.AimPoint.position, out RaycastHit hit, enemyCastLayer);
            Damageable damageable = hit.collider.GetComponent<Damageable>();
            if(damageable)  Debug.Log("IsDetect: " + damageable.Actor.Info.PlayerInfo.Nickname);
            return isHit && damageable && damageable.Actor.AimPoint == enemy.AimPoint;*/
            
            /*foreach(RaycastHit hit in Physics.RaycastAll(startPos, direction, distance, enemyCastLayer))
            {
                Damageable damageable = hit.collider.GetComponentInParent<Damageable>();
                if (damageable && damageable.Actor.Info.Affiliation.Number != m_teamNumber)
                {
                    return true;
                }
            }*/
            bool isHit = Physics.Linecast(startPos, enemy.AimPoint.position, out RaycastHit hit, controllerBot.GroundCastLayer);
            return !isHit;
        }

        private void DetectPlace(Vector3 position)
        {
            //in developing...
            DetectionTarget detection = new DetectionTarget();
            detection.ID = 0;
            detection.placePosition = position;
            detection.TypeTarget = TypeTarget.Enemy;
            detection.TypeRange = TypeRange.Random;
            OnDetect(detection);
        }
        private void DetectItem() {}
        
        private void DetectActor(Actor enemy, TypeRange range)
        {
            DetectionTarget detection = new DetectionTarget();
            detection.actor = enemy;
            detection.TypeTarget = TypeTarget.Enemy;
            detection.TypeRange = range;
            OnDetect(detection);
        }
        
        public Vector3 GenerationRandomPosition(Vector3 point)
        {
            float range = DynamicAttackRange;
            Vector3 rX = Vector3.right * Random.Range(-range,range);
            Vector3 rZ = Vector3.forward * Random.Range(-range,range);

            return point + rX + rZ;
        }
        
        public bool GetRandomEnemyActor(out Actor enemy)
        {
            List<Actor> actors = new List<Actor>();
            actors.AddRange(ActorsManager.GetEnemyActors(controllerBot.Actor.Info.Affiliation.Number));

            enemy = actors[Random.Range(0, actors.Count)];
            
            return actors.Count > 0;
        }
        
        public bool GetNearestEnemy(out Actor nearestActor, out float distance)
        {
            List<Actor> actors = new List<Actor>();
            actors.AddRange(ActorsManager.GetEnemyActors(m_teamNumber));

            nearestActor = actors.OrderBy(x => GetDistance(x)).First();
            distance = GetDistance(nearestActor);
            return distance > 0;
        }
        
        private float GetDistance(Actor actor)
        {
            if (actor == null) return -1;
            m_LastDistance = Vector3.Distance(m_AimPoint.position, actor.AimPoint.position);
            return m_LastDistance;
        }
        
        public void OnRespawn()
        {
            activation = true;
            
            if (DetectRandomEnemy)
            {
                Actor actor = ActorsManager.GetRandomEnemyActor(m_teamNumber);
                DetectActor(actor, TypeRange.Random);
            }
        }
        
        private void OnDie()
        {
            activation = false;
        }
        
        public void OnDetect(DetectionTarget detection)
        {
            //Debug.Log("OnDetect: " + detection.Position + " | " + detection.TypeTarget+ " | " + detection.TypeRange);
            
            Debug.Log("DetectActor: " + detection.TypeRange);
            onDetectedTarget?.Invoke(detection);
        }

        public void OnDamaged(Actor hitActor)
        {
            DetectActor(hitActor, TypeRange.Random);
        }

        public void OnWeaponSwitched(WeaponController newWeapon)
        {
            DynamicAttackRange = newWeapon.bulletPrefab.projectilePrefab.MaxShotDistance - 2f;
        }
    }

    public struct DetectionTarget
    {
        public TypeTarget TypeTarget;
        public TypeRange TypeRange;
        public int ID
        {
            get
            {
                return TypeTarget switch
                {
                    TypeTarget.Enemy => actor.Info.ActorID,
                    TypeTarget.Place => ID
                };
            }
            set {}
        }
        public Vector3 targetPosition
        {
            get
            {
                return TypeTarget switch {
                    TypeTarget.Enemy => actor.AimPoint.position,
                    TypeTarget.Place => placePosition
                };
            }
        }

        public bool isActive
        {
            get
            {
                return TypeTarget switch {
                    TypeTarget.Enemy => actor && !actor.ControllerBase.Health.IsDead,
                    TypeTarget.Place => true
                };
            }
        }
        public Actor actor { private get; set; }
        public Vector3 placePosition { private get; set; }
    }

}