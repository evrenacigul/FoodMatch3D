using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities;
using Behaviours;

namespace Managers
{
    public class EventManager : SingletonMonoBehaviour<EventManager>
    {
        //Base Events
        public UnityEvent<GameStates> OnGameStateChanged;

        //Player Events
        public UnityEvent<Food> OnFoodSelection;
        public UnityEvent OnFoodView;
        public UnityEvent<int, GameObject> OnMatchDone;
        public UnityEvent OnBonusTimeDone;

        //UI Events
        public UnityEvent OnWinScreenAppears;
        public UnityEvent OnFailScreenAppears;
        public UnityEvent OnUIRefresh;

        //Input Events
        public UnityEvent<Vector2> OnInputTouchStart;
        public UnityEvent<Vector2> OnInputTouchMoved;
        public UnityEvent<Vector2> OnInputTouchStationary;
        public UnityEvent<Vector2> OnInputTouchEnd;
    }
}