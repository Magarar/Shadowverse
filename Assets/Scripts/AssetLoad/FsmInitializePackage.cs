using System.Collections;
using UnityEngine;
using YooAsset;

namespace AssetLoad
{
    public class FsmInitializePackage:IStateNode
    {
        private StateMachine machine;
        
        public void OnCreate(StateMachine machine)
        {
            this.machine = machine;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("初始化资源包！");
            GameAsset.Get().StartCoroutine(InitPackage());
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }

        private IEnumerator InitPackage()
        {
            var playMode = (EPlayMode)machine.GetBlackboardValue("PlayMode");
            var packageName = (string)machine.GetBlackboardValue("PackageName");
            
            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
                package = YooAssets.CreatePackage(packageName);
            
            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                initializationOperation = package.InitializeAsync(createParameters);
            }
            
            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
            }
            
            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                Debug.LogWarning(GetHostServerURL());
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                initializationOperation = package.InitializeAsync(createParameters);
            }
            
            // WebGL运行模式
            if (playMode == EPlayMode.WebPlayMode)
            {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            var createParameters = new WebPlayModeParameters();
			string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            createParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
            initializationOperation = package.InitializeAsync(createParameters);
#else
                var createParameters = new WebPlayModeParameters();
                createParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
#endif
            }
            
            yield return initializationOperation;

            // 如果初始化失败弹出提示界面
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning($"{initializationOperation.Error}");
                PatchEventDefine.InitializeFailed.SendEventMessage();
            }
            else
            {
                machine.ChangeState<FsmRequestPackageVersion>();
            }
        }
        
         /// <summary>
    /// 获取资源服务器地址
    /// </summary>
    //     private string GetHostServerURL()
    //     {
    //        
    //         //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
    //         string hostServerIP = "http://127.0.0.1";
    //         string appVersion = "v1.0";
    //
    // #if UNITY_EDITOR
    //         if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
    //             return $"{hostServerIP}/CDN/Android/{appVersion}";
    //         else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
    //             return $"{hostServerIP}/CDN/IPhone/{appVersion}";
    //         else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
    //             return $"{hostServerIP}/CDN/WebGL/{appVersion}";
    //         else
    //             return $"{hostServerIP}/CDN/PC/{appVersion}";
    // #else
    //         if (Application.platform == RuntimePlatform.Android)
    //             return $"{hostServerIP}/CDN/Android/{appVersion}";
    //         else if (Application.platform == RuntimePlatform.IPhonePlayer)
    //             return $"{hostServerIP}/CDN/IPhone/{appVersion}";
    //         else if (Application.platform == RuntimePlatform.WebGLPlayer)
    //             return $"{hostServerIP}/CDN/WebGL/{appVersion}";
    //         else
    //             return $"{hostServerIP}/CDN/PC/{appVersion}";
    // #endif
    //     }
    
    private string GetHostServerURL()
    {
        string appVersion = "2025-11-24-678";     // 使用实际的版本目录
        string port = "80";
    
        return $"http://127.0.0.1:{port}/{appVersion}";
    }

        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteServices
        {
            private readonly string defaultHostServer;
            private readonly string fallbackHostServer;

            public RemoteServices(string defaultHostServer, string fallbackHostServer)
            {
                this.defaultHostServer = defaultHostServer;
                this.fallbackHostServer = fallbackHostServer;
            }
            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return $"{defaultHostServer}/{fileName}";
            }
            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return $"{fallbackHostServer}/{fileName}";
            }
        }
    }
}