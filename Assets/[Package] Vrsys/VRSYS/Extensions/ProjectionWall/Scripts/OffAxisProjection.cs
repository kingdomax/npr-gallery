using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vrsys
{
    [RequireComponent(typeof(Camera))]
    public class OffAxisProjection : MonoBehaviour
    {
        // externals
        public ScreenProperties screen;        

        private Vector3 eyePos;
        private Camera cam;

        public bool autoUpdate = false;
        public bool calcNearClipPlane = false;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (autoUpdate)
                CalcProjection();
        }

        public void CalcProjection()
        {
            transform.localRotation = Quaternion.Inverse(transform.parent.localRotation);

            eyePos = transform.position;

            var eyePosSP = screen.transform.worldToLocalMatrix * new Vector4(eyePos.x, eyePos.y, eyePos.z, 1f);
            eyePosSP *= -1f;

            var near = cam.nearClipPlane;
            if(calcNearClipPlane)
            {
                var s1 = screen.transform.position;
                var s2 = screen.transform.position - screen.transform.forward;
                var camOnScreenForward = Vector3.Project((transform.position - s1), (s2 - s1)) + s1;
                near = Vector3.Distance(screen.transform.position, camOnScreenForward);
            }
            var far = cam.farClipPlane;

            var factor = near / eyePosSP.z;
            var l = (eyePosSP.x - screen.width * 0.5f) * factor;
            var r = (eyePosSP.x + screen.width * 0.5f) * factor;
            var b = (eyePosSP.y - screen.height * 0.5f) * factor;
            var t = (eyePosSP.y + screen.height * 0.5f) * factor;

            cam.projectionMatrix = Matrix4x4.Frustum(l, r, b, t, near, far);
        }
    }
}
