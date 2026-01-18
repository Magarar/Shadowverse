using UnityEngine;
using YooAsset;

namespace AssetLoad
{
    public class FsmCreateDownloader: IStateNode
    {
        private StateMachine machine;


        public void OnCreate(StateMachine machine)
        {
            this.machine = machine;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("创建资源下载器！");
            CreateDownloader();
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }
        
        void CreateDownloader()
        {
            var packageName = (string)machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            machine.SetBlackboardValue("Downloader", downloader);

            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("Not found any download files !");
                machine.ChangeState<FsmStartGame>();
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;
                PatchEventDefine.FoundUpdateFiles.SendEventMessage(totalDownloadCount, totalDownloadBytes);
            }
        }
    }
}