using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CityWalk.EditorTools
{
    public static class CityWalkMenu
    {
        const string ScenePath = "Assets/CityWalk/Scenes/CityWalk.unity";

        [MenuItem("City Walk/Create Starter Scene")]
        static void CreateStarterScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("CityWalk Bootstrap").AddComponent<CityWalkBootstrap>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("City Walk: scene created at " + ScenePath + " and added to Build Settings. Press Play.");
        }

    }
}
