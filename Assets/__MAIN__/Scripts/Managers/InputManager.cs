using UnityEngine;
using UnityEngine.EventSystems;
using Utilities;

namespace Managers
{
    public class InputManager : SingletonMonoBehaviour<InputManager>
    {
        private void Update()
        {
            if (Input.touchCount is 0) return;

            Touch touch = Input.GetTouch(0);

            if (GameManager.Instance.GetGameState is not GameStates.Play &&
                EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    EventManager.Instance.OnInputTouchStart?.Invoke(touch.position);
                    break;
                case TouchPhase.Moved:
                    EventManager.Instance.OnInputTouchMoved?.Invoke(touch.position);
                    break;
                case TouchPhase.Stationary:
                    EventManager.Instance.OnInputTouchStationary?.Invoke(touch.position);
                    break;
                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    EventManager.Instance.OnInputTouchEnd?.Invoke(touch.position);
                    break;
            }
        }
    }
}