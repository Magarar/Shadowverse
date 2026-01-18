using System.Collections.Generic;
using AssetLoad;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Menu
{
    public class PatchMenu : MonoBehaviour
    {
        // Start is called before the first frame update
        private readonly EventGroup eventGroup = new EventGroup();
        private readonly List<MessageBox> msgBoxList = new List<MessageBox>();

        // UGUI相关
        public GameObject messageBoxObj;
        public Slider slider;
        public TextMeshProUGUI tips;
        void Awake()
        {
            tips.text = "Initializing the game world !";
            messageBoxObj.SetActive(false);
            
            eventGroup.AddListener<PatchEventDefine.InitializeFailed>(OnHandleEventMessage);
            eventGroup.AddListener<PatchEventDefine.PatchStepsChange>(OnHandleEventMessage);
            eventGroup.AddListener<PatchEventDefine.FoundUpdateFiles>(OnHandleEventMessage);
            eventGroup.AddListener<PatchEventDefine.DownloadUpdate>(OnHandleEventMessage);
            eventGroup.AddListener<PatchEventDefine.PackageVersionRequestFailed>(OnHandleEventMessage);
            eventGroup.AddListener<PatchEventDefine.PackageManifestUpdateFailed>(OnHandleEventMessage);
            eventGroup.AddListener<PatchEventDefine.WebFileDownloadFailed>(OnHandleEventMessage);
        }
        
        private void OnDestroy()
        {
            eventGroup.RemoveAllListener();
        }
        
         private void OnHandleEventMessage(IEventMessage message)
        {
            if (message is PatchEventDefine.InitializeFailed)
            {
                System.Action callback = () =>
                {
                    UserEventDefine.UserTryInitialize.SendEventMessage();
                };
                ShowMessageBox($"Failed to initialize package !", callback);
            }
            else if (message is PatchEventDefine.PatchStepsChange)
            {
                var msg = message as PatchEventDefine.PatchStepsChange;
                tips.text = msg.Tips;
                UnityEngine.Debug.Log(msg.Tips);
            }
            else if (message is PatchEventDefine.FoundUpdateFiles)
            {
                var msg = message as PatchEventDefine.FoundUpdateFiles;
                System.Action callback = () =>
                {
                    UserEventDefine.UserBeginDownloadWebFiles.SendEventMessage();
                };
                float sizeMB = msg.TotalSizeBytes / 1048576f;
                sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
                string totalSizeMB = sizeMB.ToString("f1");
                ShowMessageBox($"Found update patch files, Total count {msg.TotalCount} Total szie {totalSizeMB}MB", callback);
            }
            else if (message is PatchEventDefine.DownloadUpdate)
            {
                var msg = message as PatchEventDefine.DownloadUpdate;
                slider.value = (float)msg.CurrentDownloadCount / msg.TotalDownloadCount;
                string currentSizeMB = (msg.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
                string totalSizeMB = (msg.TotalDownloadSizeBytes / 1048576f).ToString("f1");
                tips.text = $"{msg.CurrentDownloadCount}/{msg.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
            }
            else if (message is PatchEventDefine.PackageVersionRequestFailed)
            {
                System.Action callback = () =>
                {
                    UserEventDefine.UserTryRequestPackageVersion.SendEventMessage();
                };
                ShowMessageBox($"Failed to request package version, please check the network status.", callback);
            }
            else if (message is PatchEventDefine.PackageManifestUpdateFailed)
            {
                System.Action callback = () =>
                {
                    UserEventDefine.UserTryUpdatePackageManifest.SendEventMessage();
                };
                ShowMessageBox($"Failed to update patch manifest, please check the network status.", callback);
            }
            else if (message is PatchEventDefine.WebFileDownloadFailed)
            {
                var msg = message as PatchEventDefine.WebFileDownloadFailed;
                System.Action callback = () =>
                {
                    UserEventDefine.UserTryDownloadWebFiles.SendEventMessage();
                };
                ShowMessageBox($"Failed to download file : {msg.FileName}", callback);
            }
            else
            {
                throw new System.NotImplementedException($"{message.GetType()}");
            }
        }
         
        private void ShowMessageBox(string content, System.Action ok)
        {
            // 尝试获取一个可用的对话框
            MessageBox msgBox = null;
            for (int i = 0; i < msgBoxList.Count; i++)
            {
                var item = msgBoxList[i];
                if (item.ActiveSelf == false)
                {
                    msgBox = item;
                    break;
                }
            }

            // 如果没有可用的对话框，则创建一个新的对话框
            if (msgBox == null)
            {
                msgBox = new MessageBox();
                var cloneObject = GameObject.Instantiate(messageBoxObj, messageBoxObj.transform.parent);
                msgBox.Create(cloneObject);
                msgBoxList.Add(msgBox);
            }

            // 显示对话框
            msgBox.Show(content, ok);
        }
        
        private class MessageBox
        {
            private GameObject cloneObject;
            private TextMeshProUGUI content;
            private Button btnOK;
            private System.Action clickOK;

            public bool ActiveSelf => cloneObject.activeSelf;

            public void Create(GameObject cloneObject)
            {
                this.cloneObject = cloneObject;
                content = cloneObject.transform.Find("txtContent").GetComponent<TextMeshProUGUI>();
                btnOK = cloneObject.transform.Find("btnOK").GetComponent<Button>();
                btnOK.onClick.AddListener(OnClickYes);
            }
            
            public void Show(string content, System.Action clickOK)
            {
                this.content.text = content;
                this.clickOK = clickOK;
                cloneObject.SetActive(true);
                cloneObject.transform.SetAsLastSibling();
            }
            
            public void Hide()
            {
                content.text = string.Empty;
                clickOK = null;
                cloneObject.SetActive(false);
            }
            
            private void OnClickYes()
            {
                clickOK?.Invoke();
                Hide();
            }
        }
    }
}
