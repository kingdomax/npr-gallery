using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Vrsys
{
    public class ProjectionWallSystemConfigParser : MonoBehaviourPunCallbacks
    {
        [Header("Parsed from SystemConfig")]
        [Tooltip("All fields will be set after instantiation. Their values are JSON parsed from the SystemConfig.content")]
        public Config config;

        [HideInInspector]
        public bool readSuccess = false;

        public static ProjectionWallSystemConfigParser Instance { get; protected set; }

        public MultiUserSettings multiUserSettings
        {
            get
            {
                return config.multiUserSettings;
            }
        }

        public StereoUserSettings localUserSettings
        {
            get
            {
                foreach (var u in config.multiUserSettings.users)
                {
                    if (u.hostname == Environment.MachineName.ToLower())
                        return u;
                }
                return config.multiUserSettings.userDefault;
            }
        }

        private bool shouldWriteStartupFeedbackFile
        {
            get
            {
                return readSuccess && config.startupFeedback && localUserSettings.masterFlag;
            }
        }

        private void Awake()
        {
            if (!photonView.IsMine)
                Destroy(this);

            Instance = this;

            if (SystemConfig.Instance != null)
            {
                config = JsonUtility.FromJson<Config>(SystemConfig.Instance.content);
                readSuccess = true;
            }
            else
            {
                throw new FileNotFoundException("No SystemConfig provided");
            }
        }

        private void Start()
        {
            if (shouldWriteStartupFeedbackFile)
                WriteStartupFeedbackFile();
            photonView.RPC("SetName", RpcTarget.AllBuffered, new object[] { localUserSettings.username });
        }

        // Feedback file is used for slave machine startup (TODO: rework cluster launch to deprecate this)

        private void WriteStartupFeedbackFile()
        {
            string file = config.startupFeedbackPath + "/master.txt";

            using (StreamWriter sw = File.CreateText(file))
            {
                sw.Close();
            }
            Debug.Log("StartupFeedbackFile written");
        }

        // Serializable Classes for Json Parsing

        [Serializable]
        public class Config
        {
            public string configName = "";
            public bool startupFeedback = false;
            public string startupFeedbackPath = "";
            public MultiUserSettings multiUserSettings;
        }

        [Serializable]
        public class MultiUserSettings
        {
            public float screenWidth;
            public float screenHeight;
            public float[] screenPos = new float[4];
            public float[] windowSettings = new float[4];
            public float[] windowSettingsCropped = new float[4];
            public bool monoFallback;
            public float monoFallbackDetectionDuration;
            public float monoFallbackDetectionMovementThreshold;
            public StereoUserSettings userDefault;
            public StereoUserSettings[] users;

            public Vector3 screenPosVector3
            {
                get
                {
                    return new Vector4(screenPos[0], screenPos[1], screenPos[2]);
                }
            }

            public Vector4 windowSettingsVector4
            {
                get
                {
                    return new Vector4(windowSettings[0], windowSettings[1], windowSettings[2], windowSettings[3]);
                }
            }

            public Vector4 windowSettingsCroppedVector4
            {
                get
                {
                    return new Vector4(windowSettingsCropped[0], windowSettingsCropped[1], windowSettingsCropped[2], windowSettingsCropped[3]);
                }
            }
        }

        [Serializable]
        public class StereoUserSettings
        {
            public string username = "";
            public string hostname = "";
            public bool masterFlag = false;
            public bool headtrackingFlag = true;
            public int trackingID = 0;
            public float[] fixedHeadPos = new float[3];
            public float eyeDistance = 0.064f;

            public Vector3 fixedHeadPosVector3
            {
                get
                {
                    return new Vector3(fixedHeadPos[0], fixedHeadPos[1], fixedHeadPos[2]);
                }
            }
        }
    }
}
