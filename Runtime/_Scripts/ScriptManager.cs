using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Template {
    public class ScriptManager : MonoBehaviour {
        [HideInInspector] public static ScriptManager instance;

        private void Awake() {
            if (instance == null) {
                instance = this;
            }
            else {
                Destroy(gameObject);
            }
        }

        //References to scripts for saving purposes
        public Settings Settings { get; set; }
        public SettingsUI SettingsUI { get; set; }
        public InputManager InputManager { get; set; }
    }
}
