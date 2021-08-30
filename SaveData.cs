using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveData : MonoBehaviour
{
    public static SaveData instance;

    GameManager gameManager;

    Timer timer;
    StatSystem stats;
    Player playerScript;

    private void Awake()
    {
        SaveSystem.DeleteQuickSave();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        timer = gameManager.GetComponent<Timer>();
        stats = gameManager.importantTransforms["Player"].GetComponent<StatSystem>();
        playerScript = gameManager.importantTransforms["Player"].GetComponent<Player>();

        if (instance != null)
        {
            Destroy(gameObject);
        }
        else if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            SaveSystem.Init();

            Load("MostRecent", false, false);
            LoadSettings("settings");
            UpdateSaveSlots();
            UpdateLevelButtons();
        }
        else
        {

        }
    }

    public void Save(string fileName, bool overWrite)
    {
        gameManager.UpdateImportantTransforms();

        SaveObject saveObject = new SaveObject
        {
            sceneName = SceneManager.GetActiveScene().name,
            sceneIndex = SceneManager.GetActiveScene().buildIndex,
            highScores = stats.highScores,
            inventory = stats.inventory,
            selectedItem = gameManager.importantTransforms["Item Slot"].childCount > 0 ? gameManager.importantTransforms["Item Slot"].GetChild(0).name : null,
            playerInfo = TransformToInfo(gameManager.importantTransforms["Player"]),
            enemiesInfo = TransformsToInfo(gameManager.importantTransforms["Enemies"]),
            interactablesInfo = TransformsToInfo(gameManager.importantTransforms["Interactables"]),
            minutesRemaining = timer.minutesRemaining,
            secondsRemaining = timer.secondsRemaining
        };

        string json = JsonConvert.SerializeObject(saveObject, Formatting.Indented);

        SaveSystem.Save(json, fileName, overWrite);

        UpdateSaveSlots();
        UpdateLevelButtons();
    }

    public void QuickSave()
    {
        gameManager.UpdateImportantTransforms();

        SaveObject saveObject = new SaveObject
        {
            playerInfo = TransformToInfo(gameManager.importantTransforms["Player"]),
            enemiesInfo = TransformsToInfo(gameManager.importantTransforms["Enemies"]),
            interactablesInfo = TransformsToInfo(gameManager.importantTransforms["Interactables"]),
            minutesRemaining = timer.minutesRemaining,
            secondsRemaining = timer.secondsRemaining
        };
        Debug.Log(timer.minutesRemaining + ":" + timer.secondsRemaining);

        string json = JsonConvert.SerializeObject(saveObject, Formatting.Indented);

        SaveSystem.QuickSave(json);
    }

    public void SaveSettings(string fileName)
    {
        Settings settings = new Settings
        {
            mouseSensitivity = gameManager.importantTransforms["Player"].GetComponent<Player>().mouseSensitivity,
            fov = gameManager.importantTransforms["Main Camera"].GetComponent<Camera>().fieldOfView,
            keys = gameManager.importantTransforms["Player"].GetComponent<Player>().keys
        };

        string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

        SaveSystem.SaveSettings(json, fileName);
    }

    public void Load(string fileName, bool newScene, bool updateTransforms)
    {
        string json = SaveSystem.Load(fileName);

        if(json != null)
        {
            SaveObject saveObject = JsonConvert.DeserializeObject<SaveObject>(json);

            if (newScene)
            {
                StartCoroutine(GetComponent<SceneLoader>().LoadScene(saveObject.sceneIndex, updateTransforms, false));
            }
            else
            {
                if (updateTransforms)
                {
                    gameManager.UpdateImportantTransforms();
                    //Updating player
                    stats.health = saveObject.playerInfo.health;
                    stats.stamina = saveObject.playerInfo.stamina;
                    gameManager.importantTransforms["Player"].position = new Vector3(saveObject.playerInfo.position[0], saveObject.playerInfo.position[1], saveObject.playerInfo.position[2]);
                    gameManager.importantTransforms["Player"].rotation = new Quaternion(saveObject.playerInfo.rotation[0], saveObject.playerInfo.rotation[1], saveObject.playerInfo.rotation[2], saveObject.playerInfo.rotation[3]);

                    //Updating enemies
                    if (saveObject.enemiesInfo != null)
                        UpdateTransforms(saveObject.enemiesInfo, gameManager.importantTransforms["Enemies"]);

                    //Updating interactables
                    if (saveObject.interactablesInfo != null)
                        UpdateTransforms(saveObject.interactablesInfo, gameManager.importantTransforms["Interactables"]);
                }

                timer.minutesRemaining = saveObject.minutesRemaining;
                timer.secondsRemaining = saveObject.secondsRemaining;

                foreach (string key in saveObject.highScores.Keys)
                {
                    if (!stats.highScores.ContainsKey(key))
                    {
                        stats.highScores.Add(key, saveObject.highScores[key]);
                    }
                    else
                    {
                        stats.highScores[key] = saveObject.highScores[key];
                    }
                }

                stats.inventory = saveObject.inventory;
                for (int i = 0; i < stats.inventory.Count; i++)
                {
                    GameObject itemButton = Instantiate((GameObject)Resources.Load("Prefabs/ItemButton", typeof(GameObject)), gameManager.importantTransforms["Inventory List Content"], false);
                    itemButton.name = stats.inventory[i];
                    itemButton.transform.Find("Text").GetComponent<Text>().text = stats.inventory[i];
                }

                gameManager.importantTransforms["Item Slot"].GetComponent<ItemSystem>().GiveItem(saveObject.selectedItem);
            }
            UpdateSaveSlots();
            UpdateLevelButtons();
        }
    }

    public void QuickLoad()
    {
        string json = SaveSystem.QuickLoad();

        if(json != null)
        {
            SaveObject saveObject = JsonConvert.DeserializeObject<SaveObject>(json);

            //Updating player
            stats.health = saveObject.playerInfo.health;
            stats.stamina = saveObject.playerInfo.stamina;
            gameManager.importantTransforms["Player"].position = new Vector3(saveObject.playerInfo.position[0], saveObject.playerInfo.position[1], saveObject.playerInfo.position[2]);
            gameManager.importantTransforms["Player"].rotation = new Quaternion(saveObject.playerInfo.rotation[0], saveObject.playerInfo.rotation[1], saveObject.playerInfo.rotation[2], saveObject.playerInfo.rotation[3]);

            //Updating enemies
            if (saveObject.enemiesInfo != null)
                UpdateTransforms(saveObject.enemiesInfo, GameObject.Find("Enemies").transform);

            //Updating interactables
            if (saveObject.interactablesInfo != null)
                UpdateTransforms(saveObject.interactablesInfo, GameObject.Find("Interactables").transform);

            timer.minutesRemaining = saveObject.minutesRemaining;
            timer.secondsRemaining = saveObject.secondsRemaining;
        }
    }

    public void LoadSettings(string fileName)
    {
        string json = SaveSystem.LoadSettings(fileName);

        if(json != null)
        {
            Settings settings = JsonConvert.DeserializeObject<Settings>(json);

            playerScript.keys = settings.keys;
            playerScript.mouseSensitivity = settings.mouseSensitivity;
            gameManager.importantTransforms["Main Camera"].GetComponent<Camera>().fieldOfView = settings.fov;

            foreach (Transform setting in gameManager.importantTransforms["Settings List Content"])
            {
                switch (setting.tag)
                {
                    case "Keybind":
                        Transform keybind = setting.GetChild(0);
                        string text = settings.keys[keybind.name].ToString();
                        keybind.GetComponent<KeybindButton>().ChangeText(text);
                        break;
                    case "Untagged":
                        switch (setting.name)
                        {
                            case "Sensitivity":
                                setting.GetComponent<Slider>().value = settings.mouseSensitivity;
                                setting.Find("Text").GetComponent<Text>().text = "Sensitivity " + settings.mouseSensitivity;
                                break;
                            case "FOV":
                                setting.GetComponent<Slider>().value = settings.fov;
                                setting.Find("Text").GetComponent<Text>().text = "FOV " + settings.fov;
                                break;
                        }
                        break;
                }
            }
        }
    }

    public void DeleteSave(string fileName)
    {
        Destroy(gameManager.importantTransforms["Save Files List Content"].Find(fileName).gameObject);

        SaveSystem.DeleteSave(fileName);

        UpdateSaveSlots();
    }

    void UpdateSaveSlots()
    {
        foreach(Transform child in gameManager.importantTransforms["Save Files List Content"].transform)
        {
            Destroy(child.gameObject);
        }

        FileInfo[] saveFiles = SaveSystem.GetAllSaveFiles();
        foreach(FileInfo save in saveFiles)
        {
            string fileName = Regex.Replace(save.Name, ".json", "");
            string json = SaveSystem.Load(fileName);

            SaveObject saveObject = JsonConvert.DeserializeObject<SaveObject>(json);

            GameObject saveSlot = Instantiate((GameObject)Resources.Load("Prefabs/SaveSlot", typeof(GameObject)), gameManager.importantTransforms["Save Files List Content"], false);

            saveSlot.name = fileName;

            int enemiesLeft = 0;
            foreach(TransformInfo character in saveObject.enemiesInfo)
            {
                if(character.health > 0)
                {
                    enemiesLeft++;
                }
            }

            saveSlot.transform.Find("Text").GetComponent<Text>().text = fileName;
            saveSlot.transform.Find("Information").GetComponent<Text>().text = "Level: " + saveObject.sceneName + 
                "\nTime Remaining: " + string.Format("{0:00}:{1:00}", saveObject.minutesRemaining, saveObject.secondsRemaining) + 
                "\nEnemies Remaining: " + enemiesLeft + 
                "\nHealth: " + saveObject.playerInfo.health + 
                "\nInventory: " + string.Join(",", saveObject.inventory) + 
                "\nLevels Won: " + saveObject.highScores.Keys.Count + 
                "\nLast Played: " + SaveSystem.GetFile(fileName).LastWriteTime;
        }
    }

    void UpdateLevelButtons()
    {
        foreach (Transform child in gameManager.importantTransforms["Levels List Content"])
        {
            Destroy(child.gameObject);
        }

        foreach(string key in stats.highScores.Keys)
        {
            GameObject levelButton = Instantiate((GameObject)Resources.Load("Prefabs/LevelButton", typeof(GameObject)), gameManager.importantTransforms["Levels List Content"], false);
            levelButton.name = key;
            levelButton.transform.Find("Text").GetComponent<Text>().text = key;
        }
    }

    void UpdateTransforms(List<TransformInfo> transformInfoList, Transform parent)
    {
        List<Transform> transformsUpdated = new List<Transform>();

        for (int i = 0; i < transformInfoList.Count; i++)
        {
            Vector3 position = new Vector3(transformInfoList[i].position[0], transformInfoList[i].position[1], transformInfoList[i].position[2]);
            Quaternion rotation = new Quaternion(transformInfoList[i].rotation[0], transformInfoList[i].rotation[1], transformInfoList[i].rotation[2], transformInfoList[i].rotation[3]);

            bool hasStats = transformInfoList[i].health != -80085;

            try
            {
                Transform currentTransform = null;
                foreach (Transform potentialTransform in parent)
                {
                    if (!transformsUpdated.Contains(potentialTransform) && potentialTransform.name == transformInfoList[i].name)
                    {
                        currentTransform = potentialTransform;
                        break;
                    }
                }

                currentTransform.position = position;
                currentTransform.rotation = rotation;
                if (hasStats)
                {
                    currentTransform.GetComponent<StatSystem>().health = transformInfoList[i].health;
                    currentTransform.GetComponent<StatSystem>().stamina = transformInfoList[i].stamina;
                }

                transformsUpdated.Add(currentTransform);
            }
            catch
            {
                GameObject prefab = (GameObject)Resources.Load("Prefabs/" + transformInfoList[i].name, typeof(GameObject));

                GameObject go = Instantiate(prefab, position, rotation, parent);
                if (hasStats)
                {
                    go.GetComponent<StatSystem>().health = transformInfoList[i].health;
                    go.GetComponent<StatSystem>().stamina = transformInfoList[i].stamina;
                }
                go.name = transformInfoList[i].name;
            }
        }
    }

    private class SaveObject
    {
        public string sceneName;
        public int sceneIndex;
        public Dictionary<string, float> highScores;
        public List<string> inventory;
        public string selectedItem;
        public TransformInfo playerInfo;
        public List<TransformInfo> enemiesInfo;
        public List<TransformInfo> interactablesInfo;
        public float minutesRemaining;
        public float secondsRemaining;
    }

    private class Settings
    {
        public float mouseSensitivity;
        public float fov;
        public Dictionary<string, KeyCode> keys;
    }

    public class TransformInfo
    {
        public string name;
        public float[] position;
        public float[] rotation;
        public float health;
        public float stamina;
    }

    private TransformInfo TransformToInfo(Transform target)
    {
        TransformInfo transformInfo = new TransformInfo
        {
            name = target.name,
            position = Vector3ToFloat(target.position),
            rotation = QuaternionToFloat(target.rotation),
            health = target.GetComponent<StatSystem>() == null ? -80085 : target.GetComponent<StatSystem>().health,
            stamina = target.GetComponent<StatSystem>() == null ? -80085 : target.GetComponent<StatSystem>().stamina
        };

        return transformInfo;
    }

    private List<TransformInfo> TransformsToInfo(Transform parent)
    {
        List<TransformInfo> transformInfoList = new List<TransformInfo>();
        foreach (Transform child in parent.transform)
        {
            transformInfoList.Add(TransformToInfo(child));
        }

        return transformInfoList;
    }

    private float[] Vector3ToFloat(Vector3 vector)
    {
        float[] floatArray = { 0, 0, 0 };
        floatArray[0] = vector.x;
        floatArray[1] = vector.y;
        floatArray[2] = vector.z;

        return floatArray;
    }

    private float[] QuaternionToFloat(Quaternion quaternion)
    {
        float[] floatArray = { 0, 0, 0, 0 };
        floatArray[0] = quaternion.x;
        floatArray[1] = quaternion.y;
        floatArray[2] = quaternion.z;
        floatArray[3] = quaternion.w;

        return floatArray;
    }
}
