using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemButton : MonoBehaviour
{
    GameManager gameManager;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        transform.GetComponent<Button>().onClick.AddListener(Give);
    }

    void Give()
    {
        gameManager.importantTransforms["Item Slot"].GetComponent<ItemSystem>().GiveItem(transform.name);
    }
}
