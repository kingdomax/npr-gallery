using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vrsys
{
    [ExecuteInEditMode]
    public class ScreenProperties : MonoBehaviour
    {
        public float width = 3f;
        public float height = 2f;

        public bool drawGizmoFlag = true; // helper visualizations

        public Vector3 topLeftCorner {
            get
            {
                return transform.TransformPoint(new Vector3(-width * 0.5f, height * 0.5f, 0f));
            }
        }

        public Vector3 topRightCorner {
            get
            {
                return transform.TransformPoint(new Vector3(width * 0.5f, height * 0.5f, 0f));
            }
        }

        public Vector3 bottomRightCorner {
            get
            {
                return transform.TransformPoint(new Vector3(width * 0.5f, -height * 0.5f, 0f));
            }
        }

        public Vector3 bottomLeftCorner
        {
            get
            {
                return transform.TransformPoint(new Vector3(-width * 0.5f, -height * 0.5f, 0f));
            }
        }


        private void OnDrawGizmos()
        {
            if (drawGizmoFlag)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(bottomLeftCorner, topLeftCorner);
                Gizmos.DrawLine(topLeftCorner, topRightCorner);
                Gizmos.DrawLine(topRightCorner, bottomRightCorner);
                Gizmos.DrawLine(bottomRightCorner, bottomLeftCorner);
            }
        }
    }

}