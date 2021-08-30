using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System;

public class UISystem : MonoBehaviour
{
    GameManager gameManager;
    GameObject sceneLoader;

    StatSystem stats;
    SaveData saveData;

    public GameObject currentKey { get; set; }
    public GameObject currentSaveSlot { get; set; }

    public bool inMainMenuScene;

    Scene scene;

    // Start is called before the first frame update
    void Awake()
    {
        if(gameManager == null)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        if(sceneLoader == null)
        {
            sceneLoader = GameObject.Find("SceneLoader");
        }

        scene = SceneManager.GetActiveScene();
        stats = gameManager.importantTransforms["Player"].GetComponent<StatSystem>();
        saveData = sceneLoader.GetComponent<SaveData>();

        currentKey = null;
        currentSaveSlot = null;

        gameManager.importantTransforms["Main Menu"].Find("Text").GetComponent<Text>().text = Regex.Replace(scene.name, "[0-9]", "") + "\nLevel " + Regex.Replace(scene.name, "[A-z]", "");

        GameTime(0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (stats.stamina <= 0)
        {
            stats.stamina = 0;
        }
        else if (stats.stamina > stats.maxStamina)
        {
            stats.stamina = stats.maxStamina;
        }
        //Changing x scale of stamina bar according to player stamina
        gameManager.importantTransforms["Stamina Bar"].Find("Blue").localScale = new Vector3(stats.stamina / stats.maxStamina, gameManager.importantTransforms["Stamina Bar"].localScale.y, gameManager.importantTransforms["Stamina Bar"].localScale.z);
        gameManager.importantTransforms["Stamina Bar"].Find("Text").GetComponent<Text>().text = (Mathf.Round(stats.stamina * 10) / 10).ToString();

        if (!inMainMenuScene)
        {
            //Updating timer
            gameManager.importantTransforms["Timer"].GetComponent<Text>().text = stats.timeText;

            gameManager.importantTransforms["Enemies Left"].GetComponent<Text>().text = GameObject.FindGameObjectsWithTag("Hostile").Length + " Enemies Left";

            //Game over when at 0 health
            if (stats.health <= 0)
            {
                stats.health = 0;
                gameManager.importantTransforms["Game Over"].gameObject.SetActive(true);
                gameManager.importantTransforms["Game Over"].Find("Text").GetComponent<Text>().text = "GAME OVER\nScore: " + stats.score;
                GameTime(0);
            }
            else if (stats.health > stats.maxHealth)
            {
                stats.health = stats.maxHealth;
            }
            //Changing x scale of health bar according to player health
            gameManager.importantTransforms["Health Bar"].Find("Green").localScale = new Vector3(stats.health / stats.maxHealth, gameManager.importantTransforms["Health Bar"].localScale.y, gameManager.importantTransforms["Health Bar"].localScale.z);
            gameManager.importantTransforms["Health Bar"].Find("Text").GetComponent<Text>().text = (Mathf.Round(stats.health * 10) / 10).ToString();


            //When no enemies left player wins
            if (!GameObject.FindGameObjectWithTag("Hostile") && !gameManager.importantTransforms["Win Screen"].gameObject.activeSelf)
            {
                gameManager.importantTransforms["Win Screen"].gameObject.SetActive(true);
                if (!stats.highScores.ContainsKey(scene.name))
                {
                    stats.highScores.Add(scene.name, stats.score);
                    gameManager.importantTransforms["Win Screen"].Find("Victory").Find("Record").gameObject.SetActive(true);
                }
                else
                {
                    if (stats.score > stats.highScores[scene.name])
                    {
                        stats.highScores[scene.name] = stats.score;
                        gameManager.importantTransforms["Win Screen"].Find("Victory").Find("Record").gameObject.SetActive(true);
                    }
                }

                gameManager.importantTransforms["Win Screen"].Find("Victory").Find("Score").GetComponent<Text>().text = "Score: " + stats.score;
                gameManager.importantTransforms["Win Screen"].Find("Victory").Find("High Score").GetComponent<Text>().text = "High Score: " + stats.highScores[scene.name];
                SaveGame();
                GameTime(0);
            }
        }
        //Escape to toggle main menu
        if (!gameManager.importantTransforms["Game Over"].gameObject.activeSelf && !gameManager.importantTransforms["Win Screen"].gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameManager.importantTransforms["Main Menu"].gameObject.activeSelf)
            {
                gameManager.importantTransforms["Main Menu"].gameObject.SetActive(false);
                GameTime(1);
            }
            else
            {
                gameManager.importantTransforms["Main Menu"].gameObject.SetActive(true);
                GameTime(0);
            }
        }
    }

