using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controllers;
using Managers;

namespace Behaviours
{
    public class SpawnBox : MonoBehaviour
    {
        List<GameObject> spawnedObjects;

        Bounds bounds;

        Vector3 scale;

        public List<GameObject> GetSpawnedObjects { get { return spawnedObjects; } }

        private void Start()
        {
            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);

            bounds = GetComponent<MeshFilter>().sharedMesh.bounds;
            scale = transform.localScale;
        }

        void OnGameStateChanged(GameStates gameState)
        {
            switch (gameState)
            {
                case GameStates.Play:
                    OnPlayState();
                    break;
                default:
                    OnEndState();
                    break;
            }
        }

        void OnPlayState()
        {
            var level = LevelController.Instance.GetCurrentLevel();
            if (level is null) new UnityException("Level is null");

            StartCoroutine(SpawnLevelObjects(level));
        }
        void OnEndState()
        {
            if (spawnedObjects is null || spawnedObjects.Count is 0) return;

            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                Destroy(spawnedObjects[i]);
            }

            spawnedObjects.Clear();
            spawnedObjects = null;
        }

        IEnumerator SpawnLevelObjects(Level level)
        {
            if (spawnedObjects is null) spawnedObjects = new();

            var spawnableFoods = level.GetSpawnableFoods;

            var position = transform.position;

            var spawnPosY = bounds.max.y;
            var spawnPosXmax = bounds.max.x * scale.x + position.x;
            var spawnPosZmax = bounds.max.z * scale.z + position.z;
            var spawnPosXmin = bounds.min.x * scale.x + position.x;
            var spawnPosZmin = bounds.min.z * scale.z + position.z;

            var boundsMin = new Vector3(spawnPosXmin, 0f, spawnPosZmin);
            var boundsMax = new Vector3(spawnPosXmax, spawnPosY, spawnPosZmax);

            foreach(var food in spawnableFoods)
            {
                for (int i = 0; i < 3; i++)
                {
                    var posX = Random.Range(spawnPosXmin, spawnPosXmax);
                    var posZ = Random.Range(spawnPosZmin, spawnPosZmax);
                    var spawnedObj = SpawnObject(food, new Vector3(posX, spawnPosY, posZ));

                    if (spawnedObj.TryGetComponent<Food>(out var foodComp))
                    {
                        foodComp.SetBounds(boundsMin, boundsMax);
                        foodComp.SetOriginalPrefab = food;
                    }
                    
                    spawnedObjects.Add(spawnedObj);
                }

                yield return new WaitForFixedUpdate();
            }
        }

        GameObject SpawnObject(Object obj, Vector3 position)
        {
            var spawnedObj = (GameObject)Instantiate(obj, position, Quaternion.identity);

            return spawnedObj;
        }
    }
}