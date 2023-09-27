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
    //저장되는 json 파일 이름
    private string SceneNameDataFile = "SceneName.json";

    //json 파일로 저장되는 데이터
    private SceneDataList mSceneDataList;

    //현재 스크립트 경로
    private string mThisScriptsPath;

    //현재 스크립트에서 사용할 하위 오브젝트를 관리하는 같은 이름의 폴더 경로
    private string mThisScriptsFolderPath;
    private Vector2 scrollPosition = Vector2.zero;
    private Texture2D mLogoTexture;
    private Texture2D mCancelTexture;
    private GUIStyle customButtonStyle;

    [MenuItem("Tools/Pencils/SceneNavigator")]
    private static void OpenWindow()
    {
        SceneNavigator window = GetWindow<SceneNavigator>();
        window.titleContent = new GUIContent("SceneNavigator");
        window.Show();
    }

    private void OnEnable()
    {
        mSceneDataList = new SceneDataList();
        //현재 스크립트가 있는 경로
        mThisScriptsPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
        //현재 스크립트의 하위 폴더 경로
        mThisScriptsFolderPath = Path.Combine(mThisScriptsPath, GetType().Name);
        //json 파일 찾기
        string jsonFilePath = Path.Combine(mThisScriptsFolderPath, SceneNameDataFile);

        //이미지 파일 불러오기
        string logoPath = $"{mThisScriptsFolderPath}/Images/Logo.psd";
        string cancelPath = $"{mThisScriptsFolderPath}/Images/Cancel.png";
        mLogoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(logoPath);
        mCancelTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(cancelPath);

        //저장된 json파일이 있다면 그 파일에서 정보 불러오고 없다면 새로운 json파일 저장하기
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

        //로고 추가하기
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
        GUILayout.Label("Ver 0.0.2");
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

    //현재 스크립트가 있는 위치에 폴더 생성 후 json파일로 SceneName 관리
    private void SaveJsonData()
    {
        if (!Directory.Exists(mThisScriptsFolderPath))
        {
            Directory.CreateDirectory(mThisScriptsFolderPath);
            Debug.Log($"<color=green>Create {GetType().Name}Folder</color>");
        }
        else
        {
            Debug.Log($"<color=green>{GetType().Name} Folder already created!</color>");
        }

        File.WriteAllText(Path.Combine(mThisScriptsFolderPath, SceneNameDataFile), JsonConvert.SerializeObject(mSceneDataList));
    }

    //json파일에서 저장된 SceneName 불러오기
    private void LoadJsonData()
    {
        mThisScriptsFolderPath = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this))), GetType().Name);

        string str = File.ReadAllText(Path.Combine(mThisScriptsFolderPath, SceneNameDataFile));
        mSceneDataList = JsonConvert.DeserializeObject<SceneDataList>(str);
    }


    //버튼 클릭 시 버튼에 현재 씬 이름 추가
    private void AddCurrentSceneButton()
    {
        LoadJsonData();

        var currentScene = EditorSceneManager.GetActiveScene();

        if (mSceneDataList.SceneNameDataList.Count > 0)
        {
            for (int i = 0; i < mSceneDataList.SceneNameDataList.Count; ++i)
            {
                if (mSceneDataList.SceneNameDataList[i].SceneName.Contains(currentScene.name))
                {
                    Debug.Log("<color=green>Scene currently added!</color>");
                    return;
                }

                SceneData currentSceneData = new SceneData();
                currentSceneData.SceneName = currentScene.name;
                currentSceneData.SceneNamePath = SearchScene(currentScene.name);

                //Scene 경로가 있을 때만 현재 데이터를 추가
                if (currentSceneData.SceneNamePath != null)
                    mSceneDataList.SceneNameDataList.Add(currentSceneData);
            }
        }
        else
        {
            SceneData currentSceneData = new SceneData();
            currentSceneData.SceneName = currentScene.name;
            currentSceneData.SceneNamePath = SearchScene(currentScene.name);

            //Scene 경로가 있을 때만 현재 데이터를 추가
            if (currentSceneData.SceneNamePath != null)
                mSceneDataList.SceneNameDataList.Add(currentSceneData);
        }

        SaveJsonData();
    }

    private void AddSelectSceneButton()
    {
        LoadJsonData();

        //프로젝트 패널에서 선택하고 있는 씬 관리
        UnityEngine.Object[] selectedScene = Selection.objects;
        Debug.Log($"<color=green>Selected Scene : {selectedScene}</color>");

        //비교를 위해 씬의 이름을 저장하는 딕셔너리 추가
        Dictionary<string, object> sceneCompareList = new Dictionary<string, object>();

        //선택한 씬이름을 딕셔너리에 저장
        foreach (SceneAsset scene in selectedScene)
            sceneCompareList.Add(scene.name, null);

        // 원래 있었던 리스트 개수만큼 X 선택한 씬의 개수만큼 반복 비교
        for (int i = 0; i < mSceneDataList.SceneNameDataList.Count; ++i)
        {
            for (int j = 0; j < selectedScene.Length; ++j)
            {
                if (mSceneDataList.SceneNameDataList[i].SceneName.Equals(selectedScene[j]) && selectedScene[j].GetType() == typeof(SceneAsset))
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

    //Scene data 삭제
    private void DeleteButton(int id)
    {
        mSceneDataList.SceneNameDataList.RemoveAt(id);

        SaveJsonData();
    }

    //SceneList의 Index 기반으로 해당 Scene 열기
    private void OpenScene(int index)
    {
        if (EditorSceneManager.GetActiveScene().name == mSceneDataList.SceneNameDataList[index].SceneName)
        {
            Debug.Log("<color=green>Same as the currently open scene!</color>");
        }
        else
        {
            //현재 씬 저장하기
            SaveCurrentScene();

            //해당 위치에 Scene파일이 있다면 열고 없으면 디버그 출력 후 버튼 삭제
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

    //string 기반으로 Assets 폴더에서 해당 이름을 가진 Scene의 경로 반환
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

    //현재 Scene의 정보 저장
    private void SaveCurrentScene()
    {
        Debug.Log($"<color=green>Saved Current Scene! : {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}</color>");
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
#endif