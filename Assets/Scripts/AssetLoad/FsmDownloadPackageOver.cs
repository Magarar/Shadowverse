namespace AssetLoad
{
    public class FsmDownloadPackageOver: IStateNode
    {
        private StateMachine machine;
        public void OnCreate(StateMachine machine)
        {
            this.machine = machine;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("资源文件下载完毕！");
            machine.ChangeState<FsmClearCacheBundle>();
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }
    }
}