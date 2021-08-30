using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildManager : MonoBehaviour
{
    public int objectLimit = 20;

    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.childCount > objectLimit)
        {
            Debug.Log("Reached " + transform.name + " object limit of " + objectLimit + " deleting " + transform.GetChild(transform.childCount - 1).name);
            Destroy(transform.GetChild(transform.childCount - 1).gameObject);
        }
    }
}
