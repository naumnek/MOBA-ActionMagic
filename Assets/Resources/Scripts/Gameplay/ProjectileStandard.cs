using Platinum.Controller;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using Platinum.Info;
using Platinum.CustomEvents;
using Platinum.Scene;
using Platinum.Settings;

namespace Platinum.Weapon
{
    public class ProjectileStandard : ProjectileBase
    {

        const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide;
        private float updateDistance;
        private bool isHit;
        Vector3 m_LastRootPosition;
        Vector3 m_Velocity;

        void OnEnable()
        {
            m_ProjectileBase = GetComponent<ProjectileBase>();
            m_LastRootPosition = Root.position;
            m_Velocity = transform.forward * Speed;

            Destroy(gameObject, MaxLifeTime);
        }

        void Update()
        {
            if (!activate) return;
            updateDistance = Vector3.Distance(InitialPosition, transform.position);
            if (updateDistance > MaxShotDistance)
            {
                Destroy(gameObject);
                return;
            }
            
            // Move
            transform.position += m_Velocity * Time.deltaTime;
            if (InheritWeaponVelocity)
            {
                transform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;
            }

            // Orient towards velocity
            transform.forward = m_Velocity.normalized;

            // Gravity
            if (GravityDownAcceleration > 0)
            {
                // add gravity to the projectile velocity for ballistic effect
                m_Velocity += Vector3.down * GravityDownAcceleration * Time.deltaTime;
            }

            // Hit detection
            if (CheckCast(Radius, out RaycastHit closestHit))
            {
                // Handle case of casting while already inside a collider
                OnHit(closestHit);
            }

            m_LastRootPosition = Root.position;
        }

        private bool CheckCast(float radius, out RaycastHit closestHit)
        {
            closestHit = new RaycastHit();
            closestHit.distance = Mathf.Infinity;
            
            // Sphere cast
            Vector3 displacementSinceLastFrame = Tip.position - m_LastRootPosition;
            RaycastHit[] hits = Physics.SphereCastAll(m_LastRootPosition, radius,
                displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, HittableLayers,
                k_TriggerInteraction);
            foreach (var hit in hits)
            {
                if (IsHitValid(hit) && hit.distance < closestHit.distance)
                {
                    closestHit = hit;
                    if (closestHit.distance <= 0f)
                    {
                        closestHit.point = Root.position;
                        closestHit.normal = -transform.forward;
                    }
                    return true;
                }
            }
            return false;
        }

        bool IsHitValid(RaycastHit hit)
        {
            // ignore hits with an ignore component
            if (hit.collider.GetComponentInParent<IgnoreHitDetection>())
            {
                return false;
            }

            // ignore hits with triggers that don't have a Damageable component
            if (hit.collider.isTrigger && hit.collider.GetComponentInParent<Damageable>() == null)
            {
                return false;
            }

            // ignore hits with specific ignored colliders (self colliders, by default)
            if (m_IgnoredColliders != null && m_IgnoredColliders.Contains(hit.collider))
            {
                return false;
            }

            return true;
        }

        void OnHit(RaycastHit hitInfo)
        {
            if(isHit) return;;
            Vector3 point = hitInfo.point;
            Vector3 normal = hitInfo.normal;
            Collider collider = hitInfo.collider;
            Vector3 center = collider.bounds.center;

            // point damage
            Damageable damageable = collider.GetComponentInParent<Damageable>();
            bool isEnemyCharacter = damageable && !damageable.Health.IsDead &&
                                    damageable.Actor.Info.Affiliation.Number !=
                                    m_ProjectileBase.Owner.Info.Affiliation.Number;

            //if(damageable)Debug.Log("OnHit: " + damageable.Actor.Info.PlayerInfo.Nickname);
            if (damageable && !isEnemyCharacter) return;
            
            // damage
            if (AreaOfDamage)
            {
                // area damage
                AreaOfDamage.InflictDamageInArea(Damage, point, HittableLayers, k_TriggerInteraction,
                    m_ProjectileBase.Owner);
            }
            else
            {
                
                //if(damageable) Debug.Log("damageable");
                if (isEnemyCharacter)
                {
                    isHit = true;
                    m_ProjectileBase.ParentWeapon.HitEnemy(damageable.Actor.Info.Affiliation.Number);
                    if (damageable.InflictDamage(Damage, false, m_ProjectileBase.Owner, collider))
                    {
                        KillEvent evt = Events.KillEvent;
                        evt.killed = damageable.Actor;
                        evt.killer = m_ProjectileBase.Owner;
                        EventManager.Broadcast(evt);
                    }
                }
                else
                {
                    /*if (CheckCast(RadiusAfterHit, out RaycastHit closestHit))
                    {
                        OnHit(closestHit);   
                    }*/
                }
                if (ImpactFXPrefab)
                {
                    ImpactFXPrefab.Spawn(point, normal, collider, isEnemyCharacter);
                }
            }
            // Self Destruct
            Destroy(gameObject, isEnemyCharacter ? onHitLifeTime / 2 : onHitLifeTime);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = RadiusColor;
            Gizmos.DrawSphere(transform.position, Radius);
        }
    }
}