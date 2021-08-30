using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    Transform loadingScreen;

    public IEnumerator LoadScene(int sceneIndex, bool updateTransforms, bool createSave)
    {
        if (loadingScreen == null)
        {
            loadingScreen = GameObject.Find("UI").transform.Find("Loading Screen");
        }
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        loadingScreen.gameObject.SetActive(true);

        //Changed !asyncLoad.isDone to progress because the former doesn't let code after the loop run
        float progress = 0;
        while (progress < 1)
        {
            progress = Mathf.Clamp01(asyncLoad.progress / .9f);

            loadingScreen.Find("Slider").GetComponent<Slider>().value = progress;
            loadingScreen.Find("Text").GetComponent<Text>().text = progress * 100 + "%";

            yield return null;
        }
        StartCoroutine(AfterLoad(updateTransforms, createSave));
    }

    //Split the coroutine because Unity kills all coroutines on scene load and there's nothing I can do about that
    IEnumerator AfterLoad(bool updateTransforms, bool createSave)
    {
        yield return new WaitForSecondsRealtime(0.2f);

        if (createSave)
        {
            GetComponent<SaveData>().Save("save", false);
        }
        else
        {
            GetComponent<SaveData>().Load(SaveSystem.currentSaveFileName, false, updateTransforms);
        }
    }
}
