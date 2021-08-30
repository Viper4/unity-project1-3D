using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButton : MonoBehaviour
{
    public Transform userInterface;

    // Start is called before the first frame update
    void Awake()
    {
        if (userInterface == null)
        {
            userInterface = GameObject.Find("UI").transform;
        }

        transform.GetComponent<Button>().onClick.AddListener(SaveSelected);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SaveSelected()
    {
        userInterface.GetComponent<UISystem>().currentSaveSlot = transform.gameObject;
    }
}
