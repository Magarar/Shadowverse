using System.Collections;
using YooAsset;

namespace AssetLoad
{
    public class FsmDownloadPackageFiles: IStateNode
    {
        private StateMachine machine;
        public void OnCreate(StateMachine machine)
        {
            this.machine = machine;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("开始下载资源文件！");
            GameAsset.Get().StartCoroutine(BeginDownload());
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }
        
        private IEnumerator BeginDownload()
        {
            var downloader = (ResourceDownloaderOperation)machine.GetBlackboardValue("Downloader");
            downloader.DownloadErrorCallback = PatchEventDefine.WebFileDownloadFailed.SendEventMessage;
            downloader.DownloadUpdateCallback = PatchEventDefine.DownloadUpdate.SendEventMessage;
            downloader.BeginDownload();
            yield return downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                yield break;

            machine.ChangeState<FsmDownloadPackageOver>();
        }
    }
}