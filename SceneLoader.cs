using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    Transform loadingScreen;

    public IEnumerator LoadScene(int sceneIndex, bool updateTransforms)
    {
        Debug.Log("LoadScene");
        SaveSystem.DeleteQuickSave();

        if (loadingScreen == null)
        {
            loadingScreen = GameObject.Find("UI").transform.Find("Loading Screen");
        }
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        loadingScreen.gameObject.SetActive(true);

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / .9f);

            loadingScreen.Find("Slider").GetComponent<Slider>().value = progress;
            loadingScreen.Find("Text").GetComponent<Text>().text = progress * 100 + "%";

            yield return null;
        }
        Debug.Log("Loaded Scene");

        yield return new WaitForSecondsRealtime(0.1f);

        GetComponent<SaveData>().Load(SaveSystem.currentSaveFileName, false, updateTransforms);
        
    }
}
