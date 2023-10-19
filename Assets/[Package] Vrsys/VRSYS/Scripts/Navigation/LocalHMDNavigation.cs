using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Vrsys
{
    public class LocalHMDNavigation : MonoBehaviourPunCallbacks
    {
        [Tooltip("Translation Velocity [m/sec]")]
        [Range(0.1f, 10.0f)]
        public float translationVelocity = 5.0f;

        [Tooltip("Rotation Velocity [degree/sec]")]
        [Range(1.0f, 10.0f)]
        public float rotationVelocity = 10.0f;

        ViewingSetupHMDAnatomy viewingSetupHmd;
        private XRController _leftController;

        private Collider groundColider;

        // Start is called before the first frame update
        void Start()
        {
            groundColider = GameObject.Find("Ground")?.GetComponent<Collider>() ?? null;

            // This script should only compute for the local user
            if (!photonView.IsMine)
                Destroy(this);
        }

        // Update is called once per frame
        void Update()
        {
            // Only calculate & apply input if local user fully instantiated
            if (EnsureViewingSetup() && EnsureController())
            {
                MapInput(CalcTranslationInput(), CalcRotationInput());
            }
        }

        private Vector3 CalcTranslationInput()
        {
            float trigger;
            _leftController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out trigger);
            trigger = trigger > 0.1 ? trigger : 0.0f;
            var dir = _leftController.transform.forward;
            dir.y = 0;
            return dir.normalized * trigger * translationVelocity * Time.deltaTime;
        }

        private Vector3 CalcRotationInput()
        {
            Vector2 joystick;
            _leftController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystick);
            return new Vector3(0, joystick.x * 0.25f * rotationVelocity, 0);
        }

        private void MapInput(Vector3 translationInput, Vector3 rotationInput)
        {
            Vector3 newPosition = viewingSetupHmd.transform.position + translationInput;
            if (groundColider?.bounds.Contains(newPosition) ?? false)
            {
                viewingSetupHmd.transform.position += translationInput;
            }

            viewingSetupHmd.transform.rotation *= Quaternion.Euler(rotationInput);
        }

        bool EnsureViewingSetup()
        {
            if (viewingSetupHmd == null)
            {
                if (NetworkUser.localNetworkUser != null)
                {
                    var viewingSetup = NetworkUser.localNetworkUser.viewingSetupAnatomy;
                    if (viewingSetup is ViewingSetupHMDAnatomy)
                    {
                        viewingSetupHmd = (ViewingSetupHMDAnatomy)viewingSetup;
                    }
                }
            }
            return viewingSetupHmd != null;
        }

        bool EnsureController()
        {
            if (_leftController == null)
            {
                _leftController = viewingSetupHmd.leftController.GetComponent<XRController>();
            }
            return _leftController != null;
        }
    }
}
