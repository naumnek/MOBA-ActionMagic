using Unity.FPS.Game;
using UnityEngine;

namespace Platinum.Weapon
{
    [RequireComponent(typeof(SkillController))]
    public class WeaponFuelCellHandler : MonoBehaviour
    {
        [Tooltip("Retract All Fuel Cells Simultaneously")]
        public bool SimultaneousFuelCellsUsage = false;

        [Tooltip("List of GameObjects representing the fuel cells on the weapon")]
        public GameObject[] FuelCells;

        [Tooltip("Cell local position when used")]
        public Vector3 FuelCellUsedPosition;

        [Tooltip("Cell local position before use")]
        public Vector3 FuelCellUnusedPosition = new Vector3(0f, -0.1f, 0f);

        SkillController weapon;
        bool[] m_FuelCellsCooled;

        void Start()
        {
            weapon = GetComponent<SkillController>();
            DebugUtility.HandleErrorIfNullGetComponent<SkillController, WeaponFuelCellHandler>(weapon, this,
                gameObject);

            m_FuelCellsCooled = new bool[FuelCells.Length];
            for (int i = 0; i < m_FuelCellsCooled.Length; i++)
            {
                m_FuelCellsCooled[i] = true;
            }
        }

        void Update()
        {
            if (SimultaneousFuelCellsUsage)
            {
                for (int i = 0; i < FuelCells.Length; i++)
                {
                    FuelCells[i].transform.localPosition = Vector3.Lerp(FuelCellUsedPosition, FuelCellUnusedPosition,
                        weapon.activeBullet.currentBulletsRatio);
                }
            }
            else
            {
                // needs simplification
                for (int i = 0; i < FuelCells.Length; i++)
                {
                    float length = FuelCells.Length;
                    float lim1 = i / length;
                    float lim2 = (i + 1) / length;

                    float value = Mathf.InverseLerp(lim1, lim2, weapon.activeBullet.currentBulletsRatio);
                    value = Mathf.Clamp01(value);

                    FuelCells[i].transform.localPosition =
                        Vector3.Lerp(FuelCellUsedPosition, FuelCellUnusedPosition, value);
                }
            }
        }
    }
}