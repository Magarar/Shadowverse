using YooAsset;

namespace AssetLoad
{
    public class FsmClearCacheBundle: IStateNode
    {
        private StateMachine machine;
        public void OnCreate(StateMachine machine)
        {
            this.machine = machine;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("清理未使用的缓存文件！");
            var packageName = (string)machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            operation.Completed += OperationCompleted;
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }
        
        private void OperationCompleted(YooAsset.AsyncOperationBase obj)
        {
            machine.ChangeState<FsmStartGame>();
        }
    }
}