using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Template {
    public class SaveSettings {
        private static SaveData saveData = new SaveData();

        [System.Serializable]
        public struct SaveData {
            public SettingsSaveData SettingsSaveData;
        }

        //Gets file name as string
        public static string SaveFileName() => Application.persistentDataPath + "/save" + ".json";

        //All save functions run in here then write to json file
        public static void Save() {
            ScriptManager.instance.Settings.Save(ref saveData.SettingsSaveData);

            File.WriteAllText(SaveFileName(), JsonUtility.ToJson(saveData));
        }

        //Json file read to save data then all load functions run
        public static void Load() {
            string fileText = File.ReadAllText(SaveFileName());
            saveData = JsonUtility.FromJson<SaveData>(fileText);

            ScriptManager.instance.Settings.Load(saveData.SettingsSaveData);
        }
    }
}