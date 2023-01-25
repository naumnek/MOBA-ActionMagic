using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

public class ImpactProjectileFX : MonoBehaviour
{
    [Header("References")] 
    public ParticleSystem ImpactVfx;
    
    [Header("Visual")] 
    public bool onlyCharacterHitEffect;
    public float ImpactVfxSize = 1f;
    //public float ImpactVfxLifetime = 0;
    [Tooltip("Offset along the hit normal where the VFX will be spawned")]
    public float ImpactVfxSpawnOffset = 0.1f;

    [Header("Sound")]
    public AudioClip ImpactSfxClip;
    
    private ImpactProjectileFX impactVfxInstance;

    public void Spawn(Vector3 point, Vector3 normal, Collider collider, bool isEnemyCharacter)
    {
        if (impactVfxInstance || !isEnemyCharacter && onlyCharacterHitEffect) return;

        Vector3 center = collider.bounds.center - Vector3.down;
            
        Vector3 shootOrigin = point + (normal * ImpactVfxSpawnOffset);

        Vector3 spawnpoint = isEnemyCharacter ? center : point;
        
        impactVfxInstance = Instantiate(this, spawnpoint,
            Quaternion.LookRotation(normal));
        impactVfxInstance.transform.localScale = new Vector3(ImpactVfxSize, ImpactVfxSize, ImpactVfxSize);
        if(ImpactSfxClip)AudioUtility.CreateSFX(ImpactSfxClip, point, AudioUtility.AudioGroups.Impact, 1f, 3f);
        impactVfxInstance.HitEffect();
    }
    
    
    public void HitEffect()
    {
        ImpactVfx = GetComponent<ParticleSystem>();
        Destroy(this.gameObject, ImpactVfx.duration + ImpactVfx.startLifetime);
        ImpactVfx.Play();
    }

}
