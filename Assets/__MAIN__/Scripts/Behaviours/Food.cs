using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;
using UnityEngine.Events;

namespace Behaviours
{
    public class Food : MonoBehaviour
    {
        public Object originalPrefab { get; private set; }

        public Object SetOriginalPrefab { set { originalPrefab = value; } }

        Rigidbody rBody;
        Material material;

        Vector3 currentPosition;

        Vector3 lastPosition;
        Quaternion lastRotation;

        Vector3 boundsMin;
        Vector3 boundsMax;

        Vector3 originalScale;

        bool isBoundsSet = false;
        bool isHandling = false;

        Vector3 camApproachPosition { get { return Vector3.Lerp(transform.position, Camera.main.transform.position, 0.1f); } }

        void Start()
        {
            rBody = GetComponent<Rigidbody>();
            material = GetComponent<MeshRenderer>().material;
            originalScale = transform.localScale;
        }

        void FixedUpdate()
        {
            if (rBody is null || !isBoundsSet) return;
            if (isHandling) return;
            if (rBody.velocity.magnitude >= 0.3f)
            {
                currentPosition = rBody.position;
                currentPosition = new Vector3(
                    Mathf.Clamp(currentPosition.x, boundsMin.x, boundsMax.x),
                    Mathf.Clamp(currentPosition.y, boundsMin.y, boundsMax.y),
                    Mathf.Clamp(currentPosition.z, boundsMin.z, boundsMax.z));
                rBody.position = currentPosition;
            }
        }

        public void SetBounds(Vector3 boundsMin, Vector3 boundsMax)
        {
            this.boundsMin = boundsMin;
            this.boundsMax = boundsMax;

            isBoundsSet = true;
        }

        public void ResetBounds()
        {
            isBoundsSet = false;

            LeanTween.cancel(gameObject);
            Destroy(GetComponent<MeshCollider>());
            Destroy(rBody);
        }

        public void Holding()
        {
            if (isHandling) return;

            isHandling = true;

            lastPosition = transform.position;

            lastRotation = transform.rotation;

            if (rBody)
            {
                rBody.useGravity = false;
                rBody.isKinematic = true;
            }

            EventManager.Instance.OnFoodView?.Invoke();

            if(material)
                material.SetFloat("_FirstOutlineWidth", 0.05f);

            LeanTween.cancel(gameObject);
            LeanTween.move(gameObject, camApproachPosition, 0.1f).setEaseInOutCubic();
            LeanTween.scale(gameObject, originalScale * 1.5f, 0.1f).setEaseInOutCubic();
            LeanTween.rotate(gameObject, Vector3.zero, 0.1f).setOnComplete(() =>
            {
                LeanTween.rotateAround(gameObject, Vector3.up + Vector3.forward, 360f, 3f).setLoopClamp();
            });
        }

        public void Released()
        {
            if (!isHandling) return;

            material.SetFloat("_FirstOutlineWidth", 0f);

            LeanTween.cancel(gameObject);
            LeanTween.move(gameObject, lastPosition, 0.1f).setEaseInOutCubic().setOnComplete(() =>
            {
                if (rBody)
                {
                    rBody.useGravity = true;
                    rBody.isKinematic = false;
                }

                isHandling = false;
            });
            LeanTween.scale(gameObject, originalScale, 0.1f).setEaseInOutCubic();
            LeanTween.rotate(gameObject, lastRotation.eulerAngles, 0.1f).setEaseInOutQuad();
        }

        public void PlaceOnSelectionBox(Vector3 setPosition)
        {
            LeanTween.cancel(gameObject);

            material.SetFloat("_FirstOutlineWidth", 0f);

            var rotateTo = Vector3.zero;
            rotateTo.x = 30f;
            rotateTo.y = 30;

            LeanTween.rotate(gameObject, rotateTo, 0.1f).setOnComplete(() =>
            {
                rotateTo.y = -30;
                LeanTween.rotate(gameObject, rotateTo, 4f).setEaseInOutSine().setLoopPingPong();
            });
            LeanTween.scale(gameObject, originalScale / 2.5f, 0.1f);
            LeanTween.move(gameObject, setPosition, 0.1f).setEaseInOutCirc();
        }

        public void SetReadyToMatch(System.Action callBackOnComplete)
        {
            var rotateTo = Vector3.zero;
            rotateTo.x = 30f;

            LeanTween.rotate(gameObject, rotateTo, 0.01f).setOnComplete(() =>
            {
                LeanTween.cancel(gameObject);
                LeanTween.scale(gameObject, originalScale / 2f, 0.1f).setEaseInOutBounce().setOnComplete(callBackOnComplete);
            });
        }

        public bool ComparePrefabs(Object prefab)
        {
            if (prefab is null || !originalPrefab.Equals(prefab)) return false;

            return true;
        }
    }
}