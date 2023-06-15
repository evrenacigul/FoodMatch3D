using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Controllers.Editors
{
    [CustomEditor(typeof(LevelController))]
    public class LevelControllerEditor : Editor
    {
        public LevelController levelController;

        private void OnEnable()
        {
            levelController = (LevelController)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Load Previous"))
                {
                    levelController.LoadPreviousLevel();
                    Debug.Log("Load Previous");
                }
                if (GUILayout.Button("Load Current"))
                {
                    levelController.LoadCurrentLevel();
                    Debug.Log("Load Current");
                }
                if (GUILayout.Button("Load Next"))
                {
                    levelController.LoadNextLevel();
                    Debug.Log("Load Next");
                }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create Level"))
            {
                levelController.CreateLevel();
                Debug.Log("Create Level");
            }
        }
    }
}