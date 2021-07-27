using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public float moveSpeed;
    public GameObject trailEffect;
    public Transform trailPoint;


    private float timeBtwTrail;
    public float trailDelay;
    public float speedMax = 12f;
    //Vector2 armazena 2 valores, um para x, outro para y
    private Vector2 moveInput;

    public Rigidbody2D theRB;

    public Transform gunArm;

    public Animator anim;
    //Shooting
    /*public GameObject bulletToFire;
    public Transform firePoint;
    public float timeBetweenShots;
    private float shotCounter;
    */
    public SpriteRenderer bodySR;

    private float activeMoveSpeed;

    //Variáveis do dash, junto com o activeMoveSpeed, que vai armazenar qual a velocidade do personagem na hora da ação
    public float dashSpeed = 8f, dashLength = .5f, dashCoolDown = 1f, dashInvincibility = .5f;
    [HideInInspector]
    public float dashCounter;
    private float dashCoolCounter;

    [HideInInspector]
    public bool canMove = true;
    [HideInInspector]
    public bool isMoving = false;

    public List<Gun> availableGuns = new List<Gun>();
    [HideInInspector]
    public int currentGun;

    private void Awake()
    {
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // theCam = Camera.main;

        activeMoveSpeed = moveSpeed;

        UIController.instance.currentGun.sprite = availableGuns[currentGun].gunUI;
        UIController.instance.gunText.text = availableGuns[currentGun].weaponName;
    }


    void Update()
    {
        GetPlayerInput();
    }

    // função pra trocar de arma 
    public void SwitchGun()
    {
        /*Para cada arma, cada vez q o botao for apertado, vai setar todas elas como falso, depois selecionar a arma com o valor int correspondente à currentGun*/
        foreach(Gun theGun in availableGuns)
        {
            theGun.gameObject.SetActive(false);
        }

        availableGuns[currentGun].gameObject.SetActive(true);

        UIController.instance.currentGun.sprite = availableGuns[currentGun].gunUI;
        UIController.instance.gunText.text = availableGuns[currentGun].weaponName;
    }

    public void GetPlayerInput()
    {
        if (canMove && !LevelManager.instance.isPaused)
        {
            //Esse comando checa qual input está sendo usado, armazenando na variável Vector2
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");

            moveInput.Normalize();

            //transform.position += new Vector3(moveInput.x * Time.deltaTime * moveSpeed, moveInput.y * Time.deltaTime * moveSpeed, 0f);
            theRB.velocity = moveInput * activeMoveSpeed;

            Vector3 mousePos = Input.mousePosition;
            Vector3 screenPoint = CameraController.instance.mainCamera.WorldToScreenPoint(transform.localPosition);

            if (mousePos.x < screenPoint.x)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
                gunArm.localScale = new Vector3(-1f, -1f, 1f);
            }
            else
            {
                transform.localScale = Vector3.one; // transform.localScale = new Vector3(1f, 1f, 1f);
                gunArm.localScale = Vector3.one;
            }



            Vector2 offset = new Vector2(mousePos.x - screenPoint.x, mousePos.y - screenPoint.y);
            //Esta função retorna o ângulo entre x e y
            float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            gunArm.rotation = Quaternion.Euler(0, 0, angle);

            /*AQUI FICAVA O CODIGO PARA MANEGAR AS ARMAS, POREM FOI MOVIDO PARA UM SCRIPT PROPRIO CHAMADO GUN*/
            //Trocando armas:
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (availableGuns.Count > 0)
                {
                    currentGun++;
                    if (currentGun >= availableGuns.Count)
                    {
                        currentGun = 0;
                    }

                    SwitchGun();

                }
                else
                {
                    Debug.Log("Player has NO GUNS ! CHECK THE CODE!");
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && isMoving)
            {
                if (dashCoolCounter <= 0 && dashCounter <= 0)
                {
                    activeMoveSpeed = dashSpeed;
                    dashCounter = dashLength;

                    anim.SetTrigger("dash");

                    PlayerHealthController.instance.MakeInvincible(dashInvincibility);

                    AudioManager.instance.PlaySFX(8);
                }
            }

            if (dashCounter > 0)
            {
                dashCounter -= Time.deltaTime;

                if (dashCounter <= 0)
                {
                    activeMoveSpeed = moveSpeed;
                    dashCoolCounter = dashCoolDown;
                }
            }

            if (dashCoolCounter > 0)
            {
                dashCoolCounter -= Time.deltaTime;
            }

            if (moveInput != Vector2.zero)
            {
                anim.SetBool("isMoving", true);
                isMoving = true;
                if (timeBtwTrail <= 0)
                {
                    Instantiate(trailEffect, trailPoint.position, Quaternion.identity);
                    timeBtwTrail = trailDelay;
                }
                else
                {
                    timeBtwTrail -= Time.deltaTime;
                }
            }
            else
            {
                anim.SetBool("isMoving", false);
                isMoving = false;
            }
        }

        else
        {

            anim.SetBool("isMoving", false);
        }
    }

    public void IncreasePlayerSpeed(float speedAmount)
    {
        moveSpeed += speedAmount;
        activeMoveSpeed += speedAmount;

        if (moveSpeed > speedMax || activeMoveSpeed > speedMax)
        {
            moveSpeed = speedMax;
            activeMoveSpeed = speedMax;
        }
    }
}
    