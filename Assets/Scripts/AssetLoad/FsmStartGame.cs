namespace AssetLoad
{
    public class FsmStartGame: IStateNode
    {
        private PatchOperation owner;
        
        public void OnCreate(StateMachine machine)
        {
            this.owner = machine.Owner as PatchOperation;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("开始游戏！");
            owner.SetFinish();
        }

        public void OnUpdate()
        {
            
        }

        public void OnExit()
        {
        }
    }
}