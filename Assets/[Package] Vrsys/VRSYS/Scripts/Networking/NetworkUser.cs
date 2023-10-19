using TMPro;
using Photon.Pun;
using UnityEngine;
using NprGallery.Models;
using NprGallery;

namespace Vrsys
{
    [RequireComponent(typeof(AvatarAnatomy))]
    public class NetworkUser : MonoBehaviourPunCallbacks, IPunObservable
    {
        public static GameObject localGameObject;
        public static GameObject localHead;
        public static NetworkUser localNetworkUser
        {
            get
            {
                return localGameObject.GetComponent<NetworkUser>();
            }
        }

        [Tooltip("The viewing prefab to instantiate for the local user. For maximum support, this should contain a ViewingSetupAnatomy script at root level, which supports the AvatarAnatomy attached to gameObject.")]
        [SerializeField]
        private GameObject viewingSetup;

        [Tooltip("If true, a TMP_Text element will be searched in child components and a text will be set equal to photonView.Owner.NickName. Note, this feature may create unwanted results if the GameObject, which contains this script, holds any other TMP_Text fields but the actual NameTag.")]
        public bool setNameTagToNickname = true;

        [Tooltip("The spawn position of this NetworkUser")]
        public Vector3 spawnPosition = Vector3.zero;

        [HideInInspector]
        public AvatarAnatomy avatarAnatomy { get; private set; }

        [HideInInspector]
        public ViewingSetupAnatomy viewingSetupAnatomy { get; private set; }

        private Vector3 receivedScale = Vector3.one;

        private bool hasPendingScaleUpdate
        {
            get
            {
                return (transform.localScale - receivedScale).magnitude > 0.001;
            }
        }

        // REFERECE
        public GameObject _handMesh;
        public GameObject _pistol;
        public GameObject _magicStick;
        public GameObject _shadingText;
        public GameObject _manipulationText;
        public GameObject _pistolAnchor;
        public GameObject _targetPoint;
        private LineRenderer _lineRenderer;

        void Awake()
        {
            avatarAnatomy = GetComponent<AvatarAnatomy>();
            _lineRenderer = GetComponent<LineRenderer>();

            // Only owner user
            if (photonView.IsMine)
            {
                NetworkUser.localGameObject = gameObject;
                NetworkUser.localHead = avatarAnatomy.head;
                InitializeAvatar();
                InitializeViewing();
            }

            // All user
            if (PhotonNetwork.IsConnected)
            {
                gameObject.name = photonView.Owner.NickName + (photonView.IsMine ? " [Local User]" : " [External User]");
                var nameTagTextComponent = avatarAnatomy.nameTag.GetComponentInChildren<TMP_Text>();
                if (nameTagTextComponent && setNameTagToNickname)
                {
                    nameTagTextComponent.text = photonView.Owner.NickName;
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting && photonView.IsMine)
            {
                stream.SendNext(viewingSetup.transform.lossyScale);
                if (_targetPoint == null)
                    stream.SendNext(false);
                else
                    stream.SendNext(_targetPoint.activeInHierarchy);
            }
            else if (stream.IsReading)
            {
                receivedScale = (Vector3)stream.ReceiveNext();
                bool targetPointActive = (bool)stream.ReceiveNext();
                if(_targetPoint != null)
                    _targetPoint.SetActive(targetPointActive);
            }
        }

        void Update()
        {
            if (photonView.IsMine) { return; }
            
            if (hasPendingScaleUpdate)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, receivedScale, Time.deltaTime);
            }

            if (_lineRenderer != null && _lineRenderer.enabled)
            {
                var pistolAnchor = _pistolAnchor.transform;
                _lineRenderer.SetPosition(0, _pistolAnchor.transform.position);
                _lineRenderer.SetPosition(1, _targetPoint.activeInHierarchy 
                                                ? _targetPoint.transform.position
                                                : pistolAnchor.position + pistolAnchor.TransformDirection(Vector3.left) * 6f);
            }
        }

        private void InitializeAvatar()
        {
            Color clr = GetColorFromList();
            photonView.RPC("SetColor", RpcTarget.AllBuffered, new object[] { new Vector3(clr.r, clr.g, clr.b) });
        }

