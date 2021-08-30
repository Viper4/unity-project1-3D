using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeybindButton : MonoBehaviour
{
    public Transform userInterface;
    public Transform player;

    // Start is called before the first frame update
    void Awake()
    {
        if (userInterface == null)
        {
            userInterface = GameObject.Find("UI").transform;
        }
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }

        transform.GetComponent<Button>().onClick.AddListener(ChangeKeybind);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ChangeKeybind()
    {
        userInterface.GetComponent<UISystem>().currentKey = transform.gameObject;
    }

    public void ChangeText(string newText)
    {
        transform.Find("Text").GetComponent<Text>().text = newText;
    }
}
