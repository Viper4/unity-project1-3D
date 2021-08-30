using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class InteractableObject : MonoBehaviour
{
    GameManager gameManager;
    static Transform closestItem;

    public int count;
    public bool playerInRange { get; set; }

    ItemSystem itemSystem;

    // Start is called before the first frame update
    void Awake()
    {
        if(gameManager == null)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        itemSystem = gameManager.importantTransforms["Item Slot"].GetComponent<ItemSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(!itemSystem.interactablesInRange.Contains(transform))
            {
                itemSystem.interactablesInRange.Add(transform);
            }
            playerInRange = true;

            closestItem = itemSystem.GetClosestTarget(itemSystem.interactablesInRange, gameManager.importantTransforms["Player"]);

            gameManager.importantTransforms["Text Prompt"].gameObject.SetActive(true);
            string text = "interact";
            switch (closestItem.tag)
            {
                case "Item":
                    text = "pickup " + closestItem.name;
                    break;
                case "Ammo":
                    text = "pickup " + closestItem.GetComponent<InteractableObject>().count + " " + closestItem.name + "s";
                    break;
            }
            gameManager.importantTransforms["Text Prompt"].Find("Text").GetComponent<Text>().text = gameManager.importantTransforms["Player"].GetComponent<Player>().keys["Interact"] + " to " + text;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            itemSystem.interactablesInRange.Remove(transform);
            playerInRange = false;

            gameManager.importantTransforms["Text Prompt"].gameObject.SetActive(false);
        }
    }

    public void Interact()
    {
        switch(closestItem.tag)
        {
            case "Item":
                itemSystem.GiveItem(closestItem.name);
                itemSystem.interactablesInRange.Remove(closestItem);
                Destroy(closestItem.gameObject);
                gameManager.importantTransforms["Text Prompt"].gameObject.SetActive(false);
                break;
            case "Ammo":
                itemSystem.GiveAmmo(closestItem.name, closestItem.GetComponent<InteractableObject>().count);
                Destroy(closestItem.gameObject);
                gameManager.importantTransforms["Text Prompt"].gameObject.SetActive(false);
                break;
        }
    }
}