    private void OnGUI()
    {
        if(currentKey != null)
        {
            Event e = Event.current;
            if(e.isKey && e.keyCode != KeyCode.Escape)
            {
                currentKey.transform.Find("Text").GetComponent<Text>().text = e.keyCode.ToString();
                gameManager.importantTransforms["Player"].GetComponent<Player>().keys[currentKey.name] = e.keyCode;

                currentKey = null;
            }
            if(e.isMouse)
            {
                currentKey.transform.Find("Text").GetComponent<Text>().text = "Mouse" + e.button;
                gameManager.importantTransforms["Player"].GetComponent<Player>().keys[currentKey.name] = (KeyCode)Enum.Parse(typeof(KeyCode), "Mouse" + e.button);

                currentKey = null;
            }
        }
    }

    void GameTime(float timeScale)
    {
        Cursor.lockState = timeScale != 1 ? CursorLockMode.None : CursorLockMode.Locked;
        Time.timeScale = timeScale;
        gameManager.importantTransforms["Player"].GetComponent<Player>().disableInput = timeScale != 1;
    }

    public void Play()
    {
        gameManager.importantTransforms["Main Menu"].gameObject.SetActive(false);
        GameTime(1);
    }

    public void SaveGame()
    {
        saveData.Save(SaveSystem.currentSaveFileName, true);
    }

    public void NewGame()
    {
        StartCoroutine(sceneLoader.GetComponent<SceneLoader>().LoadScene(1, false, true));
    }

    public void Continue()
    {
        saveData.Load(SaveSystem.currentSaveFileName, true, true);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Retry()
    {
        StartCoroutine(sceneLoader.GetComponent<SceneLoader>().LoadScene(scene.buildIndex, false, false));
    }

    public void NextLevel()
    {
        StartCoroutine(sceneLoader.GetComponent<SceneLoader>().LoadScene(scene.buildIndex + 1, false, false));
    }
     
    public void ActivateMenu(GameObject menu)
    {
        menu.SetActive(true);
    }

    public void DeactivateMenu(GameObject menu)
    {
        menu.SetActive(false);
    }

    public void CreateSave(InputField inputField)
    {
        string fileName = inputField.transform.Find("Text").GetComponent<Text>().text;
        if(fileName == "")
        {
            saveData.Save("save", false);
        }
        else
        {
            saveData.Save(fileName, false);
        }
    }

    public void LoadSelectedSave()
    {
        if(currentSaveSlot != null)
        {
            saveData.Load(currentSaveSlot.name, true, true);
            currentSaveSlot = null;
        }
    }

    public void DeleteSelectedSave()
    {
        if(currentSaveSlot != null)
        {
            saveData.DeleteSave(currentSaveSlot.name);
            currentSaveSlot = null;
        }
    }

    public void SaveSettings()
    {
        saveData.SaveSettings("settings");
    }

    public void LoadSettings(string fileName)
    {
        saveData.LoadSettings(fileName);
    }

    public IEnumerator TextPrompt(string text, float waitTime)
    {
        gameManager.importantTransforms["Text Prompt"].gameObject.SetActive(true);
        gameManager.importantTransforms["Text Prompt"].Find("Text").GetComponent<Text>().text = text;

        yield return new WaitForSeconds(waitTime);

        gameManager.importantTransforms["Text Prompt"].gameObject.SetActive(false);
    }

    public void ChangeSensitivity(Slider slider)
    {
        gameManager.importantTransforms["Player"].GetComponent<Player>().mouseSensitivity = slider.value;
        slider.transform.Find("Text").GetComponent<Text>().text = "Sensitivity " + slider.value;
    }

    public void ChangeFOV(Slider slider)
    {
        gameManager.importantTransforms["Player"].transform.Find("Main Camera").GetComponent<Camera>().fieldOfView = slider.value;
        slider.transform.Find("Text").GetComponent<Text>().text = "FOV " + slider.value;
    }

    public void ToggleInventory()
    {
        if(gameManager.importantTransforms["Inventory"].GetComponent<Image>().color.a == 0)
        {
            foreach(Transform item in gameManager.importantTransforms["Inventory List Content"])
            {
                item.GetComponent<Button>().interactable = true;
            }
            Cursor.lockState = CursorLockMode.None;
            gameManager.importantTransforms["Inventory"].GetComponent<Animation>().Play("SlideDown");
            gameManager.importantTransforms["Player"].GetComponent<Player>().disableInput = true;
        }
        else
        {
            foreach(Transform item in gameManager.importantTransforms["Inventory List Content"])
            {
                item.GetComponent<Button>().interactable = false;
            }
            Cursor.lockState = CursorLockMode.Locked;
            gameManager.importantTransforms["Inventory"].GetComponent<Animation>().Play("SlideUp");
            gameManager.importantTransforms["Player"].GetComponent<Player>().disableInput = false;
        }
    }
}
