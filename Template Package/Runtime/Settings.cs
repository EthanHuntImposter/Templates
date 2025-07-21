using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

namespace Template {
    public class Settings : MonoBehaviour {
        [HideInInspector] public static Settings instance;

        private void Awake() {
            if (instance == null) {
                instance = this;
            }
            else {
                Destroy(gameObject);
            }
        }

        //Subscribe and unsibscribe from SettingsUI events
        private void OnEnable() {
            SettingsUI.FullscreenClicked += Fullscreen;
            SettingsUI.ResolutionChanged += ChangeResolution;
            SettingsUI.VSyncChanged += ChangeVsync;
            SettingsUI.QualityChanged += ChangeQuality;

            SettingsUI.MuteClicked += MuteClicked;
            SettingsUI.MasterVolChanged += NewMasterVolume;
            SettingsUI.MusicVolChanged += NewMusicVolume;
            SettingsUI.SFXVolChanged += NewSFXVolume;
        }

        private void OnDisable() {
            SettingsUI.FullscreenClicked -= Fullscreen;
            SettingsUI.ResolutionChanged -= ChangeResolution;
            SettingsUI.VSyncChanged -= ChangeVsync;
            SettingsUI.QualityChanged -= ChangeQuality;

            SettingsUI.MuteClicked -= MuteClicked;
            SettingsUI.MasterVolChanged -= NewMasterVolume;
            SettingsUI.MusicVolChanged -= NewMusicVolume;
            SettingsUI.SFXVolChanged -= NewSFXVolume;
        }

        private void Start() {
            //Removes itself from parent game object then stop self from being destroyed on scene change
            gameObject.transform.parent = null;
            DontDestroyOnLoad(gameObject);

            //Setup default values for all settings for if player saves without changing that setting
            resolutions = Screen.resolutions; for (int i = 0; i < resolutions.Length; i++) {
                if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height) {
                    currentRes = i;
                }
            }
            newResIndexCache = resolutions.Length;
            fsm = Screen.fullScreenMode;
            fullscreened = fsm == FullScreenMode.FullScreenWindow;
            vSyncCount = QualitySettings.vSyncCount;
            quality = QualitySettings.GetQualityLevel();
        }

        bool settingsMenuOpen = false;

        // Update is called once per frame
        void Update() {
            //Menu open/close
            if (InputManager.instance.SettingsMenu) {
                if (settingsMenuOpen) {
                    settingsMenuOpen = false;
                    SettingsUI.instance.CloseSettings();
                }
                else {
                    settingsMenuOpen = true;
                    SettingsUI.instance.OpenSettings();
                }
            }

            //Save settings test location
            if (Input.GetKeyDown(KeyCode.Alpha0)) {
                SaveSettings.Save();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                SaveSettings.Load();
            }
        }

        [Header("Graphics")]
        Resolution[] resolutions;
        int currentRes;

        FullScreenMode fsm;

        bool fullscreened;

        //Swap from fullscreen-windowed to windowed
        void Fullscreen() {
            if (fullscreened) {
                fsm = FullScreenMode.Windowed;
                fullscreened = false;
            }
            else {
                fsm = FullScreenMode.FullScreenWindow;
                fullscreened = true;
            }

            Screen.fullScreenMode = fsm;

            ChangeResolution(newResIndexCache);
        }

        int newResIndexCache; //Re set resolution when leaving fullscreen
        void ChangeResolution(int newResIndex) {
            newResIndexCache = newResIndex;

            if (fullscreened) { return; }
            //Set resolution based width and height - no referesh rate
            Screen.SetResolution(resolutions[newResIndex].width, resolutions[newResIndex].height, fsm);
            currentRes = newResIndex;
        }

        //Swaps between no vync and every v blank - can change to dropdown to also include every second v blank
        int vSyncCount;
        void ChangeVsync() {
            if (QualitySettings.vSyncCount == 0) {
                QualitySettings.vSyncCount = 1;
            }
            else {
                QualitySettings.vSyncCount = 0;
            }
            vSyncCount = QualitySettings.vSyncCount;

            SettingsUI.instance.UpdateVSyncText();
        }

        //Change unity quality settings
        int quality;
        void ChangeQuality(int index) {
            QualitySettings.SetQualityLevel(index);
            quality = index;
        }

        [Header("Audio")]
        [SerializeField] AudioMixer mixer;

        bool muted = false;
        float masterVolCache = 1f, musicVolCache = 1f, sfxVolCache = 1f;

        //Caches volume level of all sliders then sets value of each to lowest
        //Resets to cached value on unmute
        void MuteClicked() {
            if (muted) {
                NewMasterVolume(masterVolCache);
                NewMusicVolume(musicVolCache);
                NewSFXVolume(sfxVolCache);

                muted = false;
            }
            else {
                mixer.SetFloat("MasterVolume", Mathf.Log10(0.0001f) * 20);
                mixer.SetFloat("MusicVolume", Mathf.Log10(0.0001f) * 20);
                mixer.SetFloat("SFXVolume", Mathf.Log10(0.0001f) * 20);

                muted = true;
            }
        }

        void NewMasterVolume(float newVol) {
            mixer.SetFloat("MasterVolume", Mathf.Log10(newVol) * 20);
            masterVolCache = newVol;
        }

        void NewMusicVolume(float newVol) {
            mixer.SetFloat("MusicVolume", Mathf.Log10(newVol) * 20);
            musicVolCache = newVol;
        }

        void NewSFXVolume(float newVol) {
            mixer.SetFloat("SFXVolume", Mathf.Log10(newVol) * 20);
            sfxVolCache = newVol;
        }



        //Saving
        public void Save(ref SettingsSaveData saveData) {
            saveData.resolution = currentRes;
            saveData.fullScreenMode = fsm;
            saveData.isFullscreened = fullscreened;
            saveData.vSyncCount = vSyncCount;
            saveData.quality = quality;

            saveData.isMuted = muted;
            saveData.masterVol = masterVolCache;
            saveData.musicVol = musicVolCache;
            saveData.sfxVol = sfxVolCache;

            saveData.inputs = InputManager.instance.InputsSave();
        }

        public void Load(SettingsSaveData saveData) {
            ChangeResolution(saveData.resolution);
            Screen.fullScreenMode = saveData.fullScreenMode;
            fullscreened = saveData.isFullscreened;
            vSyncCount = saveData.vSyncCount;
            ChangeQuality(saveData.quality);

            muted = saveData.isMuted;
            NewMasterVolume(saveData.masterVol);
            NewMusicVolume(saveData.musicVol);
            NewSFXVolume(saveData.sfxVol);

            InputManager.instance.InputsLoad(saveData.inputs);

            SettingsUI.instance.UpdateSettingsMenuOnLoad(currentRes, quality, vSyncCount, masterVolCache, musicVolCache, sfxVolCache);
            SettingsUI.instance.UpdateControlsTextOnLoad();
        }
    }

    //Struct of all things saved to json file
    [System.Serializable]
    public struct SettingsSaveData {

        public int resolution;
        public FullScreenMode fullScreenMode;
        public bool isFullscreened;
        public int vSyncCount;
        public int quality;

        public bool isMuted;
        public float masterVol;
        public float musicVol;
        public float sfxVol;

        public string inputs;
    }
}