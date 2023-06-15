using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Managers
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        [Header("======== Base Settings ========")]
        [SerializeField]
        int targetFrameRate = 30;

        [SerializeField]
        float bonusTimeOutSeconds = 5f;

        [Header("======== Debug ========")]

        [SerializeField]
        private GameStates gameState;

        public GameStates GetGameState { get { return gameState; } }

        public int GetTargetFPS { get { return targetFrameRate; } }

        public float GetBonusTimeOutSeconds { get { return bonusTimeOutSeconds; } }

        void Start()
        {
            Application.targetFrameRate = targetFrameRate;

            StartCoroutine(LateStart());
        }

        IEnumerator LateStart()
        {
            yield return new WaitForSeconds(0.1f);

            SetGameState(GameStates.Load);
        }

        public void SetGameState(GameStates state)
        {
            gameState = state;

            EventManager.Instance.OnGameStateChanged?.Invoke(state);

            switch (state)
            {
                case GameStates.Load:
                case GameStates.Ready:
                    break;
            }
        }
    }

    public enum GameStates
    {
        Load,
        Ready,
        Play,
        Win,
        Fail
    }
}