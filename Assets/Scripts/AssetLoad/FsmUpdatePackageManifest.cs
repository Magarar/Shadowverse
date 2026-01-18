using System.Collections;
using UnityEngine;
using YooAsset;

namespace AssetLoad
{
    public class FsmUpdatePackageManifest: IStateNode
    {
        private StateMachine machine;

        public void OnCreate(StateMachine machine)
        {
            this.machine = machine;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("更新资源清单！");
            GameAsset.Get().StartCoroutine(UpdateManifest());
        }

        public void OnUpdate()
        {
            
        }

        public void OnExit()
        {
            
        }
        
        private IEnumerator UpdateManifest()
        {
            var packageName = (string)machine.GetBlackboardValue("PackageName");
            var packageVersion = (string)machine.GetBlackboardValue("PackageVersion");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
                PatchEventDefine.PackageManifestUpdateFailed.SendEventMessage();
                yield break;
            }
            else
            {
                machine.ChangeState<FsmCreateDownloader>();
            }
        }
    }
}