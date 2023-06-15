using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Managers;
using Behaviours;
using Utilities;
using UnityEngine.Windows;
using Unity.VisualScripting;
using System;

namespace Controllers
{
    public class PlayerController : SingletonMonoBehaviour<PlayerController>
    {
        [Header("======== Base Settings ========")]

        [SerializeField]
        SpawnBox spawnBox;

        [SerializeField]
        LayerMask foodLayer;

        [Header("======== Debug ========")]

        [SerializeField]
        bool isReadyToSelect = false;

        [SerializeField]
        Food selectedFood;

        [SerializeField]
        List<Food> matchedFoods;

        [SerializeField]
        List<Food> addedFoodInBox;

        [SerializeField]
        Camera mainCamera;

        [SerializeField]
        int bonusMultiply = 0;

        float timeLeft = 0f;
        int starBonus = 0;

        Coroutine SortFoodBoxCO;
        Coroutine CheckMatch3CO;

        public float GetTimeLeft { get { return timeLeft; } }
        public int GetStarBonus {  get { return starBonus; } }

        private EventManager eventManager;

        void Start()
        {
            eventManager = EventManager.Instance;
            eventManager.OnGameStateChanged.AddListener(OnGameStateChanged);
            eventManager.OnFoodSelection.AddListener(OnFoodSelection);
            eventManager.OnMatchDone.AddListener(OnMatchDone);
            eventManager.OnBonusTimeDone.AddListener(OnBonusOnBonusTimeDone);
            eventManager.OnInputTouchStart.AddListener(OnInputTouchStart);
            eventManager.OnInputTouchStationary.AddListener(OnInputTouchMoved);
            eventManager.OnInputTouchMoved.AddListener(OnInputTouchMoved);
            eventManager.OnInputTouchEnd.AddListener(OnInputTouchEnd);

            if (spawnBox is null) { GameObject.Find("Spawn Box").GetComponent<SpawnBox>(); }
            if (spawnBox is null) new UnityException("Spawn Box can't be found");

            mainCamera = Camera.main;
        }

        #region EVENTS
        void OnGameStateChanged(GameStates gameState)
        {
            switch(gameState)
            {
                case GameStates.Ready:
                    OnReadyState();
                    break;
                case GameStates.Play:
                    OnPlayState();
                    break;
                case GameStates.Win:
                case GameStates.Fail:
                    OnEndState();
                    break;
            }
        }

        void OnReadyState()
        {
            timeLeft = LevelController.Instance.GetCurrentLevel().GetLevelDurationSeconds;
            starBonus = 0;
        }

        void OnPlayState()
        {
            if (addedFoodInBox is null)
                addedFoodInBox = new();
            else
                addedFoodInBox.Clear();

            isReadyToSelect = true;

            StartCoroutine("LevelCountDown");
        }

        void OnEndState()
        {
            isReadyToSelect = false;
            selectedFood = null;

            for (int i = 0; i < addedFoodInBox.Count; i++)
            {
                Destroy(addedFoodInBox[i].gameObject, 0.1f);
            }

            StopAllCoroutines();
            CheckMatch3CO = null;
            SortFoodBoxCO = null;

            addedFoodInBox.Clear();
        }

        void OnMatchDone(int multiplier, GameObject _)
        {
            starBonus += multiplier;

            CheckMatch3CO = StartCoroutine(CheckMatch3());

            if (spawnBox.GetSpawnedObjects.Count == 0)
                GameManager.Instance.SetGameState(GameStates.Win);
        }

        void OnInputTouchStart(Vector2 input) 
        {
            if (!isReadyToSelect) return;

            var food = GetRaycastFood(input);

            if (food != null) 
                selectedFood = food;
        }

        void OnInputTouchMoved(Vector2 input)
        {
            if (!isReadyToSelect) return;

            var food = GetRaycastFood(input);

            if (selectedFood != null && selectedFood != food) selectedFood.Released();
            
            selectedFood = food;

            if (selectedFood != null) selectedFood.Holding();
        }

        void OnInputTouchEnd(Vector2 input)
        {
            if (!isReadyToSelect) return;

            var food = GetRaycastFood(input);

            if (food != null && food == selectedFood)
            {
                selectedFood = null;
                eventManager.OnFoodSelection?.Invoke(food);
                return;
            }

            if (selectedFood == null) return;

            selectedFood.Released();
            selectedFood = null;
        }

        void OnBonusOnBonusTimeDone()
        {
            bonusMultiply = 0;
        }

        void OnFoodSelection(Food food)
        {
            if (food is null) return;

            var selectionBox = UIController.Instance.GetSelectionBoxes;

            if (addedFoodInBox.Count < selectionBox.Count)
                addedFoodInBox.Add(food);

            food.ResetBounds();

            var foodID = CheckIfFoodAddedAndReturnID(food);
            foodID = foodID == -1 ? addedFoodInBox.Count - 1 : foodID;

            if (addedFoodInBox.Count >= selectionBox.Count && CheckHowManySameFoodInBox(food) < 3)
            {
                StartCoroutine(SetEndDelayed(winner: false));
                return;
            }

            var setPosition = selectionBox[foodID].transform.position;
            setPosition.y += 0.2f;
            food.PlaceOnSelectionBox(setPosition);

            SortFoodBoxCO = StartCoroutine(SortFoodBox());
        }
        #endregion
        
