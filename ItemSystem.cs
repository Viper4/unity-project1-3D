using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class ItemSystem : MonoBehaviour
{
    GameManager gameManager;

    public int maxAmmoSlots = 5;
    public int maxAmmoCount = 100;

    Dictionary<string, int> ammoList = new Dictionary<string, int>();
    public List<Transform> interactablesInRange = new List<Transform>();

    Transform selectedItem;

    Animator animator;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.childCount > 0)
        {
            selectedItem = transform.GetChild(0);
            if(!gameManager.importantTransforms["Player"].GetComponent<StatSystem>().inventory.Contains(selectedItem.name))
            {
                gameManager.importantTransforms["Player"].GetComponent<StatSystem>().inventory.Add(selectedItem.name);
            }

            try
            {
                animator = selectedItem.GetComponent<Animator>();
            }
            catch
            {
                Debug.LogWarning("Cannot find animator of " + selectedItem.name + " in Item slot");
            }
        }
    }

    public void Use(int mode)
    {
        foreach (string key in ammoList.Keys)
        {
            if (ammoList[key] <= 0)
            {
                ammoList.Remove(key);
            }
        }
        if (transform.childCount > 0 && !animator.GetCurrentAnimatorStateInfo(0).IsTag("1"))
        {
            switch (selectedItem.tag)
            {
                case "Melee":
                    if (mode == 0)
                        animator.Play("Primary");
                    else
                        animator.Play("Secondary");
                    StartCoroutine(selectedItem.GetComponent<ApplyDmgKBOnColl>().Damage(mode, animator.runtimeAnimatorController.animationClips[mode].length));
                    break;
                case "Ranged":
                    if (ammoList.ContainsKey("Arrow"))
                    {
                        if (mode == 0)
                        {
                            animator.Play("Primary");

                            selectedItem.GetComponent<WeaponSystem>().HoldFire();
                        }
                        else
                        {
                            animator.Play("Secondary");

                            selectedItem.GetComponent<WeaponSystem>().LoadProjectile();
                        }
                    }

                    else
                    {

                    }
                    break;
                case "Gun":
                    if(ammoList.ContainsKey("Bullet"))
                    {
                        
                    }
                    else
                    {
                        gameManager.importantTransforms["Text Prompt"].gameObject.SetActive(true);
                    }

                    break;
            } 
        }
    }

    public void GiveItem(string itemName)
    {
        if(itemName != null)
        {
            if(transform.childCount > 0)
            {
                Drop(false);
            }
            GameObject selectedItemReference = (GameObject)Resources.Load("Prefabs/" + itemName, typeof(GameObject));
            GameObject selectedItem = Instantiate(selectedItemReference, transform, false);
            selectedItem.name = selectedItemReference.name;
        }
        else
        {
            Debug.LogWarning("No itemName given to Instantiate to item slot");
        }
    }

    public void Drop(bool ammo)
    {
        if (ammo)
        {
            
        }
        else
        {
            if (transform.childCount > 0)
            {
                GameObject droppedItemReference = (GameObject)Resources.Load("Prefabs/" + selectedItem.name + "1", typeof(GameObject));
                GameObject droppedItem = Instantiate(droppedItemReference, GameObject.Find("Interactables").transform, false);
                droppedItem.name = Regex.Replace(droppedItemReference.name, "[0-9]", "");
                droppedItem.transform.position = new Vector3(gameManager.importantTransforms["Player"].position.x, gameManager.importantTransforms["Player"].position.y + 0.5f, gameManager.importantTransforms["Player"].position.z) + gameManager.importantTransforms["Player"].forward;

                Destroy(selectedItem.gameObject);
            }
        }
    }

    public void GiveAmmo(string ammoName, int count)
    {
        if (ammoList.ContainsKey(ammoName))
        {
            ammoList[ammoName] += count;
        }
        else
        {
            ammoList.Add(ammoName, count);
        }
    }

    public Transform GetClosestTarget(List<Transform> targets, Transform toThis)
    {
        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;
        foreach (Transform potentialTarget in targets)
        {
            float distanceToTarget = Vector3.Distance(toThis.position, potentialTarget.position);
            if (potentialTarget.GetComponent<InteractableObject>().playerInRange && distanceToTarget < closestDistance)
            {
                closestDistance = distanceToTarget;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
    }
}
