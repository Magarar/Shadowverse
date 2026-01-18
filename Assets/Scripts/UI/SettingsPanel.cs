using System.Collections.Generic;
using TMPro;
using Unit;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingsPanel:UIPanel
    {
        public string tabGroup;
        public SliderDrag masterVol;
        public SliderDrag musicVol;
        public SliderDrag sfxVol;
        public SliderDrag quality;
        public SliderDrag resolution;
        public Toggle windowed;
        
        public TextMeshProUGUI masterVolText;
        public TextMeshProUGUI musicVolText;
        public TextMeshProUGUI sfxVolText;
        public TextMeshProUGUI qualityText;
        public TextMeshProUGUI resolutionText;
        
        public static HashSet<string> resoHash = new HashSet<string>();
        public static List<Resolution> resolutions = new List<Resolution>();
        private bool refreshing = false;
        
        private static SettingsPanel instance;
        
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Start()
        {
            base.Start();
            //Default min max
            masterVol.minValue = 0;
            masterVol.maxValue = 100;
            musicVol.minValue = 0;
            musicVol.maxValue = 100;
            sfxVol.minValue = 0;
            sfxVol.maxValue = 100;
            quality.minValue = 0;
            resolution.minValue = 0;
            
            masterVol.onValueChanged += RefreshText;
            musicVol.onValueChanged += RefreshText;
            sfxVol.onValueChanged += RefreshText;
            quality.onValueChanged += RefreshText;
            resolution.onValueChanged += RefreshText;

            masterVol.onEndDrag += OnChangeAudio;
            musicVol.onEndDrag += OnChangeAudio;
            sfxVol.onEndDrag += OnChangeAudio;
            quality.onEndDrag += OnChangeQuality;
            resolution.onEndDrag += OnChangeResolution;
            windowed.onValueChanged.AddListener(OnChangeWindowed);
            
            foreach (Resolution reso in Screen.resolutions)
            {
                string resoTag = reso.width + "x" + reso.height;
                if (!resoHash.Contains(resoTag))
                {
                    resolutions.Add(reso);
                    resoHash.Add(resoTag);
                }
            }
            
            quality.maxValue = QualitySettings.names.Length - 1;
            resolution.maxValue = resolutions.Count - 1;
            
            foreach (TabButton btn in TabButton.GetAll(tabGroup))
                btn.onClick += OnClickTab;
        }

        private void RefreshPanel()
        {
            refreshing = true;
            masterVol.value = AudioTool.Get().masterVol * 100f;
            musicVol.value = AudioTool.Get().musicVol * 100f;
            sfxVol.value = AudioTool.Get().sfxVol * 100f;
            
            int qualityValue = QualitySettings.GetQualityLevel();
            int resoValue = GetResolutionIndex();
            bool windowedValue = !Screen.fullScreen;
            
            quality.value = qualityValue;
            resolution.value = resoValue;
            windowed.isOn = windowedValue;
            refreshing = false;
            
            RefreshText();
        }

        private void RefreshText()
        {
            masterVolText.text = masterVol.value.ToString();
            musicVolText.text = musicVol.value.ToString();
            sfxVolText.text = sfxVol.value.ToString();
            
            int qualityValue = Mathf.RoundToInt(quality.value);
            qualityText.text = QualitySettings.names[qualityValue];
            resolutionText.text = "";
            
            int resoValue = Mathf.RoundToInt(resolution.value);
            if (resolutions.Count > 0)
            {
                Resolution resolu = resolutions[resoValue];
                string resoTag = resolu.width + "x" + resolu.height + " " + Screen.currentResolution.refreshRate + "Hz";
                resolutionText.text = resoTag;
            }
        }

        private void OnChangeAudio()
        {
            if (!refreshing)
            {
                AudioTool.Get().masterVol = masterVol.value / 100f;
                AudioTool.Get().sfxVol = sfxVol.value / 100f;
                AudioTool.Get().musicVol = musicVol.value / 100f;
                AudioTool.Get().RefreshVolume();
                AudioTool.Get().SavePrefs();
                RefreshText();
            }
        }
        
        private void OnChangeQuality()
        {
            if (!refreshing)
            {
                int qualityValue = Mathf.RoundToInt(quality.value);
                QualitySettings.SetQualityLevel(qualityValue);
                RefreshText();
            }
        }
        
        private void OnChangeResolution()
        {
            if (!refreshing && resolutions.Count > 0)
            {
                int resoValue = Mathf.RoundToInt(resolution.value);
                Resolution resolu = resolutions[resoValue];
                Screen.SetResolution(resolu.width, resolu.height, !windowed.isOn);
                RefreshText();
            }
        }
        
        private void OnChangeWindowed(bool val)
        {
            OnChangeResolution();
        }
        
        private void OnClickTab()
        {
            Hide();
        }
        
        public void OnClickOK()
        {
            Hide();
        }
        
        private int GetResolutionIndex()
        {
            int distMin = 99999;
            int closest = 0;
            for (int i=0; i<resolutions.Count; i++)
            {
                Resolution res = resolutions[i];
                int dist = Mathf.Abs(res.height - Screen.height) + Mathf.Abs(res.width - Screen.width);
                if (dist < distMin)
                {
                    distMin = dist;
                    closest = i;
                }
            }
            return closest;
        }
        
        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }
        
        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
        }

        public static SettingsPanel Get()
        {
            return instance;
        }
    }
}