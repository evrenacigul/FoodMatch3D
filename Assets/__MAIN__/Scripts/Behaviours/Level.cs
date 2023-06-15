using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Behaviours
{
    public class Level : MonoBehaviour
    {
        [Header("======== Settings ========")]
        [SerializeField]
        List<Object> spawnableFoods;

        [SerializeField]
        float levelDurationSeconds = 600f;

        [HideInInspector]
        public List<Object> GetSpawnableFoods { get { return spawnableFoods; } }

        [HideInInspector]
        public float GetLevelDurationSeconds { get {  return levelDurationSeconds; } }
    }
}