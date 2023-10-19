using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vrsys
{
    public class HeadtrackingObserver : MonoBehaviour
    {
        private float measurementDuration = 5f; // in sec
        private float movementThreshold = 0.01f; // in meter

        private List<float> timeStepList;
        private List<Vector3> posStepList;

        public bool movementFlag = false;

        private GameObject infoDisplay;
        public Vector2 infoDisplayPos;

        // Start is called before the first frame update
        void Start()
        {
            timeStepList = new List<float>();
            posStepList = new List<Vector3>();

            measurementDuration = ProjectionWallSystemConfigParser.Instance.config.multiUserSettings.monoFallbackDetectionDuration;
            movementThreshold = ProjectionWallSystemConfigParser.Instance.config.multiUserSettings.monoFallbackDetectionMovementThreshold;

            infoDisplay = Vrsys.Utility.FindRecursive(gameObject, "HeadtrackingInfoDisplay");

            GameObject infoPanel = Vrsys.Utility.FindRecursive(infoDisplay, "InfoPanel");
            infoPanel.GetComponent<RectTransform>().anchoredPosition = infoDisplayPos;

            GameObject infoText = Vrsys.Utility.FindRecursive(infoDisplay, "InfoText");
            infoText.GetComponent<RectTransform>().anchoredPosition = infoDisplayPos;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateMovementHistory();
            DetectSignificantMovement();
        }

        private void UpdateMovementHistory()
        {
            timeStepList.Add(Time.time);
            posStepList.Add(transform.localPosition);

            int counter = 0;
            foreach (float timeStep in timeStepList)
            {
                if (Time.time - timeStep > measurementDuration)
                    counter += 1;
            }
            for (int i = 0; i < counter; i++)
            {
                timeStepList.RemoveAt(0);
                posStepList.RemoveAt(0);
            }
        }

        private void DetectSignificantMovement()
        {
            float dist = 0f;

            for (int i = 0; i < posStepList.Count; i++)
            {
                if (i > 0)
                {
                    Vector3 pos = posStepList[i];
                    Vector3 posLF = posStepList[i - 1];

                    dist += (pos - posLF).magnitude;
                }
            }

            movementFlag = dist > movementThreshold;
        }

        public void ShowInfoDisplay(bool flag)
        {
            infoDisplay.SetActive(flag);
        }
    }
}
