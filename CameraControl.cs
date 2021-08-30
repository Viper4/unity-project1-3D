using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float dstToTarget = 0;

    public Transform target;

    // Start is called before the first frame update
    void Awake()
    {
        if (target == null)
        {
            target = transform.parent;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