        private void InitializeViewing()
        {
            //Check whcih platform is running
            if (viewingSetup == null)
            {
                throw new System.ArgumentNullException("Viewing Setup must not be null for local NetworkUser.");
            }

            viewingSetup = Instantiate(viewingSetup);
            viewingSetup.transform.position = spawnPosition;
            viewingSetup.transform.SetParent(gameObject.transform, false);
            viewingSetup.name = "Viewing Setup";

            viewingSetupAnatomy = viewingSetup.GetComponentInChildren<ViewingSetupAnatomy>();
            if (viewingSetupAnatomy)
            {
                avatarAnatomy.ConnectFrom(viewingSetupAnatomy);
            }
            else
            {
                Debug.LogWarning("Your Viewing Setup Prefab does not contain a '" + typeof(ViewingSetupAnatomy).Name + "' Component. This can lead to unexpected behavior.");
            }
        }

        private void HideHandsInFavorOfControllers()
        {
            AvatarHMDAnatomy ahmda = GetComponent<AvatarHMDAnatomy>();
            if (ahmda != null)
            {
                ahmda.handRight.SetActive(false);
                ahmda.handLeft.SetActive(false);
            }
        }

        private static Color GetColorFromList()
        {
            var colorList = new Color[]
            {
                new Color(1f, 1f, 1f), // White
                new Color(.2f, .2f, .2f), // Black
                new Color(0f, 1f, 0f), // Green
                new Color(1f, 0f, 0f), // Red
                new Color(0f, 0f, 1f), // Blue
                new Color(255f / 255f, 192f / 255f, 203 / 255f), // Pink
                new Color(.6f, .6f, .6f) // Grey
            };

            return colorList[(PhotonNetwork.PlayerListOthers?.Length ?? 0) % 7];
        }

        [PunRPC]
        public void SetColor(Vector3 color)
        {
            var myColor = new Color(color.x, color.y, color.z);
            avatarAnatomy.SetColor(myColor);
            if(_lineRenderer != null)
                _lineRenderer.material.color = myColor;
            if(_targetPoint != null)
                _targetPoint.GetComponent<MeshRenderer>().material.color = myColor;
        }

        [PunRPC]
        public void SetName(string name)
        {
            gameObject.name = name + (photonView.IsMine ? " [Local User]" : " [External User]");
            var nameTagTextComponent = avatarAnatomy.nameTag.GetComponentInChildren<TMP_Text>();
            if (nameTagTextComponent && setNameTagToNickname)
            {
                nameTagTextComponent.text = name;
            }
        }

        [PunRPC]
        public void SwitchEquipment(Equipment target)
        {
            if (_handMesh == null || _magicStick == null || _manipulationText == null || _pistol == null || _shadingText == null)
                return;

            if (target == Equipment.Pistol)
            {
                _handMesh.SetActive(false);
                _magicStick.SetActive(false);
                _manipulationText.SetActive(false);
                _pistol.SetActive(true);
                _shadingText.SetActive(true);
            }
            else if (target == Equipment.MagicStick)
            {
                _handMesh.SetActive(false);
                _pistol.SetActive(false);
                _shadingText.SetActive(false);
                _magicStick.SetActive(true);
                _manipulationText.SetActive(true);
            }
            else
            {
                _handMesh.SetActive(true);
                _magicStick.SetActive(false);
                _manipulationText.SetActive(false);
                _pistol.SetActive(false);
                _shadingText.SetActive(false);
                _targetPoint.SetActive(false);
            }
        }

        [PunRPC]
        public void ToggleLineRenderer(Equipment current)
        {
            if (_lineRenderer == null)
                return;

            _lineRenderer.enabled = current != Equipment.None;
        }

        [PunRPC]
        public void SwitchRenderingTechniques(RenderingTechnique technique)
        {
            if (_shadingText == null)
                return;

            var renderingConfig = RenderingProvider.GetRenderingConfig(technique);
            _shadingText.GetComponent<TextMesh>().text = $"[Rendering] {renderingConfig.DisplayName}";
        }

        [PunRPC]
        public void SwitchManipulationTechniques(ManipulationTechnique technique)
        {
            if (_manipulationText == null)
                return;

            _manipulationText.GetComponent<TextMesh>().text = $"[Manipulation] {technique.ToString()}";
        }
    }
}
