using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LF.Editor
{
    public class DefaultIconWindow : EditorWindow
    {
        [MenuItem("Window/Unity内置图标")]
        static void Init()
        {
            EditorWindow window = GetWindow<DefaultIconWindow>();
            window.Show();
        }

        List<Texture2D> builtInTexs = new ();
        private string searchContent = "";
        private const float width = 50f;
        Vector2 scrollPos = Vector2.zero;

        void GetBultinAsset()
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var info = typeof(EditorGUIUtility).GetMethod("GetEditorAssetBundle", flags);
            var bundle = info.Invoke(null, new object[0]) as AssetBundle;
            Object[] objs = bundle.LoadAllAssets<Texture2D>();
            if (null != objs)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    builtInTexs.Add(objs[i] as Texture2D);
                }
            }
        }

        void OnEnable()
        {
            GetBultinAsset();
        }
        
        void OnGUI()
        {
            GUILayout.BeginHorizontal("Toolbar");
            {
                GUILayout.Label("Search:", GUILayout.Width(50));
                searchContent = GUILayout.TextField(searchContent, "SearchTextField");
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            List<string> matchNames = new List<string>();
            for (int i = 0; i < builtInTexs.Count; i++)
            {
                if (!builtInTexs[i].name.Equals(string.Empty) && builtInTexs[i].name.ToLower().Contains(searchContent.ToLower()))
                {
                    matchNames.Add(builtInTexs[i].name);
                }
            }
            int count = Mathf.RoundToInt(position.width / (width + 3f));
            for (int i = 0; i < matchNames.Count; i += count)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < count; j++)
                {
                    int index = i + j;
                    if (index < matchNames.Count)
                    {
                        if (GUILayout.Button(EditorGUIUtility.IconContent(matchNames[index]), GUILayout.Width(width), GUILayout.Height(30)))
                        {
                            EditorGUIUtility.systemCopyBuffer = matchNames[index];
                            Debug.Log(matchNames[index]);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}