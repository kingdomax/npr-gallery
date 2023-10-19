using UnityEngine;

namespace NprGallery
{
    public class LightController : MonoBehaviour
    {
        public float _rayLength = .5f;
        public GameObject _dlMain;
        public GameObject _dlChild1;
        public GameObject _dlChild2;

        void Start() { DrawDirectionLightDirection(); }

        private void DrawDirectionLightDirection()
        {
            var dlLine0 = _dlMain.GetComponent<LineRenderer>();
            dlLine0.positionCount = 2;
            dlLine0.SetPosition(0, _dlMain.transform.position);
            dlLine0.SetPosition(1, _dlMain.transform.position + _dlMain.transform.TransformDirection(Vector3.forward) * _rayLength);

            var dlLine1 = _dlChild1.GetComponent<LineRenderer>();
            dlLine1.positionCount = 2;
            dlLine1.SetPosition(0, _dlChild1.transform.position);
            dlLine1.SetPosition(1, _dlChild1.transform.position + _dlChild1.transform.TransformDirection(Vector3.forward) * _rayLength);

            var dlLine2 = _dlChild2.GetComponent<LineRenderer>();
            dlLine2.positionCount = 2;
            dlLine2.SetPosition(0, _dlChild2.transform.position);
            dlLine2.SetPosition(1, _dlChild2.transform.position + _dlChild2.transform.TransformDirection(Vector3.forward) * _rayLength);
        }
    }
}
