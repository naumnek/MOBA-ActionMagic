using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platinum.Weapon;
using System.Linq;
using System;

namespace Platinum.Settings
{
    [CreateAssetMenu(menuName = "ItemSettings")]
    public class ItemSettings : ScriptableObject
    {
        [field: SerializeField]
        public WeaponItem[] ItemList { get; private set; }

        [field: SerializeField]
        public MaxValueAttributes MaxWeaponAttributes { get; private set; }

        private WeaponItem[] NotRequredItems;
        private WeaponItem[] RequredItems;
        private WeaponController[] RequredWeaponsList;
        //public WeaponController[] WeaponsList { get; private set; }

        public void SetItemsSaves()
        {
            for (int i = 0; i < ItemList.Length; i++)
            {
                ItemList[i].weapon.SetItemID(i);
            }
        }
        public WeaponController[] GetNotRequredWeapons()
        {
            NotRequredItems = GetNotRequredItems();
            RequredWeaponsList = new WeaponController[NotRequredItems.Length];
            for (int i = 0; i < NotRequredItems.Length; i++)
            {
                RequredWeaponsList[i] = NotRequredItems[i].weapon;
                NotRequredItems[i].Reset();
            }
            return RequredWeaponsList;
        }

        public WeaponController GetWeaponByID(int id)
        {
            return ItemList[id].weapon;
        }
        
        public string GetPublicRandomWeaponName()
        {
            WeaponItem[] currentItems = GetRequredItems().Where(i => !i.IsBlocked).ToArray();
            RequredWeaponsList = new SkillController[currentItems.Length];
            for (int i = 0; i < currentItems.Length; i++)
            {
                RequredWeaponsList[i] = currentItems[i].weapon;
                currentItems[i].Reset();
            }

            if (RequredWeaponsList.Length == 0) return "";
            int indexWeapon = UnityEngine.Random.Range(0, RequredWeaponsList.Length);
            return RequredWeaponsList[indexWeapon].WeaponName;
        }
        public WeaponController[] GetRequredWeapons()
        {
            RequredItems = GetRequredItems();
            RequredWeaponsList = new WeaponController[RequredItems.Length];
            for (int i = 0; i < RequredItems.Length; i++)
            {
                RequredWeaponsList[i] = RequredItems[i].weapon;
                RequredItems[i].Reset();
            }
            return RequredWeaponsList;
        }

        public bool CheckItems()
        {
            return ItemList.Where(i => !i.IsBlocked && !i.IsUnvisibly && !i.IsPaid).ToArray().Length > 0;
        }
        public WeaponItem[] GetRequredItems()
        {
            return ItemList.Where(i => !i.IsBlocked).ToArray();
        }
        public WeaponItem[] GetNotRequredItems()
        {
            return ItemList.Where(i => i.IsBlocked).ToArray();
        }

        public WeaponController[] ExceptNotRequredWeapon(WeaponController[] currentWeapon)
        {
            var weapons = currentWeapon.Except(GetNotRequredWeapons());
            WeaponController[] requredWeapon = weapons.ToArray();
            return requredWeapon;
        }

        public void ResetAllItemInfo()
        {
            foreach (WeaponItem item in ItemList)
            {
                item.Reset();
            }
        }
    }
    [Serializable]
    public struct MaxValueAttributes
    {
        public float MaxBullets;
        public float Damage;
        public float BulletSpeed;
        public float BulletSpreadAngle;
        public float BulletsPerShoot;
    }

    [Serializable]
    public class WeaponItem
    {
        [Header("Options Item")]
        public WeaponController weapon;
        public bool IsUnvisibly;
        public bool IsBlocked;

        public string Name() => weapon.WeaponName;
        public WeaponAttributes Attributes { get; private set; }

        public bool IsPaid { get; private set; }

        public bool RemovedFromList { get; private set; }

        public void OnPaid() => IsPaid = true;
        public void OnRemovedFromList() => RemovedFromList = true;

        public void Reset()
        {
            IsPaid = false;
            RemovedFromList = false;
            Attributes = new WeaponAttributes();
            Attributes.SetAttributes(weapon);
        }
    }

    public struct WeaponAttributes
    {
        public float MaxBullets { get; private set; }
        public float Damage { get; private set; }
        public float BulletSpeed { get; private set; }
        public float SpreadAngle { get; private set; }
        public float BulletsPerShoot { get; private set; }

        public void SetAttributes(WeaponController weapon)
        {
            MaxBullets = weapon.bulletPrefab.maxBullets;
            Damage = weapon.bulletPrefab.projectilePrefab.Damage;
            BulletSpeed = weapon.bulletPrefab.projectilePrefab.Speed;
            //SpreadAngle = weapon.BulletSpreadAngle;
            BulletsPerShoot = weapon.BulletsPerShot;
        }
    }
}
