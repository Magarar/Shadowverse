using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace AssetLoad
{
    public class FsmRequestPackageVersion: IStateNode
    {
        private StateMachine machine;
        
        public void OnCreate(StateMachine machine)
        {
            this.machine = machine;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage("请求资源版本 !");
            GameAsset.Get().StartCoroutine(UpdatePackageVersion());
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }
        
        private IEnumerator UpdatePackageVersion()
        {
            var packageName = (string)machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
                PatchEventDefine.PackageVersionRequestFailed.SendEventMessage();
            }
            else
            {
                Debug.Log($"Request package version : {operation.PackageVersion}");
                machine.SetBlackboardValue("PackageVersion", operation.PackageVersion);
                machine.ChangeState<FsmUpdatePackageManifest>();
            }
        }
    }
}