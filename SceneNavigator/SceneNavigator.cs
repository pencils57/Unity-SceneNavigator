/* 
Author: pencils (https://stroypencils.tistory.com)
License: None

Scene Navigator provides easy mobility by registering Scene that Unity uses frequently. 

**Requirements**: newtonsoft.json must be added to the project before it can be used. 
You can download it from the Nugget Package Manager in visual study or from the project. 
(https://github.com/GlitchEnzo/NuGetForUnity)

How to use it
1. In the Unity Editor, open the Scene Navigator window from the Tools menu.
2. You can add a scene move button in two ways. 
  - Add Current Scene : Adds the currently open Scene with a button.
  - Add Select Scene : button to add the selected Scene in the project panel. You can add it after selecting duplicate.
   The buttons are not added in duplicate.
3. Use the added buttons to easily move the Scene.  If you change the Scene, the previous Scene is saved automatically.
4. You can delete unused Scenes. 
*/

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Linq;

[Serializable]
public class SceneData
{
    public string SceneName;
    public string SceneNamePath;
}

[Serializable]
public class SceneDataList
{
    public List<SceneData> SceneNameDataList;

    public SceneDataList()
    {
        SceneNameDataList = new List<SceneData>();
    }
}

public class SceneNavigator : EditorWindow
{
    //Name of the json file to be saved
    private string SceneNameDataFile = "SceneName.json";

    //Data stored as a json file
    private SceneDataList mSceneDataList;

    //Current Script Path
    private string mThisScriptsPath;
    private Vector2 scrollPosition = Vector2.zero;
    private Texture2D mLogoTexture;
    private Texture2D mCancelTexture;
    private GUIStyle customButtonStyle;

    [MenuItem("Tools/Pencils/SceneNavigator")]
    public static void ShowWindow()
    {
        GetWindow<ItemNavigator>("SceneNavigator");
    }

    private void OnEnable()
    {
        mSceneDataList = new SceneDataList();
        //Path with current script
        mThisScriptsPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
        //Find json file
        string jsonFilePath = Path.Combine(mThisScriptsPath, SceneNameDataFile);

        string logoPath = $"{mThisScriptsPath}/Images/Logo.psd";
        string cancelPath = $"{mThisScriptsPath}/Images/Cancel.png";
        mLogoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(logoPath);
        mCancelTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(cancelPath);

        //Recall information from a saved json file if it exists and save a new json file if not
        if (File.Exists(jsonFilePath))
        {
            Debug.Log("Save file exists!");
            LoadJsonData();
        }
        else
        {
            Debug.Log("No save file found!");
            SaveJsonData();
        }
    }


    private void OnGUI()
    {
        customButtonStyle = new GUIStyle(GUI.skin.button);
        customButtonStyle.hover.textColor = Color.green;

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        if (mLogoTexture != null)
        {
            GUILayout.Space(20);

            float inspectorWidth = EditorGUIUtility.currentViewWidth;
            float width = inspectorWidth - 20;
            float height = mLogoTexture.height * (width / mLogoTexture.width);

            Rect rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            if (GUI.Button(rect, mLogoTexture, GUIStyle.none))
            {
                Application.OpenURL("https://github.com/pencils57");
            }

            GUILayout.Space(20);
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("by Pencils");
        GUILayout.FlexibleSpace(); 
        GUILayout.Label("Ver 0.0.4");
        GUILayout.EndHorizontal();
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Current Scene", customButtonStyle, GUILayout.Height(35)))
        {
            AddCurrentSceneButton();
        }

        if (GUILayout.Button("Add Select Scene", customButtonStyle, GUILayout.Height(35)))
        {
            AddSelectSceneButton();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.Label("Move to Scene Name");
        if (mSceneDataList.SceneNameDataList != null)
        {
            for (int i = 0; i < mSceneDataList.SceneNameDataList.Count; ++i)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(mSceneDataList.SceneNameDataList[i].SceneName, customButtonStyle, GUILayout.Height(25)))
                {
                    OpenScene(i);
                }

                if (GUILayout.Button(mCancelTexture, GUILayout.Width(25), GUILayout.Height(25)))
                {
                    DeleteButton(i);
                }
                GUILayout.EndHorizontal();
            }
        }
        else
        {
            Debug.Log("<color=green>NO SceneDataList!</color>");
        }
        GUILayout.EndScrollView();
    }

    //Create a folder where the current script is located and manage SceneName with a json file
    private void SaveJsonData()
    {
        File.WriteAllText(Path.Combine(mThisScriptsPath, SceneNameDataFile), JsonConvert.SerializeObject(mSceneDataList));
    }

    //Importing saved SceneName from json file
    private void LoadJsonData()
    {
        string str = File.ReadAllText(Path.Combine(mThisScriptsPath, SceneNameDataFile));
        mSceneDataList = JsonConvert.DeserializeObject<SceneDataList>(str);
    }


