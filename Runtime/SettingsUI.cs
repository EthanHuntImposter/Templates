using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Template {
    public class SettingsUI : MonoBehaviour {
        [HideInInspector] public static SettingsUI instance;

        private void Awake() {
            if (instance == null) {
                instance = this;
            }
            else {
                Destroy(gameObject);
            }
        }

        [SerializeField] UIDocument doc;
        [SerializeField] StyleSheet style;

        // Start is called before the first frame update
        void Start() {
            //Removes itself from parent game object then stop self from being destroyed on scene change
            gameObject.transform.parent = null;
            DontDestroyOnLoad(gameObject);

            StartCoroutine(Generate());
        }

        private void OnValidate() {
            if (Application.isPlaying) { return; } //Dont run twice if playing

            if(PrefabUtility.GetPrefabInstanceStatus(gameObject) == PrefabInstanceStatus.NotAPrefab) {  return; } //Returns from prefab menu (avoids coroutine errors)

            StartCoroutine(Generate());
        }

        //Elements that need to be accessed outside Generate func
        VisualElement root;
        VisualElement container;
        VisualElement tabBox;

        Button generalBtn, gameplayBtn, graphicsBtn, audioBtn, controlsBtn;

        VisualElement generalSettingsBox, gameplaySettingsBox, graphicsSettingsBox, audioSettingsBox, controlsSettingsBox;

        DropdownField resolutionDropdown;
        DropdownField qualitySettings;
        TextElement vSyncText;

        Slider masterVolSlider, musicVolSlider, sfxVolSlider;

        VisualElement remapWarning; //Warning message when player remaps a key
        TextElement forwardsKey, backwardsKey, leftKey, rightKey, settingsKey, inventoryKey;
        TextElement currentRemapKey;

        private IEnumerator Generate() {
            yield return null;

            resolutions = Screen.resolutions;

            //Root setup
            root = doc.rootVisualElement;
            root.Clear();
            root.styleSheets.Add(style);
            root.style.display = DisplayStyle.None;

            container = Create("container");

            //Tab Buttons
            tabBox = Create("tab-box", "bordered-box");
            container.Add(tabBox);
            root.Add(container);

            generalBtn = Create<Button>("tab-button");
            generalBtn.text = "General";

            gameplayBtn = Create<Button>("tab-button");
            gameplayBtn.text = "Gameplay";

            graphicsBtn = Create<Button>("tab-button");
            graphicsBtn.text = "Graphics";

            audioBtn = Create<Button>("tab-button");
            audioBtn.text = "Audio";

            controlsBtn = Create<Button>("tab-button");
            controlsBtn.text = "Controls";

            tabBox.Add(generalBtn);
            tabBox.Add(gameplayBtn);
            tabBox.Add(graphicsBtn);
            tabBox.Add(audioBtn);
            tabBox.Add(controlsBtn);

            //Tab button funcs
            generalBtn.clicked += showGeneral;
            gameplayBtn.clicked += showGameplay;
            graphicsBtn.clicked += showGraphics;
            audioBtn.clicked += showAudio;
            controlsBtn.clicked += showControls;


            //Settings boxes for each type of settings
            #region General Settings
            generalSettingsBox = Create<ScrollView>("settings-box", "bordered-box");
            container.Add(generalSettingsBox);
            generalSettingsBox.style.display = DisplayStyle.Flex;
            #endregion


            #region Gameplay Settings
            gameplaySettingsBox = Create<ScrollView>("settings-box", "bordered-box");
            container.Add(gameplaySettingsBox);
            gameplaySettingsBox.style.display = DisplayStyle.None;

            #endregion


            #region Graphics Settings
            graphicsSettingsBox = Create<ScrollView>("settings-box", "bordered-box");
            container.Add(graphicsSettingsBox);
            graphicsSettingsBox.style.display = DisplayStyle.None;

            var _fullscreenBtn = Create<Button>();
            _fullscreenBtn.text = "Fullscreen";
            _fullscreenBtn.clicked += FullscreenClicked;
            graphicsSettingsBox.Add(_fullscreenBtn);

            resolutionDropdown = Create<DropdownField>("dropdown");
            graphicsSettingsBox.Add(resolutionDropdown);

            for (int i = 0; i < resolutions.Length; i++) {
                string _resReadable = resolutions[i].width + "x" + resolutions[i].height;
                resolutionDropdown.choices.Add(_resReadable);

                if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height) {
                    resolutionDropdown.index = i;
                }
            }
            resolutionDropdown.RegisterValueChangedCallback(i => ResolutionChanged?.Invoke(resolutionDropdown.index));

            var _qualitySettingsText = Create<TextElement>("main-text");
            _qualitySettingsText.text = "Quality Settings";
            graphicsSettingsBox.Add(_qualitySettingsText);

            qualitySettings = Create<DropdownField>("dropdown");
            qualitySettings.choices.Add("Very Low");
            qualitySettings.choices.Add("Low");
            qualitySettings.choices.Add("Medium");
            qualitySettings.choices.Add("High");
            qualitySettings.choices.Add("Very High");
            qualitySettings.choices.Add("Ultra");
            graphicsSettingsBox.Add(qualitySettings);
            qualitySettings.index = QualitySettings.GetQualityLevel();
            qualitySettings.RegisterValueChangedCallback(i => QualityChanged?.Invoke(qualitySettings.index));

            var _vSyncBtn = Create<Button>();
            _vSyncBtn.text = "VSync";
            _vSyncBtn.clicked += VSyncChanged;
            vSyncText = Create<TextElement>();
            vSyncText.text = "On";
            _vSyncBtn.Add(vSyncText);
            graphicsSettingsBox.Add(_vSyncBtn);
            #endregion


            #region Audio Settings
            audioSettingsBox = Create<ScrollView>("settings-box", "bordered-box");
            container.Add(audioSettingsBox);
            audioSettingsBox.style.display = DisplayStyle.None;

            var _muteBtn = Create<Button>();
            _muteBtn.text = "Mute";
            _muteBtn.clicked += MuteClicked;
            audioSettingsBox.Add(_muteBtn);

            var _masterTxt = Create<TextElement>();
            _masterTxt.text = "Master Volume";
            audioSettingsBox.Add(_masterTxt);

            masterVolSlider = Create<Slider>("volume-slider");
            masterVolSlider.lowValue = 0.0001f;
            masterVolSlider.highValue = 1f;
            masterVolSlider.value = 1f;

            masterVolSlider.RegisterValueChangedCallback(v => MasterVolChanged?.Invoke(v.newValue));
            audioSettingsBox.Add(masterVolSlider);

            var _musicTxt = Create<TextElement>();
            _musicTxt.text = "Music Volume";
            audioSettingsBox.Add(_musicTxt);

            musicVolSlider = Create<Slider>("volume-slider");
            musicVolSlider.lowValue = 0.0001f;
            musicVolSlider.highValue = 1f;
            musicVolSlider.value = 1f;

            musicVolSlider.RegisterValueChangedCallback(v => MusicVolChanged?.Invoke(v.newValue));
            audioSettingsBox.Add(musicVolSlider);

            var _sfxTxt = Create<TextElement>();
            _sfxTxt.text = "Sound Effects Volume";
            audioSettingsBox.Add(_sfxTxt);

            sfxVolSlider = Create<Slider>("volume-slider");
            sfxVolSlider.lowValue = 0.0001f;
            sfxVolSlider.highValue = 1f;
            sfxVolSlider.value = 1f;

            sfxVolSlider.RegisterValueChangedCallback(v => SFXVolChanged?.Invoke(v.newValue));
            audioSettingsBox.Add(sfxVolSlider);
            #endregion


            #region Controls Settings
            controlsSettingsBox = Create<ScrollView>("settings-box", "bordered-box");
            container.Add(controlsSettingsBox);
            controlsSettingsBox.style.display = DisplayStyle.None;

            //Remap warnings
            remapWarning = Create("warning-box");
            remapWarning.style.display = DisplayStyle.None;
            var _remapWarningText = Create<TextElement>();
            _remapWarningText.text = "Press Any Key To Remap";
            var _remapWarningText2 = Create<TextElement>("cancel-text");
            _remapWarningText2.text = "Press Delete To Cancel";
            remapWarning.Add(_remapWarningText);
            remapWarning.Add(_remapWarningText2);
            container.Add(remapWarning);

            //Control Buttons
            var _walkForwardBtn = Create<Button>();
            _walkForwardBtn.text = "Walk Forwards";
            forwardsKey = Create<TextElement>();
            forwardsKey.text = "W";
            _walkForwardBtn.Add(forwardsKey);
            controlsSettingsBox.Add(_walkForwardBtn);

            var _walkBackwardBtn = Create<Button>();
            _walkBackwardBtn.text = "Walk Backwards";
            backwardsKey = Create<TextElement>();
            backwardsKey.text = "S";
            _walkBackwardBtn.Add(backwardsKey);
            controlsSettingsBox.Add(_walkBackwardBtn);

            var _walkLeftBtn = Create<Button>();
            _walkLeftBtn.text = "Walk Left";
            leftKey = Create<TextElement>();
            leftKey.text = "A";
            _walkLeftBtn.Add(leftKey);
            controlsSettingsBox.Add(_walkLeftBtn);

            var _walkRightBtn = Create<Button>();
            _walkRightBtn.text = "Walk Right";
            rightKey = Create<TextElement>();
            rightKey.text = "D";
            _walkRightBtn.Add(rightKey);
            controlsSettingsBox.Add(_walkRightBtn);

            var _settingsBtn = Create<Button>();
            _settingsBtn.text = "Settings";
            settingsKey = Create<TextElement>();
            settingsKey.text = "ESCAPE";
            _settingsBtn.Add(settingsKey);
            controlsSettingsBox.Add(_settingsBtn);

            var _inventoryBtn = Create<Button>();
            _inventoryBtn.text = "Inventory";
            inventoryKey = Create<TextElement>();
            inventoryKey.text = "E";
            _inventoryBtn.Add(inventoryKey);
            controlsSettingsBox.Add(_inventoryBtn);

            //Avoid OnValidate errors
            if (Application.isPlaying) {
                _walkForwardBtn.RegisterCallback<ClickEvent, int>(InputManager.instance.RemapBindingCompositeIndex, 1);
                _walkForwardBtn.RegisterCallback<ClickEvent, InputAction>(InputManager.instance.RemapBinding, InputManager.instance.Move);
                _walkForwardBtn.RegisterCallback<ClickEvent, TextElement>(SetCurrentRemapKey, forwardsKey);

                _walkBackwardBtn.RegisterCallback<ClickEvent, int>(InputManager.instance.RemapBindingCompositeIndex, 2);
                _walkBackwardBtn.RegisterCallback<ClickEvent, InputAction>(InputManager.instance.RemapBinding, InputManager.instance.Move);
                _walkBackwardBtn.RegisterCallback<ClickEvent, TextElement>(SetCurrentRemapKey, backwardsKey);

                _walkLeftBtn.RegisterCallback<ClickEvent, int>(InputManager.instance.RemapBindingCompositeIndex, 3);
                _walkLeftBtn.RegisterCallback<ClickEvent, InputAction>(InputManager.instance.RemapBinding, InputManager.instance.Move);
                _walkLeftBtn.RegisterCallback<ClickEvent, TextElement>(SetCurrentRemapKey, leftKey);

                _walkRightBtn.RegisterCallback<ClickEvent, int>(InputManager.instance.RemapBindingCompositeIndex, 4);
                _walkRightBtn.RegisterCallback<ClickEvent, InputAction>(InputManager.instance.RemapBinding, InputManager.instance.Move);
                _walkRightBtn.RegisterCallback<ClickEvent, TextElement>(SetCurrentRemapKey, rightKey);


                _settingsBtn.RegisterCallback<ClickEvent, InputAction>(InputManager.instance.RemapBinding, InputManager.instance.Settings);
                _settingsBtn.RegisterCallback<ClickEvent, TextElement>(SetCurrentRemapKey, settingsKey);

                _inventoryBtn.RegisterCallback<ClickEvent, InputAction>(InputManager.instance.RemapBinding, InputManager.instance.Inventory);
                _inventoryBtn.RegisterCallback<ClickEvent, TextElement>(SetCurrentRemapKey, inventoryKey);
            }
            #endregion
        }

        private Resolution[] resolutions;

        #region Settings Events
        public static event Action FullscreenClicked;
        public static event Action<int> ResolutionChanged;
        public static event Action<int> QualityChanged;
        public static event Action VSyncChanged;

        public static event Action MuteClicked;
        public static event Action<float> MasterVolChanged;
        public static event Action<float> MusicVolChanged;
        public static event Action<float> SFXVolChanged;

        #endregion

        #region In Menu Events
        //Funcs for hiding and showing different settings tabs
        private static event Action showGeneral;
        private static event Action showGameplay;
        private static event Action showGraphics;
        private static event Action showAudio;
        private static event Action showControls;

        private void OnEnable() {
            showGeneral += ShowGeneralSettings;

            showGameplay += ShowGameplaySettings;

            showGraphics += ShowGraphicsSettings;

            showAudio += ShowAudioSettings;

            showControls += ShowControlsSettings;

            InputManager.ShowRemapWarning += ShowRemapWarning;
            InputManager.NewRemapKey += SetNewRemapKeyText;
        }

        private void OnDisable() {
            showGeneral -= ShowGeneralSettings;

            showGameplay -= ShowGameplaySettings;

            showGraphics -= ShowGraphicsSettings;

            showAudio -= ShowAudioSettings;

            showControls -= ShowControlsSettings;

            InputManager.ShowRemapWarning -= ShowRemapWarning;
            InputManager.NewRemapKey -= SetNewRemapKeyText;
        }

        //Opening and closing WHOLE menu
        bool menuOpen = false;
        public void OpenSettings() {
            if (menuOpen) { return; }
            menuOpen = true;

            root.style.display = DisplayStyle.Flex;
            ShowGeneralSettings();
        }

        public void CloseSettings() {
            if (!menuOpen) { return; }
            menuOpen = false;

            root.style.display = DisplayStyle.None;
        }

        //Hides all boxes before setting one box to visible
        void HideSettingsBoxes() {
            generalSettingsBox.style.display = DisplayStyle.None;
            gameplaySettingsBox.style.display = DisplayStyle.None;
            graphicsSettingsBox.style.display = DisplayStyle.None;
            audioSettingsBox.style.display = DisplayStyle.None;
            controlsSettingsBox.style.display = DisplayStyle.None;
        }
        void ShowGeneralSettings() {
            HideSettingsBoxes();
            generalSettingsBox.style.display = DisplayStyle.Flex;
        }
        void ShowGameplaySettings() {
            HideSettingsBoxes();
            gameplaySettingsBox.style.display = DisplayStyle.Flex;
        }
        void ShowGraphicsSettings() {
            HideSettingsBoxes();
            graphicsSettingsBox.style.display = DisplayStyle.Flex;
        }
        void ShowAudioSettings() {
            HideSettingsBoxes();
            audioSettingsBox.style.display = DisplayStyle.Flex;
        }
        void ShowControlsSettings() {
            HideSettingsBoxes();
            controlsSettingsBox.style.display = DisplayStyle.Flex;
        }

        //UI updates from other scripts
        public void UpdateVSyncText() {
            if (vSyncText.text == "On") {
                vSyncText.text = "Off";
            }
            else {
                vSyncText.text = "On";
            }
        }

        public void ShowRemapWarning() {
            if (remapWarning.style.display == DisplayStyle.Flex) {
                remapWarning.style.display = DisplayStyle.None;
            }
            else {
                remapWarning.style.display = DisplayStyle.Flex;
            }
        }

        void SetCurrentRemapKey(ClickEvent ev, TextElement ele) {
            currentRemapKey = ele;
        }
        void SetNewRemapKeyText(string newText) {
            currentRemapKey.text = newText;
        }

        public void UpdateSettingsMenuOnLoad(int resIndex, int qualityIndex, int vSyncCount, float masterVol, float musicVol, float sfxVol) {
            resolutionDropdown.index = resIndex;
            qualitySettings.index = qualityIndex;
            vSyncText.text = vSyncCount == 0 ? "On" : "Off";

            masterVolSlider.value = masterVol;
            musicVolSlider.value = musicVol;
            sfxVolSlider.value = sfxVol;
        }

        public void UpdateControlsTextOnLoad() {
            forwardsKey.text = InputManager.instance.Move.bindings[1].ToDisplayString().ToUpper();
            backwardsKey.text = InputManager.instance.Move.bindings[2].ToDisplayString().ToUpper();
            leftKey.text = InputManager.instance.Move.bindings[3].ToDisplayString().ToUpper();
            rightKey.text = InputManager.instance.Move.bindings[4].ToDisplayString().ToUpper();
            settingsKey.text = InputManager.instance.Settings.bindings[0].ToDisplayString().ToUpper();
            inventoryKey.text = InputManager.instance.Inventory.bindings[0].ToDisplayString().ToUpper();
        }
        #endregion

        /// <summary>
        /// Creates a visual element when generic function does give T
        /// </summary>
        /// <param name="classNames">Names of classes this element will be added to</param>
        /// <returns></returns>
        VisualElement Create(params string[] classNames) {
            return Create<VisualElement>(classNames);
        }

        /// <summary>
        /// Generic function for creating elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classNames">Names of classes this element will be added to</param>
        /// <returns></returns>
        T Create<T>(params string[] classNames) where T : VisualElement, new() {
            var element = new T();
            foreach (var className in classNames) {
                element.AddToClassList(className);
            }
            return element;
        }
    }
}
