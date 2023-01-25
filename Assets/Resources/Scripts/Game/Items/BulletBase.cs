using System.Collections;
using System.Collections.Generic;
using Platinum.Weapon;
using UnityEngine;
using UnityEngine.Events;

public abstract class BulletBase : MonoBehaviour
{
    [Header("References")]
    public ProjectileBase projectilePrefab;
    
    [Header("Parameters")]
    [Tooltip("Delay after the last shot before starting to reload")]
    public float bulletsReloadDelay = 2f;
    
    [Tooltip("Amount of ammo reloaded per bulletsReloadDelay")]
    public float bulletsReloadRate = 1f;
        
    [Tooltip("Maximum amount of ammo in the gun")]
    public int maxBullets = 120;
        
    [Tooltip("Start amount of ammo in the gun")]
    public int startMaxBullets = 60;
        
    [Tooltip("Has physical clip on the weapon and ammo shells are ejected when firing")]
    public bool HasPhysical = false;
    public int GetCurrentBullets() => Mathf.FloorToInt(currentBullets);

    public float ammoRatio => currentBullets / maxBullets;
    public float currentBullets { get; protected set; }
        
    public bool IsReloading { get; protected set; }
    public bool IsCooling { get; protected set; }
    public float currentBulletsRatio { get; protected set; }
    public int GetCarriedAmmo() => Mathf.FloorToInt(currentBullets);
    private float lastUpdateTime;
        
    public virtual void Update()
    {
        if (lastUpdateTime + bulletsReloadDelay > Time.time) return;
        if (IsReloading)
        {
            if (currentBullets < maxBullets)
            {
                UpdateBullets();
                IsCooling = true;
            }
            else
            {
                IsReloading = false;
                IsCooling = false;
            }
        }

        if (maxBullets == Mathf.Infinity)
        {
            currentBulletsRatio = 1f;
        }
        else
        {
            currentBulletsRatio = currentBullets / maxBullets;
        }
    }

    protected virtual void UpdateBullets()
    {
        lastUpdateTime = Time.time;
        // reloads weapon over time
        float resultBullets = currentBullets + bulletsReloadRate;
        currentBullets = resultBullets > maxBullets ? maxBullets : resultBullets;
    }
    private void Awake()
    {
        SetCurrentAmmo(false);
    }

    public virtual void Reload()
    {
        IsReloading = true;
    }
        
    public virtual bool UseAmmo(float amount)
    {
        if (currentBullets >= amount)
        {
            currentBullets -= amount;

            return true;
        }
        return false;
    }
        
    public void AddCarriablePhysicalBullets(int count)
    {
        if (currentBullets + count < maxBullets) currentBullets += count;
        else currentBullets = maxBullets;
    }
        
    public virtual void SetCurrentAmmo(bool InfinityAmmo)
    {
        currentBullets = InfinityAmmo ? 0 : startMaxBullets;
        lastUpdateTime = Time.time;
    }
}
