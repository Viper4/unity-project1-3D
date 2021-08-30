using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    GameManager gameManager;

    Animator animator;

    public GameObject projectile;
    GameObject loadedProjectile;
    public float damage = 12;
    public float knockback = 2;
    public float velocityMultiplier = 1;
    public float holdingSpeedMultiplier = 0.65f;

    bool holdingFire;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame 
    void Update()
    {
        if (holdingFire && !Input.GetKey(gameManager.importantTransforms["Player"].GetComponent<Player>().keys["Primary"]))
        {
            holdingFire = false;
            gameManager.importantTransforms["Player"].GetComponent<StatSystem>().speedMultiplier += holdingSpeedMultiplier;
            animator.Play("Fire");
            FireProjectile();
        }
    }

    public void HoldFire()
    {
        loadedProjectile.GetComponent<Animator>().Play("Pull");
        holdingFire = true;
        gameManager.importantTransforms["Player"].GetComponent<StatSystem>().speedMultiplier -= holdingSpeedMultiplier;
    }

    public void FireProjectile()
    {
        transform.GetChild(0).GetComponent<Animator>().Play("Fire");
        loadedProjectile.GetComponent<Projectile>().Fire(velocityMultiplier);
        loadedProjectile.GetComponent<ApplyDmgKBOnColl>().baseDamage += damage;
    }

    public void LoadProjectile()
    {
        loadedProjectile = Instantiate(projectile, transform, false);
        loadedProjectile.GetComponent<Animator>().Play("Load");
    }
}
