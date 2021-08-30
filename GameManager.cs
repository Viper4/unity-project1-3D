using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dictionary<string, Transform> importantTransforms = new Dictionary<string, Transform>();

    // Start is called before the first frame update
    void Awake()
    {
        UpdateImportantTransforms();
    }

    public void UpdateImportantTransforms()
    {
        ImportantTransform[] allTransforms = FindObjectsOfType<ImportantTransform>();
        foreach (ImportantTransform target in allTransforms)
        {
            AddOrReplace(target.name, target.transform);
        }

        //For disabled UI elements
        AddOrReplace("Settings", importantTransforms["UI"].Find("Settings"));
        AddOrReplace("Save Files", importantTransforms["UI"].Find("Save Files"));
        AddOrReplace("Levels", importantTransforms["UI"].Find("Levels"));
        AddOrReplace("Game Over", importantTransforms["UI"].Find("Game Over"));
        AddOrReplace("Win Screen", importantTransforms["UI"].Find("Win Screen"));
        AddOrReplace("Loading Screen", importantTransforms["UI"].Find("Loading Screen"));
        AddOrReplace("Text Prompt", importantTransforms["UI"].Find("Text Prompt"));
        AddOrReplace("Timer", importantTransforms["UI"].Find("Timer"));
        AddOrReplace("Enemies Left", importantTransforms["UI"].Find("Enemies Left"));

        AddOrReplace("Inventory List Content", importantTransforms["Inventory"].Find("ListViewport").Find("ListContent"));
        AddOrReplace("Settings List Content", importantTransforms["Settings"].Find("ListViewport").Find("ListContent"));
        AddOrReplace("Save Files List Content", importantTransforms["Save Files"].Find("ListViewport").Find("ListContent"));
        AddOrReplace("Levels List Content", importantTransforms["Levels"].Find("ListViewport").Find("ListContent"));
    }

    void AddOrReplace(string key, Transform value)
    {
        if (!importantTransforms.ContainsKey(key))
        {
            importantTransforms.Add(key, value);
        }
        else
        {
            importantTransforms[key] = value;
        }
    }
}
