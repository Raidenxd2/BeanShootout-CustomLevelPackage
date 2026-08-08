using System;
using Cysharp.Threading.Tasks;
using SerialPackage.Runtime;
using UnityEngine;
#if KILLITMYSELF_FULL
using UnityEngine.XR.Management;
#endif

namespace KillItMyself.Runtime
{
    public class VRManager : MonoBehaviour
    {
        public bool VREnabled;
        public bool FakeVR;
        public bool VRInitialized;
        public bool BypassVRInit;
        public static bool VRBootEnabled;
        public static bool FakeVRBootEnbaled;
        
#if KILLITMYSELF_FULL
        private XRLoader currentLoader;
#endif

        public GameObject GenericVRCamera;
        public GameObject GlobalXROriginPrefab;
        public GameObject GlobalXROrigin;

        public static VRManager instance;
        
        private void Awake()
        {
            if (instance)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            instance = this;
        }

        public void Init()
        {
            if (VRBootEnabled)
            {
                VREnabled = true;
            }

            if (FakeVRBootEnbaled)
            {
                FakeVR = true;
            }

            if (VREnabled)
            {
                if (PlayerPrefs.GetString("BeanShootoutOpenXRRuntimeOverride", "") == "")
                {
                    Environment.SetEnvironmentVariable("XR_RUNTIME_JSON", null);
                }
                else
                {
                    Environment.SetEnvironmentVariable("XR_RUNTIME_JSON", PlayerPrefs.GetString("BeanShootoutOpenXRRuntimeOverride", ""));
                }

                if (!BypassVRInit)
                {
                    Cursor.visible = false;
#if KILLITMYSELF_FULL
                    InitXR();
#endif
                }
                else
                {
                    VRInitialized = true;
                }
            }
        }

        private void InitXR()
        {
            if (FakeVR)
            {
                VRInitialized = true;
                
                return;
            }
        
#if KILLITMYSELF_FULL
            currentLoader = XRGeneralSettings.Instance.Manager.activeLoaders[0];

            if (!currentLoader.Initialize())
            {
                BeanLogger.LogError("Failed to init current loader.", this);
                VREnabled = false;
                Cursor.visible = true;
                return;
            }

            if (!currentLoader.Start())
            {
                BeanLogger.LogError("Failed to start current loader.", this);
                currentLoader.Deinitialize();
                VREnabled = false;
                Cursor.visible = true;
                return;
            }

            BeanShootoutURP.EnableEarlyCmd = false;
            BeanShootoutURP.EnableXRRenderingSupport = true;

            XRGeneralSettings.Instance.Manager.StartSubsystems();
#endif

            VRInitialized = true;
        }

        public async UniTaskVoid FixUI()
        {
            GlobalXROrigin.SetActive(false);
            await UniTask.WaitForEndOfFrame();
            GlobalXROrigin.SetActive(true);
        }
        
        private void OnDestroy()
        {
            DeInitVR();
        }
        
        public void DeInitVR()
        {
            if (FakeVR || !VREnabled)
            {
                return;
            }
            
#if KILLITMYSELF_FULL
            currentLoader.Stop();
            currentLoader.Deinitialize();
            currentLoader = null;
#endif
        }
    }
}