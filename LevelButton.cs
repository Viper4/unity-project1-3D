using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public Transform gameManager;

    // Start is called before the first frame update
    void Awake()
    {
        if (gameManager == null)
        {
            gameManager = GameObject.Find("GameManager").transform;
        }

        transform.GetComponent<Button>().onClick.AddListener(LoadLevel);
    }

    void LoadLevel()
    {
        int sceneIndex = SceneManager.GetSceneByName(transform.name).buildIndex;
        StartCoroutine(gameManager.GetComponent<SceneLoader>().LoadScene(sceneIndex, true, false));
    }
}