        /*-----------------------------------------------------------------------------------------*/

        #region CLASS METHODS
        int CheckIfFoodAddedAndReturnID(Food food)
        {
            var id = -1;

            for (int i = 0; i < addedFoodInBox.Count; i++)
            {
                if(food == addedFoodInBox[i])
                    id = i;
            }

            return id;
        }

        int CheckHowManySameFoodInBox(Food food)
        {
            if (!addedFoodInBox.Contains(food))
                return 0;

            var count = 0;

            foreach(Food foodIn in addedFoodInBox)
            {
               if(foodIn.originalPrefab == food.originalPrefab)
                    count++;
            }

            return count;
        }

        IEnumerator SortFoodBox(bool reSort = false)
        {
            if (addedFoodInBox.Count == 0) yield break;

            for (int i = 0; i < addedFoodInBox.Count; i++)
            {
                var foodA = addedFoodInBox[i];

                for(int y = i + 1; y < addedFoodInBox.Count; y++)
                {
                    var foodB = addedFoodInBox[y];

                    if(foodA.originalPrefab == foodB.originalPrefab)
                    {
                        addedFoodInBox.RemoveAt(y);
                        addedFoodInBox.Insert(i + 1, foodB);
                    }
                }
            }

            var selectionBox = UIController.Instance.GetSelectionBoxes;

            if (reSort) yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < addedFoodInBox.Count; i++)
            {
                var setPosition = selectionBox[i].transform.position;
                setPosition.y += 0.2f;
                addedFoodInBox[i].PlaceOnSelectionBox(setPosition);
            }

            if (reSort) yield break;

            if (addedFoodInBox.Count < 3) yield break;

            CheckMatch3CO = StartCoroutine(CheckMatch3());
        }

        IEnumerator CheckMatch3()
        {
            var selectionBox = UIController.Instance.GetSelectionBoxes;

            for (int i = 1; i < addedFoodInBox.Count; i++)
            {
                if (i + 1 > addedFoodInBox.Count - 1) yield break;

                var foodA = addedFoodInBox[i - 1];
                var foodB = addedFoodInBox[i];
                var foodC = addedFoodInBox[i + 1];

                if (foodA.originalPrefab == foodB.originalPrefab && foodA.originalPrefab == foodC.originalPrefab)
                {
                    var lastIndex = i;

                    var setPositionA = selectionBox[i - 1].transform.position;
                    setPositionA.y += 0.2f;

                    var setPositionC = selectionBox[i + 1].transform.position;
                    setPositionC.y += 0.2f;

                    LeanTween.move(foodA.gameObject, setPositionA, 0.1f);
                    LeanTween.move(foodC.gameObject, setPositionC, 0.1f).setOnComplete(() => 
                    {
                        LeanTween.delayedCall(0.1f, () => 
                        {
                            foodA.SetReadyToMatch(null);
                            foodB.SetReadyToMatch(null);
                            foodC.SetReadyToMatch(CallOnReadyToMatch);

                            void CallOnReadyToMatch()
                            {
                                var setPosition = selectionBox[lastIndex].transform.position;
                                setPosition.y += 0.25f;

                                LeanTween.move(foodA.gameObject, setPosition, 0.1f);
                                LeanTween.move(foodC.gameObject, setPosition, 0.1f).setOnComplete(() =>
                                {
                                    spawnBox.GetSpawnedObjects.Remove(foodA.gameObject);
                                    spawnBox.GetSpawnedObjects.Remove(foodB.gameObject);
                                    spawnBox.GetSpawnedObjects.Remove(foodC.gameObject);

                                    addedFoodInBox.Remove(foodA);
                                    addedFoodInBox.Remove(foodB);
                                    addedFoodInBox.Remove(foodC);

                                    Destroy(foodA.gameObject);
                                    Destroy(foodB.gameObject);
                                    Destroy(foodC.gameObject);

                                    eventManager.OnMatchDone?.Invoke(++bonusMultiply, selectionBox[lastIndex]);

                                    if(SortFoodBoxCO is null)
                                        SortFoodBoxCO = StartCoroutine(SortFoodBox(reSort: true));
                                });
                            }
                        });
                    });
                    yield break;
                }
            }
        }

        GameObject GetRaycastObject(Vector2 screenPos)
        {
            var ray = mainCamera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out var hit, 1000f, foodLayer))
            {
                return hit.collider.gameObject;
            }

            return null;
        }

        Food GetRaycastFood(Vector2 screenPos)
        {
            var raycastObj = GetRaycastObject(screenPos);

            if (!raycastObj) return null;

            var food = raycastObj.GetComponent<Food>();

            return food;
        }

        IEnumerator LevelCountDown()
        {
            while (GameManager.Instance.GetGameState == GameStates.Play)
            {
                yield return new WaitForSeconds(1);

                timeLeft--;

                eventManager.OnUIRefresh?.Invoke();

                if (timeLeft <= 0)
                {
                    StartCoroutine(SetEndDelayed(winner: false));
                    yield break;
                }
            }
        }

        IEnumerator SetEndDelayed(bool winner)
        {
            yield return new WaitForSeconds(0.5f);

            if (winner)
                GameManager.Instance.SetGameState(GameStates.Win);
            else
                GameManager.Instance.SetGameState(GameStates.Fail);
        }
        #endregion
    }
}