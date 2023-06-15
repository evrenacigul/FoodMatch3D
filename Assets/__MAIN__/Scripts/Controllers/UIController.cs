using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Managers;
using Utilities;
using TMPro;
using Unity.VisualScripting;
using Behaviours;

namespace Controllers
{
    public class UIController : SingletonMonoBehaviour<UIController>
    {
        [Header("======== Screens ========")]
        [SerializeField]
        GameObject winScreen;

        [SerializeField]
        GameObject failScreen;

        [Header("======== Selection Box Item Holders ========")]

        [SerializeField]
        List<GameObject> selectionBoxes;

        [Header("======== Bonus TimeOut Slider Settings ========")]

        [SerializeField]
        Slider bonusSlider;

        [SerializeField]
        Color bonusSliderMaxColor = Color.red;

        [SerializeField]
        Color bonusSliderMinColor = Color.yellow;

        [SerializeField]
        Image bonusSliderImage;

        [SerializeField]
        TMP_Text bonusSliderText;

        [SerializeField]
        float bonusSliderCurrentValue = 0f;

        [SerializeField]
        float bonusSliderMaxValue = 1f;

        [SerializeField]
        float bonusSliderMinValue = 0f;

        float timeOut;
        float timeOutLeft;
        bool isTimeOutCountingDown = false;
        Coroutine countDownCO;

        [Header("======== Misc ========")]

        [SerializeField]
        GameObject mainPanel;

        [SerializeField]
        TMP_Text levelText;

        [SerializeField]
        GameObject fpsObj;

        [SerializeField]
        Button restartButton;

        [SerializeField]
        Image starSprite;

        [SerializeField]
        TMP_Text starCountText;

        [SerializeField]
        TMP_Text levelTimeText;

        public List<GameObject> GetSelectionBoxes { get { return selectionBoxes; } }

        private PlayerController pCont;

        private void Start()
        {
            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
            EventManager.Instance.OnMatchDone.AddListener(OnMatchDone);
            EventManager.Instance.OnBonusTimeDone.AddListener(OnBonusTimeDone);
            EventManager.Instance.OnUIRefresh.AddListener(OnUIRefresh);
            
            bonusSlider.value = bonusSliderCurrentValue;
            bonusSlider.maxValue = bonusSliderMaxValue;
            bonusSlider.minValue = bonusSliderMinValue;

            timeOut = GameManager.Instance.GetBonusTimeOutSeconds;
        }

        #region EVENTS
        void OnGameStateChanged(GameStates state)
        {

            switch (state)
            {
                case GameStates.Load:
                    OnLoadState();
                    break;

                case GameStates.Ready:
                    OnReadyState();
                    break;

                case GameStates.Play:
                    ////////
                    break;
                case GameStates.Win:
                    OnEndState(true);
                    break;

                case GameStates.Fail:
                    OnEndState(false);
                    break;
            }
        }

        void OnLoadState()
        {
            winScreen.SetActive(false);
            failScreen.SetActive(false);
        }

        void OnReadyState()
        {
            pCont = PlayerController.Instance;

            OnUIRefresh();

            levelText.text = "Lvl " + (LevelController.Instance.GetCurrentLevelID + 1).ToString();

            if(countDownCO is not null) StopCoroutine(countDownCO);
            bonusSlider.value = 0f;
            OnBonusTimeDone();
        }

        void OnEndState(bool isWinner)
        {
            if (isWinner)
            {
                AnimateScreen(winScreen);
                EventManager.Instance.OnWinScreenAppears?.Invoke();
            }
            else
            {
                AnimateScreen(failScreen);
                EventManager.Instance.OnFailScreenAppears?.Invoke();
            }

            timeOutLeft = 0f;
        }

        void OnMatchDone(int bonusMultiplier, GameObject uiObject)
        {
            isTimeOutCountingDown = false;

            if(countDownCO is not null)
                StopCoroutine(countDownCO);

            bonusSliderCurrentValue = bonusSliderMaxValue;
            bonusSliderText.text = "x" + bonusMultiplier.ToString();
            bonusSlider.value = bonusSliderCurrentValue;

            timeOutLeft = timeOut;

            countDownCO = StartCoroutine(BonusSliderCountDown());

            StartCoroutine(CreateStarAndSendToDestination(uiObject.transform.position, starSprite.transform.position));
        }

        void OnBonusTimeDone()
        {
            bonusSlider.value = 0f;
            bonusSliderText.text = "x1";
            isTimeOutCountingDown = false;
        }

        void OnUIRefresh()
        {
            if (pCont is null) pCont = PlayerController.Instance;
            if (pCont is null) new UnityException("PlayerController can't be found");

            starCountText.text = pCont.GetStarBonus.ToString();
            var timeLeft = pCont.GetTimeLeft;
            var timeSpan = TimeSpan.FromSeconds(timeLeft);

            levelTimeText.text = timeSpan.Minutes.ToString("00") + ":" + timeSpan.Seconds.ToString("00");
        }
        #endregion

        #region CLASS METHODS

        void AnimateScreen(GameObject screen)
        {
            var scale = screen.transform.localScale;

            screen.transform.localScale = Vector3.zero;

            screen.SetActive(true);

            LeanTween.scale(screen, scale, 0.5f).setEaseInOutElastic();
        }

        IEnumerator CreateStarAndSendToDestination(Vector3 startPosition, Vector3 endPosition)
        {
            for (int i = 0; i < 3; i++)
            {
                var star = Instantiate(starSprite.gameObject, startPosition, starSprite.transform.rotation, mainPanel.transform);
                LeanTween.move(star, endPosition, 0.65f).setEaseInOutExpo().setOnComplete(() => { Destroy(star.gameObject); });
                yield return new WaitForSeconds(0.03f);
            }
        }

        IEnumerator BonusSliderCountDown()
        {
            isTimeOutCountingDown = true;

            while (isTimeOutCountingDown && timeOutLeft > 0f)
            {
                timeOutLeft -= 0.05f;

                var tValue = Mathf.InverseLerp(0f, timeOut, timeOutLeft);

                bonusSliderCurrentValue = Mathf.Lerp(bonusSliderMinValue, bonusSliderMaxValue, tValue);

                timeOutLeft = Mathf.Clamp(timeOutLeft, 0f, timeOut);

                bonusSliderImage.color = Color.Lerp(bonusSliderMinColor, bonusSliderMaxColor, bonusSliderCurrentValue);

                bonusSlider.value = bonusSliderCurrentValue;

                if (timeOutLeft <= 0f)
                {
                    EventManager.Instance.OnBonusTimeDone?.Invoke();
                    
                    yield break;
                }

                yield return new WaitForSeconds(0.05f);
            }
        }

        #endregion
    }
}