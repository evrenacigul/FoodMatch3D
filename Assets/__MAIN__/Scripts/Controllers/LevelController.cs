using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using Managers;
using Utilities;
using Behaviours;

namespace Controllers
{
    public class LevelController : SingletonMonoBehaviour<LevelController>
    {
        [SerializeField]
        List<Object> levels;
        
        [SerializeField]
        [Range(0, 100)]
        int currentLevelID = 0;

        [SerializeField]
        Object baseLevel;

        [SerializeField]
        string levelsDirectoryPath;
        
        GameObject currentLevel;

        Transform levelsParent;

        bool initialLoad = true;
        
        public int LevelCount { get; private set; }

        public int GetCurrentLevelID { get { return currentLevelID; } }

        public Level GetCurrentLevel() { return currentLevel?.GetComponent<Level>(); }

        private void OnEnable()
        {
            if (!levelsParent) levelsParent = transform;
        }

        protected override void Awake()
        {
            base.Awake();

            if(!levelsParent) levelsParent = transform;

            LevelCount = levels.Count;
        }

        private void Start()
        {
            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStates state)
        {
            if (!Application.isPlaying) return;
            if (levels is null || levels.Count == 0) throw new UnityException("No levels assigned!");

            switch(state)
            {
                case GameStates.Load:
                    StartCoroutine(WaitLevelLoadToStart(() => { GameManager.Instance.SetGameState(GameStates.Ready); }));
                    break;

                case GameStates.Ready:
                    StartCoroutine(WaitLevelLoadToStart(() => { GameManager.Instance.SetGameState(GameStates.Play); }));
                    break;

                case GameStates.Play:
                    //var level = currentLevel.GetComponent<Level>();

                    break;

                case GameStates.Win:

                    break;

                case GameStates.Fail:
                    
                    break;
            }
        }

        private IEnumerator WaitLevelLoadToStart(UnityAction callback)
        {
            do
            {
                if (initialLoad)
                {
                    LoadLevelInitially();
                }
                else
                {
                    LoadCurrentLevel();
                }

                yield return new WaitForFixedUpdate();
            } while (currentLevel is null);

            callback?.Invoke();
        }

        private void LoadLevelInitially()
        {
            currentLevelID = PlayerPrefs.GetInt("currentLevel", 0);
            if (currentLevelID >= levels.Count)
            {
                currentLevelID = 0;
                PlayerPrefs.SetInt("currentLevel", currentLevelID);
            }

            initialLoad = false;

            LoadCurrentLevel();
        }

        private bool CheckLevels()
        {
            if (levels is null || levels.Count == 0) throw new UnityException("Cannot find any level to load");

            return true;
        }

        public void LoadLevelByID(int ID)
        {
            if (!CheckLevels()) return;

            currentLevelID = ID;
            PlayerPrefs.SetInt("currentLevel", ID);
            LeanTween.delayedCall(0.1f, () => { GameManager.Instance.SetGameState(GameStates.Load); });
        }

        public void LoadCurrentLevel()
        {
            if (!CheckLevels()) return;

            if (Application.isPlaying)
            {
                var childCount = transform.childCount;

                for (int i = 0; i < childCount; i++)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }

                currentLevel = Instantiate(levels[currentLevelID], transform) as GameObject;
            }
            else
            {
            #if UNITY_EDITOR
                var childs = transform.GetComponentsInChildren<Transform>();

                foreach (Transform child in childs)
                {
                    if (child != null && child.parent == transform)
                        DestroyImmediate(child.gameObject);
                }

                var level = PrefabUtility.InstantiatePrefab(levels[currentLevelID]) as GameObject;
                level.transform.SetParent(transform);
            #endif
            }
        }

        public void RestartLevel()
        {
            if (!CheckLevels()) return;
            
            if (Application.isPlaying)
            {
                PlayerPrefs.SetInt("currentLevel", currentLevelID);

                LeanTween.delayedCall(0.1f, () => { GameManager.Instance.SetGameState(GameStates.Load); });
            }
        }

        public void LoadNextLevel()
        {
            if (!CheckLevels()) return;

            if (currentLevelID < levels.Count - 1)
                currentLevelID++;
            else
                currentLevelID = 0;


            if (Application.isPlaying)
            {
                PlayerPrefs.SetInt("currentLevel", currentLevelID);

                LeanTween.delayedCall(0.1f, () => { GameManager.Instance.SetGameState(GameStates.Load); });
            }
            else
            {
                LoadCurrentLevel();
            }
        }

        public void LoadPreviousLevel()
        {
            if (!CheckLevels()) return;

            if (currentLevelID == 0)
                currentLevelID = levels.Count - 1;
            else
                currentLevelID--;

            if (Application.isPlaying)
            {
                PlayerPrefs.SetInt("currentLevel", currentLevelID);

                LeanTween.delayedCall(0.1f, () => { GameManager.Instance.SetGameState(GameStates.Load); });
            }
            else
            {
                LoadCurrentLevel();
            }
        }

        public void CreateLevel()
        {
            #if UNITY_EDITOR
            var levelCount = levels.Count + 1;
            var createdLevel = PrefabUtility.InstantiatePrefab(baseLevel) as GameObject;
            var variantLevel = PrefabUtility.SaveAsPrefabAsset(createdLevel, levelsDirectoryPath + "Level_" + levelCount.ToString("00") + ".prefab");
            levels.Add(variantLevel);
            DestroyImmediate(createdLevel);
            #endif
        }
    }
}