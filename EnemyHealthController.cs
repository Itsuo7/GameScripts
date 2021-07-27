using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthController : MonoBehaviour
{
    public static EnemyHealthController instance;

    [Header("Enemy Health and Hit Effect")]
    public int health = 150;
    public GameObject hitEffect;
    private Animator anim;
    private Rigidbody2D rb;
    public GameObject deathEffect;

    [Header("Post-Death")]
    public bool shouldDropItem;
    public GameObject[] itemsToDrop;
    public float itemDropPercent;
    public bool shouldShootAfterDead;
    bool isColliding = false;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
       
    }

    public void DamageEnemy(int damage)
    {
        health -= damage;

        anim.SetTrigger("hurt");

        StartCoroutine(EnemyHurtTriggerReset());

        AudioManager.instance.PlaySFX(2);

        HitStop.instance.StopTime(0.05f);

        Instantiate(hitEffect, transform.position, transform.rotation);

        if (health <= 0)
        {
            
            if (isColliding) return;
            Destroy(gameObject);
            AudioManager.instance.PlaySFX(1);
            Instantiate(deathEffect, transform.position, transform.rotation);
            isColliding = true;

            if (shouldShootAfterDead)
            {
                this.GetComponent<RangedAttack>().BulletPoolAttack();
            }

            //Droping Items
            if (shouldDropItem)
            {
                
                float dropChance = Random.Range(0f, 100f);

                if (dropChance < itemDropPercent)
                {
                    int randomItem = Random.Range(0, itemsToDrop.Length);

                    Instantiate(itemsToDrop[randomItem], transform.position, transform.rotation);
                }
            }

            Time.timeScale = 1f;

        }

    }
    IEnumerator EnemyHurtTriggerReset()
    {
        yield return new WaitForSeconds(0.01f);
        anim.ResetTrigger("hurt");
    }
}
