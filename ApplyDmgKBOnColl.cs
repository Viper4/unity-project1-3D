using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyDmgKBOnColl : MonoBehaviour
{
    public float noDamageTime;
    public float baseDamage = 5;
    public float baseKnockback = 3;
    int mode = 0;
    List<Collider> collidersHit = new List<Collider>();

    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.transform.CompareTag("Player") && !other.transform.CompareTag("Item") && !other.transform.CompareTag("Ammo") && !other.transform.CompareTag("Projectile") && !collidersHit.Contains(other))
        {
            collidersHit.Add(other);

            StatSystem otherStats = other.GetComponent<StatSystem>();
            Rigidbody rb = other.attachedRigidbody;
            Vector3 direction = other.transform.position - transform.position;
            direction.y = 0.5f;
            float multiplier;
            if (mode == 0)
            {
                multiplier = 1;
            }
            else
            {
                multiplier = 2;
            }
            if (otherStats != null)
            {
                otherStats.health -= baseDamage / multiplier;
            }
            rb.AddForce(direction.normalized * baseKnockback * multiplier, ForceMode.Impulse);
        }
    }

    public IEnumerator Damage(int attackMode, float waitTime)
    {
        yield return new WaitForSeconds(noDamageTime);
        mode = attackMode;
        transform.GetComponent<BoxCollider>().enabled = true;

        yield return new WaitForSeconds(waitTime - noDamageTime);

        transform.GetComponent<BoxCollider>().enabled = false;
        collidersHit.Clear();
    }
}
