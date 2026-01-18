using System;
using Unit;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Add sound when clicking on a button
    /// </summary>
    public class ButtonAudio:MonoBehaviour
    {
        public AudioClip clickAudio;

        private void Start()
        {
            Button btn = GetComponent<Button>();
            btn.onClick.AddListener(OnClick);
            
        }

        private void OnClick()
        {
            AudioTool.Get().PlaySFX("ui", clickAudio);
        }
    }
}