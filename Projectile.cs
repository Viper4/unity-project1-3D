using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float baseVelocity = 5;
    public ForceMode forceMode = ForceMode.Force;

    Rigidbody rb;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire(float velocityMultiplier)
    {
        Vector3 direction = transform.forward;
        rb.AddForce(direction.normalized * baseVelocity * velocityMultiplier, forceMode);
    }
}
