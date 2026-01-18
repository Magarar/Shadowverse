using System.Collections;
using Unit;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

namespace AssetLoad
{
    public class GameAsset:MonoBehaviour
    {
        /// <summary>
        /// 资源系统运行模式
        /// </summary>
        public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;
        
        private static GameAsset instance;
        
        public void Awake()
        {
            Debug.Log($"资源系统运行模式：{PlayMode}");
            instance = this;
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);
        }

        IEnumerator Start()
        {
            //StartCoroutine(TestRequest());
            UniEvent.Initalize();

            YooAssets.Initialize();
            
            
            //开始补丁更新流程
            var operation = new PatchOperation("DefaultPackage", PlayMode);
            YooAssets.StartOperation(operation);
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                // 设置默认的资源包
                var package = YooAssets.GetPackage("DefaultPackage");
                YooAssets.SetDefaultPackage(package);

                // Debug.Log("Available assets in package:");
                // var assetInfos = package.GetAllAssetInfos();
                // foreach (var assetInfo in assetInfos)
                // {
                //     Debug.Log($"Address: {assetInfo.Address}, Path: {assetInfo.AssetPath}");
                // }
                SceneNav.GoTo("LoginMenu");
            }
            else
            {
                Debug.LogError("补丁更新失败");
            }
            
            
        }
        
        IEnumerator TestRequest()
        {
            string url = "http://localhost:80/2025-11-17-656/DefaultPackage.version";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"TestRequest Failed: {request.error}");
                }
                else
                {
                    Debug.LogWarning($"TestRequest Success: {request.downloadHandler.text}");
                }
            }
        }
        
        public static GameAsset Get()
        {
            return instance;
        }
    }
}