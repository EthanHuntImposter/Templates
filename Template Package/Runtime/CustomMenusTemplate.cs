using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Template
{
    public class CustomMenusTemplate : MonoBehaviour
    {
        [MenuItem("Template/Instantiate Selected Prefab")]
        static void SpawnSelectedPrefab() {
            Selection.activeObject = PrefabUtility.InstantiatePrefab(Selection.activeObject as GameObject);

            if (Selection.activeObject == null) { Debug.LogWarning("No Prefab Selected"); }
        }
    }
}
