using Photon.Pun;
using UnityEngine;
using NprGallery.Models;

namespace NprGallery
{
    public class ManipulationHandler : MonoBehaviourPunCallbacks
    {
        private ManipulationTechnique _currentManipulationTechnique;
        private Quaternion _defaultRotation;

        // OBJECT STATE
        private bool _isRotate;

        void Start()
        {
            _currentManipulationTechnique = ManipulationTechnique.Default;
            _defaultRotation = transform.rotation;
            _isRotate = false;
        }

        void Update()
        {
            if (_isRotate) { transform.Rotate(Vector3.up * (25f * Time.deltaTime), Space.World); }
        }

        public void OnManipulation(ManipulationTechnique newTechnique)
        {
            photonView.RPC("OnManipulation", RpcTarget.AllBuffered, newTechnique);
        }

        [PunRPC]
        public void OnManipulation(ManipulationTechnique newTechnique, PhotonMessageInfo info)
        {
            _currentManipulationTechnique = newTechnique;

            switch (newTechnique)
            {
                case ManipulationTechnique.Rotate:
                    ToggleObjectRotation();
                    break;
                case ManipulationTechnique.Default:
                default:
                    ResetToDefault();
                    break;
            }
        }

        private void ResetToDefault()
        {
            transform.rotation = _defaultRotation;
            _isRotate = false;
        }

        private void ToggleObjectRotation()
        {
            _isRotate = !_isRotate;
        }
    }
}
