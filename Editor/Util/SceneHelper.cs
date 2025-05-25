using UnityEditor;
using UnityEditor.SceneManagement;

namespace LF.Editor
{
    static class SceneHelper
    {
        static string sceneToOpen;

        public static void StartScene(string scenePath)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            sceneToOpen = scenePath;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            if (sceneToOpen == null ||
                EditorApplication.isPlaying || EditorApplication.isPaused ||
                EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            EditorApplication.update -= OnUpdate;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(sceneToOpen);
                EditorApplication.isPlaying = true;
            }

            sceneToOpen = null;
        }
    }
}