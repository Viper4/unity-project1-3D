using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";
    private static readonly string QUICKSAVE_FOLDER = Application.dataPath + "/QuickSaves/";
    private static readonly string SETTINGS_FOLDER = Application.dataPath + "/Settings/";

    public static string currentSaveFileName;

    public static void Init()
    {
        if(!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
        if (!Directory.Exists(QUICKSAVE_FOLDER))
        {
            Directory.CreateDirectory(QUICKSAVE_FOLDER);
        }
        if(!Directory.Exists(SETTINGS_FOLDER))
        {
            Directory.CreateDirectory(SETTINGS_FOLDER);
        }
    }

    public static void Save(string json, string fileName, bool overWrite)
    {
        if(fileName == "MostRecent")
        {
            FileInfo mostRecentFile = GetFile("MostRecent");

            if (mostRecentFile != null)
            {
                File.WriteAllText(mostRecentFile.FullName, json);
            }
            else
            {
                File.WriteAllText(SAVE_FOLDER + "save0.json", json);
            }
        }
        else
        {
            if(overWrite)
            {
                if (File.Exists(SAVE_FOLDER + fileName + ".json"))
                {
                    File.WriteAllText(SAVE_FOLDER + fileName + ".json", json);
                }
                else
                {
                    Debug.LogError("Could not overwrite save since file does not exist: '" + SAVE_FOLDER + fileName + ".json'");
                }
            }
            else
            {
                File.WriteAllText(SAVE_FOLDER + fileName + SaveNumber(fileName) + ".json", json);
            }
        }
    }

    public static void QuickSave(string json)
    {
        File.WriteAllText(QUICKSAVE_FOLDER + "quick_save.json", json);
    }

    public static void SaveSettings(string json, string fileName)
    {
        File.WriteAllText(SETTINGS_FOLDER + fileName + ".json", json);
    }

    public static string Load(string fileName)
    {
        FileInfo fileInfo = GetFile(fileName);
        if(fileInfo != null)
        {
            currentSaveFileName = Regex.Replace(fileInfo.Name, ".json", "");
            try
            {
                string json = File.ReadAllText(fileInfo.FullName);
                return json;
            }
            catch
            {
                Debug.LogWarning("Could not load '" + fileName + ".json" + "' since it does not exist");

                return null;
            }
        }
        else
        {
            return null;
        }
    }

    public static string QuickLoad()
    {
        if(File.Exists(QUICKSAVE_FOLDER + "quick_save.json"))
        {
            string json = File.ReadAllText(QUICKSAVE_FOLDER + "quick_save.json");
            return json;
        }
        else
        {
            Debug.Log("No quicksave found to load from");
            return null;
        }
    }

    public static string LoadSettings(string fileName)
    {
        if(File.Exists(SETTINGS_FOLDER + fileName + ".json"))
        {
            string json = File.ReadAllText(SETTINGS_FOLDER + fileName + ".json");
            return json;
        }
        else
        {
            try
            {
                string json = File.ReadAllText(SETTINGS_FOLDER + "default_settings.json");
                return json;
            }
            catch
            {
                Debug.LogWarning("Could not load settings since there are no files in " + SETTINGS_FOLDER);
                return null;
            }
        }
    }

    public static void DeleteSave(string fileName)
    {
        File.Delete(SAVE_FOLDER + fileName + ".json");
        File.Delete(SAVE_FOLDER + fileName + ".json.meta");
    }

    public static void DeleteQuickSave()
    {
        File.Delete(QUICKSAVE_FOLDER + "quick_save.json");
        File.Delete(QUICKSAVE_FOLDER + "quick_save.json.meta");
    }

    public static int SaveNumber(string fileName)
    {
        int saveNumber = 0;

        while(File.Exists(SAVE_FOLDER + fileName + saveNumber + ".json"))
        {
            saveNumber++;
        }

        return saveNumber;
    }

    public static FileInfo GetFile(string fileName)
    {
        if(fileName == "MostRecent")
        {
            FileInfo[] saveFiles = GetAllSaveFiles();
            FileInfo mostRecentFile = null;

            foreach (FileInfo fileInfo in saveFiles)
            {
                if (mostRecentFile == null)
                {
                    mostRecentFile = fileInfo;
                }
                else if (fileInfo.LastWriteTime > mostRecentFile.LastWriteTime)
                {
                    mostRecentFile = fileInfo;
                }
            }

            if (mostRecentFile != null)
            {
                return mostRecentFile;
            }
            else
            {
                return null;
            }
        }
        else
        {
            FileInfo fileInfo = new FileInfo(SAVE_FOLDER + fileName + ".json");
            return fileInfo;
        }
    }

    public static FileInfo[] GetAllSaveFiles()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_FOLDER);
        return directoryInfo.GetFiles("*.json");
    }
}
