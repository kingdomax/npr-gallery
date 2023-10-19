using Vrsys;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;
using NprGallery.Models;
using UnityEngine.XR.Interaction.Toolkit;

namespace NprGallery
{
    public class PaintInteraction : MonoBehaviourPunCallbacks
    {
        // ACTION STATE
        private bool _aButtonLF;
        private bool _bButtonLF;
        private float _triggerLF;
        private bool _triggerButtonLF;
        private bool _triggerButtonLF2;
        private bool _isRayHit;
        private RaycastHit _hitObject;

        // USER STATE
        private int _currentEquipment;
        private int _currentRenderingTechnique;
        private int _currentManipulationTechnique;
        private Equipment[] _availableEquipments;
        private RenderingTechnique[] _availableRenderingTechniques;
        private ManipulationTechnique[] _availableManipulationTechniques;

        // REFERECE
        public GameObject _targetPoint;
        public GameObject _pistolAnchor;
        private LineRenderer _lineRenderer;
        private XRController _rightController;
        private ViewingSetupHMDAnatomy _viewingSetupHmd;

        void Start()
        {
            if (!photonView.IsMine) { Destroy(this); } // This script should only compute for the local user

            _aButtonLF = false;
            _bButtonLF = false;
            _triggerLF = 0f;
            _triggerButtonLF = false;
            _triggerButtonLF2 = false;
            _isRayHit = false;

            _currentEquipment = 0;
            _currentRenderingTechnique = 0;
            _currentManipulationTechnique = 0;
            _availableEquipments = new Equipment[] {
                Equipment.None,
                Equipment.Pistol,
                Equipment.MagicStick
            };
            _availableRenderingTechniques = new RenderingTechnique[] { 
                RenderingTechnique.Default,
                RenderingTechnique.Toon,
                RenderingTechnique.Gooch,
                RenderingTechnique.DuotoneSurface,
                RenderingTechnique.RealtimeHatching
            };
            _availableManipulationTechniques = new ManipulationTechnique[] {
                ManipulationTechnique.Default,
                ManipulationTechnique.Rotate,
            };

            _lineRenderer = GetComponent<LineRenderer>();
        }

        void Update()
        {
            if (!IsControllerReady()) { return; }

            var currentEquipment = _availableEquipments[_currentEquipment];

            SwitchEquipment();
            if (currentEquipment != Equipment.None) { RenderRay(); }
            if (currentEquipment != Equipment.None) { SlideNavigation(); }
            if (currentEquipment == Equipment.Pistol) { SwitchRenderingTechniques(); }
            if (currentEquipment == Equipment.Pistol) { Painting(); }
            if (currentEquipment == Equipment.MagicStick) { SwitchManipulationTechniques(); }
            if (currentEquipment == Equipment.MagicStick) { Manipulating(); }
        }

        private bool IsControllerReady()
        {
            _viewingSetupHmd = _viewingSetupHmd == null && NetworkUser.localNetworkUser?.viewingSetupAnatomy is ViewingSetupHMDAnatomy
                                ? (ViewingSetupHMDAnatomy)NetworkUser.localNetworkUser.viewingSetupAnatomy : null;
            _rightController = _rightController == null ? _viewingSetupHmd?.rightController?.GetComponent<XRController>() : null;

            return _viewingSetupHmd != null && _rightController != null;
        }

        private void SwitchEquipment()
        {
            _rightController.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool aButton);

            if (aButton && aButton != _aButtonLF)
            {
                _currentEquipment++;
                if (_currentEquipment == _availableEquipments.Length) { _currentEquipment = 0; }

                photonView.RPC("SwitchEquipment", RpcTarget.AllBuffered, _availableEquipments[_currentEquipment]);
                photonView.RPC("ToggleLineRenderer", RpcTarget.AllBuffered, _availableEquipments[_currentEquipment]);
            }

            _aButtonLF = aButton;
        }

        private void RenderRay()
        {
            var pistolAnchor = _pistolAnchor.transform;

            _isRayHit = Physics.Raycast(pistolAnchor.position,
                                        pistolAnchor.TransformDirection(Vector3.left),
                                        out _hitObject,
                                        6f);

            _targetPoint.transform.position = _hitObject.point;
            _targetPoint.SetActive(_isRayHit);
            _lineRenderer.SetPosition(0, pistolAnchor.position);
            _lineRenderer.SetPosition(1, _isRayHit ? _hitObject.point
                                                   : pistolAnchor.position + pistolAnchor.TransformDirection(Vector3.left) * 6f);
        }

        private void SlideNavigation()
        {
            _rightController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerButton);

            if (triggerButton &&
                triggerButton != _triggerButtonLF &&
                _isRayHit &&
                _hitObject.transform?.parent?.GetComponent<SlideshowScript>() != null)
            {
                _triggerButtonLF = true;
                _hitObject.transform.parent.GetComponent<SlideshowScript>().NavigateSlide(_hitObject.transform.tag);
            }

            _triggerButtonLF = triggerButton;
        }

        private void SwitchRenderingTechniques()
        {
            _rightController.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool bButton);

            if (bButton && bButton != _bButtonLF)
            {
                _currentRenderingTechnique++;
                if(_currentRenderingTechnique == _availableRenderingTechniques.Length) { _currentRenderingTechnique = 0; }

                photonView.RPC("SwitchRenderingTechniques", RpcTarget.AllBuffered, _availableRenderingTechniques[_currentRenderingTechnique]);
            }

            _bButtonLF = bButton;
        }

        private void Painting()
        {
            _rightController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float trigger);

            if (trigger > 0.80f &&
                trigger != _triggerLF &&
                _isRayHit &&
                _hitObject.transform?.tag == "TestObject")
            {
                _hitObject.transform.GetComponent<PaintHandler>().OnPaint(_availableRenderingTechniques[_currentRenderingTechnique]);
            }

            _triggerLF = trigger;
        }

        private void SwitchManipulationTechniques()
        {
            _rightController.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool bButton);

            if (bButton && bButton != _bButtonLF)
            {
                _currentManipulationTechnique++;
                if (_currentManipulationTechnique == _availableManipulationTechniques.Length) { _currentManipulationTechnique = 0; }

                photonView.RPC("SwitchManipulationTechniques", RpcTarget.AllBuffered, _availableManipulationTechniques[_currentManipulationTechnique]);
            }

            _bButtonLF = bButton;
        }

        private void Manipulating()
        {
            _rightController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerButton2);

            if (triggerButton2 &&
                triggerButton2 != _triggerButtonLF2 &&
                _isRayHit &&
                _hitObject.transform?.tag == "TestObject")
            {
                _triggerButtonLF2 = true;
                _hitObject.transform.GetComponent<ManipulationHandler>().OnManipulation(_availableManipulationTechniques[_currentManipulationTechnique]);
            }

            _triggerButtonLF2 = triggerButton2;
        }
    }
}
