using UnityEngine;
using YooAsset;

namespace AssetLoad
{
    public class PatchOperation: GameAsyncOperation
    {
        private readonly EventGroup eventGroup = new EventGroup();
        private readonly StateMachine machine;
        private readonly string packageName;
        private ESteps steps = ESteps.None;

        public PatchOperation(string packageName, EPlayMode playMode)
        {
            this.packageName = packageName;
            
            eventGroup.AddListener<UserEventDefine.UserTryInitialize>(OnHandleEventMessage);
            eventGroup.AddListener<UserEventDefine.UserBeginDownloadWebFiles>(OnHandleEventMessage);
            eventGroup.AddListener<UserEventDefine.UserTryRequestPackageVersion>(OnHandleEventMessage);
            eventGroup.AddListener<UserEventDefine.UserTryUpdatePackageManifest>(OnHandleEventMessage);
            eventGroup.AddListener<UserEventDefine.UserTryDownloadWebFiles>(OnHandleEventMessage);
            
            machine = new StateMachine(this);
            machine.AddNode<FsmInitializePackage>();
            machine.AddNode<FsmRequestPackageVersion>();
            machine.AddNode<FsmUpdatePackageManifest>();
            machine.AddNode<FsmCreateDownloader>();
            machine.AddNode<FsmDownloadPackageFiles>();
            machine.AddNode<FsmDownloadPackageOver>();
            machine.AddNode<FsmClearCacheBundle>();
            machine.AddNode<FsmStartGame>();
            
            machine.SetBlackboardValue("PackageName", packageName); 
            machine.SetBlackboardValue("PlayMode", playMode);
        }
        
        protected override void OnStart()
        {
            steps = ESteps.Update;
            machine.Run<FsmInitializePackage>();
        }

        protected override void OnUpdate()
        {
            if (steps == ESteps.None || steps == ESteps.Done)
                return;

            if (steps == ESteps.Update)
            {
                machine.Update();
            }
        }
        
        public void SetFinish()
        {
            steps = ESteps.Done;
            eventGroup.RemoveAllListener();
            Status = EOperationStatus.Succeed;
            Debug.Log($"Package {packageName} patch done !");
        }

        protected override void OnAbort()
        {
            
        }
        
        private void OnHandleEventMessage(IEventMessage message)
        {
            if (message is UserEventDefine.UserTryInitialize)
            {
                machine.ChangeState<FsmInitializePackage>();
            }
            else if (message is UserEventDefine.UserBeginDownloadWebFiles)
            {
                machine.ChangeState<FsmDownloadPackageFiles>();
            }
            else if (message is UserEventDefine.UserTryRequestPackageVersion)
            {
                machine.ChangeState<FsmRequestPackageVersion>();
            }
            else if (message is UserEventDefine.UserTryUpdatePackageManifest)
            {
                machine.ChangeState<FsmUpdatePackageManifest>();
            }
            else if (message is UserEventDefine.UserTryDownloadWebFiles)
            {
                machine.ChangeState<FsmCreateDownloader>();
            }
            else
            {
                throw new System.NotImplementedException($"{message.GetType()}");
            }
        }
        
        private enum ESteps
        {
            None,
            Update,
            Done,
        }
        
    }
    
  
}