    //Add the current scene name to the button when you click the button
    private void AddCurrentSceneButton()
    {
        LoadJsonData();

        var currentScene = EditorSceneManager.GetActiveScene();

        if (mSceneDataList.SceneNameDataList.Count > 0)
        {
            bool isNoScene = true;
            for (int i = 0; i < mSceneDataList.SceneNameDataList.Count; ++i)
            {
                if (mSceneDataList.SceneNameDataList[i].SceneName.Contains(currentScene.name))
                {
                    Debug.Log("<color=green>Scene currently added!</color>");
                    isNoScene = false;
                }
            }

            if(isNoScene)
            {
                SceneData currentSceneData = new SceneData();
                currentSceneData.SceneName = currentScene.name;
                currentSceneData.SceneNamePath = SearchScene(currentScene.name);

                //Add current data only when Scene path exists
                if (currentSceneData.SceneNamePath != null)
                    mSceneDataList.SceneNameDataList.Add(currentSceneData);

            }
        }
        else
        {
            SceneData currentSceneData = new SceneData();
            currentSceneData.SceneName = currentScene.name;
            currentSceneData.SceneNamePath = SearchScene(currentScene.name);

            //Add current data only when Scene path exists
            if (currentSceneData.SceneNamePath != null)
                mSceneDataList.SceneNameDataList.Add(currentSceneData);
        }
        SaveJsonData();
    }

    private void AddSelectSceneButton()
    {
        LoadJsonData();

        //Manage the scene you are selecting in the project panel
        UnityEngine.Object[] selectedScene = Selection.objects;
        for(int i = 0; i < selectedScene.Length; ++i)
        {
        Debug.Log($"<color=green>Selected Scene : {selectedScene[i]}</color>");
        }

        //Add a dictionary that saves the name of the scene for comparison
        Dictionary<string, object> sceneCompareList = new Dictionary<string, object>();

        //Save the selected scene name to the dictionary
        foreach (SceneAsset scene in selectedScene)
            sceneCompareList.Add(scene.name, null);

        // Repeat comparison by the number of scenes selected * as many times as the number of scenes that were originally present
        for (int i = 0; i < mSceneDataList.SceneNameDataList.Count; ++i)
        {
            for (int j = 0; j < selectedScene.Length; ++j)
            {
                if (mSceneDataList.SceneNameDataList[i].SceneName == selectedScene[j].name && selectedScene[j].GetType() == typeof(SceneAsset))
                {
                    sceneCompareList.Remove(mSceneDataList.SceneNameDataList[i].SceneName);
                    Debug.Log($"<color=green>{mSceneDataList.SceneNameDataList[i].SceneName} Scene is already saved!</color>");
                }
            }
        }

        foreach (string str in sceneCompareList.Keys)
        {
            SceneData selectSceneData = new SceneData();
            selectSceneData.SceneName = str;
            selectSceneData.SceneNamePath = SearchScene(str);

            if (selectSceneData.SceneNamePath != null)
                mSceneDataList.SceneNameDataList.Add(selectSceneData);
        }

        SaveJsonData();
    }

    //Delete Scene data
    private void DeleteButton(int id)
    {
        mSceneDataList.SceneNameDataList.RemoveAt(id);

        SaveJsonData();
    }

    //Open corresponding Scene based on Index in SceneList
    private void OpenScene(int index)
    {
        if (EditorSceneManager.GetActiveScene().name == mSceneDataList.SceneNameDataList[index].SceneName)
        {
            Debug.Log("<color=green>Same as the currently open scene!</color>");
        }
        else
        {
            //Save Current Scene
            SaveCurrentScene();

            //Open if Scene file is in that location and delete button after debug output if not
            if (File.Exists(mSceneDataList.SceneNameDataList[index].SceneNamePath))
            {
                EditorSceneManager.OpenScene(mSceneDataList.SceneNameDataList[index].SceneNamePath);
            }
            else
            {
                Debug.Log($"<color=green>{mSceneDataList.SceneNameDataList[index].SceneName} Scene has been repositioned! Please register again</color>");
                DeleteButton(index);
            }
        }

        SaveJsonData();
    }

    //Returns the path of the Scene with its name in the Assets folder based on string
    private string SearchScene(string sceneName)
    {
        string[] scenePathArr = Directory.GetFiles("Assets", sceneName + ".unity", SearchOption.AllDirectories);

        if (scenePathArr.Length == 1)
        {
            Debug.Log($"<color=green>Add Scene : {sceneName}</color>");
            return scenePathArr[0];
        }
        else
        {
            Debug.Log($"<color=green>No scene with {sceneName} name exists or more than 2 scenes!</color>");
            return null;
        }
    }

    //Save information from the current Scene
    private void SaveCurrentScene()
    {
        Debug.Log($"<color=green>Saved Current Scene! : {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}</color>");
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
#endif
