using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;


namespace Vrsys
{
    public class ProjectionWallViewing : MonoBehaviour
    {
        public static ProjectionWallViewing Instance { get; private set; }

        public GameObject leftCamera;
        public GameObject rightCamera;
        public GameObject head;
        public GameObject screen;

        private GameObject dtrack;
        private DTrack.DTrackReceiver6Dof dtrackReceiver;
        private bool headtrackingFlag = false;
        public HeadtrackingObserver headtrackingObserver;
        
        private ProjectionWallSystemConfigParser.StereoUserSettings user;
        
        public Vector4 windowSettings = new Vector4(4096, 2160, 0, 0);
        public Vector4 windowSettingsCropped = new Vector4(3270, 2160, 665, 0);        

        // key bindings
        private KeyCode headtrackingKey = KeyCode.H;


        // Start is called before the first frame update
        void Awake()
        {                       
            if (Instance != null)
            {
                Destroy(this);
                throw new Exception("you can only have one [ViewingSetup] in the scene");
            }

            Instance = this;
        }

        private void Start()
        {
            ApplyMultiUserViewingConfig();

            // Custom fixed update for projection calculation, set a 120Hz
            InvokeRepeating("MyFixedUpdate", 0, (1f / 120f));
        }

        private void ApplyMultiUserViewingConfig()
        {

            var configParser = ProjectionWallSystemConfigParser.Instance;

            user = configParser.localUserSettings;
            windowSettings = configParser.multiUserSettings.windowSettingsVector4;
            windowSettingsCropped = configParser.multiUserSettings.windowSettingsCroppedVector4;

            // ensure cameras
            if (leftCamera == null || rightCamera == null)
            {
                foreach(var rootGo in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()) 
                {
                    leftCamera = Utility.FindRecursive(rootGo, "LeftCamera");
                    rightCamera = Utility.FindRecursive(rootGo, "RightCamera");
                    if (leftCamera != null && rightCamera != null)
                        break;
                }
                var leftLocalPos = leftCamera.transform.localPosition;
                var rightLocalPos = rightCamera.transform.localPosition;
                leftCamera.transform.SetParent(head.transform, false);
                leftCamera.transform.localPosition = leftLocalPos;
                leftCamera.transform.localRotation = Quaternion.identity;
                rightCamera.transform.SetParent(head.transform, false);
                rightCamera.transform.localPosition = rightLocalPos;
                rightCamera.transform.localRotation = Quaternion.identity;
            }
            

            if (screen == null)
            {
                screen = Utility.FindRecursive(this.gameObject, "Screen");
            }
            screen.transform.localPosition = configParser.multiUserSettings.screenPosVector3;
            var screenProps = screen.GetComponent<ScreenProperties>();
            screenProps.width = configParser.multiUserSettings.screenWidth;
            screenProps.height = configParser.multiUserSettings.screenHeight;

            // ensure OffAxisProjection
            var rightOffAxis = rightCamera.GetComponent<OffAxisProjection>();
            if (rightOffAxis == null) 
            {
                rightOffAxis = rightCamera.AddComponent<OffAxisProjection>();
                rightOffAxis.screen = screen.GetComponent<ScreenProperties>();
            }

            var leftOffAxis = leftCamera.GetComponent<OffAxisProjection>();
            if (leftOffAxis == null)
            {
                leftOffAxis = leftCamera.AddComponent<OffAxisProjection>();
                leftOffAxis.screen = screen.GetComponent<ScreenProperties>();
            }
            //

            EnableHeadtracking(user.headtrackingFlag);

            SetViewportOnCameras();
            SetEyeDistance(user.eyeDistance);

            if (user.headtrackingFlag && configParser.multiUserSettings.monoFallback)
            {
                var go = Instantiate(Resources.Load("ViewingSetup/HeadtrackingObserver") as GameObject, Vector3.zero, Quaternion.identity);
                go.transform.SetParent(this.transform, false);
                go.GetComponent<DTrack.DTrackReceiver6Dof>().bodyId = user.trackingID;

                headtrackingObserver = go.GetComponent<HeadtrackingObserver>();
                headtrackingObserver.infoDisplayPos = new Vector2((int)windowSettingsCropped.z + windowSettingsCropped.x / 2, (int)windowSettingsCropped.w + windowSettingsCropped.y / 2);
            }
        }

        private void EnableHeadtracking(bool flag)
        {
            headtrackingFlag = flag;            

            if (head == null)
                head = Utility.FindRecursive(this.gameObject, "Head");

            if (dtrack == null)
                dtrack = Utility.FindRecursiveInScene("DTrack");

            if (dtrackReceiver == null)
                dtrackReceiver = head.GetComponent<DTrack.DTrackReceiver6Dof>();

            if (headtrackingFlag)
            {                
                dtrackReceiver.enabledFlag = true;
                dtrackReceiver.bodyId = user.trackingID;
                SetEyeDistance(user.eyeDistance);
            }
            else 
            {
                head.transform.localPosition = user.fixedHeadPosVector3;
                head.transform.localRotation = Quaternion.identity;

                dtrackReceiver.enabledFlag = false;
                SetEyeDistance(0f);
            }
            
            Debug.Log("[Camera] Headtracking: " + headtrackingFlag);
        }
        
        // Update is called once per frame
        void Update()
        {           

        }

        private void MyFixedUpdate()
        {
            leftCamera.GetComponent<OffAxisProjection>().CalcProjection();
            rightCamera.GetComponent<OffAxisProjection>().CalcProjection();
        }

        private void MonoStereoToggle()
        {
            if (headtrackingObserver is null)
                return;

            if (headtrackingObserver.movementFlag != headtrackingFlag)
            {
                EnableHeadtracking(headtrackingObserver.movementFlag);
                headtrackingObserver.ShowInfoDisplay(!headtrackingObserver.movementFlag);
            }
        }

        private void SetViewportOnCameras()
        {            
            float viewportPosX = windowSettingsCropped.z / (int)windowSettings.x;
            float viewportWidth = windowSettingsCropped.x / (int)windowSettings.x;

            leftCamera.GetComponent<Camera>().rect = new Rect(viewportPosX * 0.5f, 0f, viewportWidth * 0.5f, 1f);
            rightCamera.GetComponent<Camera>().rect = new Rect(0.5f + viewportPosX * 0.5f, 0f, viewportWidth * 0.5f, 1f);            
        }

        
        private void ToggleHeadtracking()
        {
            if (!user.headtrackingFlag)
                return;

            EnableHeadtracking(!headtrackingFlag);
        }

        private void SetEyeDistance(float eyeDist)
        {                        
            leftCamera.transform.localPosition = new Vector3(-eyeDist * 0.5f, 0f, 0f);
            rightCamera.transform.localPosition = new Vector3(eyeDist * 0.5f, 0f, 0f);
        }
    }
}
