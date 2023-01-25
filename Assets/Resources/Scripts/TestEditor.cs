#if UNITY_EDITOR
using UnityEditor;

namespace Platinum
{
    // Tells Unity to use this Editor class with the EnemyStats component.
    [CustomEditor(typeof(Test))]
    public class TestEditor : Editor
    {
        // The various categories the editor will display the variables in
        public enum DisplayCategory
        {
            Basic, Combat, Magic
        }
        // The enum field that will determine what variables to display in the Inspector
        public DisplayCategory categoryToDisplay;
        public override void OnInspectorGUI()
        {
            //Display the enum popup in the inspector
            categoryToDisplay = (DisplayCategory)EditorGUILayout.EnumPopup("Display", categoryToDisplay);

            //Create a space to separate this enum popup from the other variables
            EditorGUILayout.Space();

            //Switch statement to handle what happens for each category
            switch (categoryToDisplay)
            {
                case DisplayCategory.Basic:
                    DisplayBasicInfo();
                    break;

                case DisplayCategory.Combat:
                    DisplayCombatInfo();
                    break;

                case DisplayCategory.Magic:
                    DisplayMagicInfo();
                    break;

            }
            serializedObject.ApplyModifiedProperties();
        }
        // When the categoryToDisplay enum is at "Basic"
        void DisplayBasicInfo()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("health"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defense"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movementSpeed"));
        }

        // When the categoryToDisplay enum is at "Combat"
        void DisplayCombatInfo()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attack"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRange"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackSpeed"));
        }

        // When the categoryToDisplay enum is at "Magic"
        void DisplayMagicInfo()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("magicResistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hasMagic"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mana"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("magicType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("magicDamage"));

        }
    }
}
#endif