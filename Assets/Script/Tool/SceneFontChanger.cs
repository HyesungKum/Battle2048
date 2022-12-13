using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

namespace BeanTool
{
    #if UNITY_EDITOR
    public class SceneFontChanger : EditorWindow
    {
        static string Path = null;
        static string AssetPath = null;
        static bool FontProcessing = false;

        private void CreateGUI()
        {
            Init();
        }
        private void OnGUI()
        {
            FontChanger();
        }

        [MenuItem("BeanTool/BeanFontChanger")]
        static void Init()//custon window setting
        {
            SceneFontChanger YBWindow = (SceneFontChanger)GetWindow<SceneFontChanger>();
            YBWindow.Show();

            YBWindow.titleContent.text = "BeanFont Changer";

            YBWindow.minSize = new Vector2(400f, 100f);
            YBWindow.maxSize = new Vector2(400f, 100f);
        }
        static void FontChanger()
        {
            GUILayout.Space(10f);

            if (GUILayout.Button("Choose Font Asset"))
            {
                Path = EditorUtility.OpenFilePanel("Choose Font Asset", "", "Asset");
                AssetPath = "Assets" + Path.Split("Assets")[1];
            }
            EditorGUILayout.LabelField(AssetPath);

            GUILayout.Space(10f);

            if (GUILayout.Button("Apply"))
            {
                Changing();
            }
            
        }
        static void Changing()
        {
            #region Exception
            if (string.IsNullOrEmpty(Path) && !FontProcessing)
            {
                Debug.Log($"fonts change error## {Path} wrong input");
                return;
            }

            if (!File.Exists(Path))
            {
                Debug.Log($"fonts change error## {Path} wrong Path");
                return;
            }
            #endregion

            FontProcessing = true;
            int count = 1;

            Debug.Log($"path : {Path}");

            GameObject[] gameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int i = 0; i < gameObjects.Length; i++)
            {
                GameObject eachObject = gameObjects[i];
                Debug.Log($"applied object name : {eachObject.name}");
                Component[] components = eachObject.GetComponentsInChildren<TextMeshProUGUI>();
                Debug.Log(components.Length);
                foreach (TextMeshProUGUI text in components)
                {
                    Debug.Log($"applied object name : {eachObject.name} - {text.name}: apply");
                    text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
                    count++;
                }
            }

            Debug.Log($"Font Change Done =========================== {count} Fonts Changing in this Scene");
            Path = null;
            FontProcessing = false;
        }
    }
    #endif
}
