using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public AudioSource swordSwing;


    //Components
    private Rigidbody2D box;
    private Animator animator;
    private SpriteRenderer sprite;
    private Camera cam;
    public GameObject brush;
    private Vector3 brushDefault;
    private Canvas canvas;
    public GameObject sword;

    //Player Variables
    public int direction; //0 = right, 1 = left
    public float health = 1.0f;
    public float runSpeed = 1.0f;
    public int attackFrames = 5;
    private int attackFramesRemaining = 0;

    //Control Variables
    private float horizontal;
    private float vertical;
    public bool shift;
    [SerializeField]
    public bool leftMouse;
    [SerializeField]
    public float paintMultiplier;
    public float paintInital;

    //Drawing via HP Variables
    private Vector3 mousePosLastFrame;
    private bool mouseDownLastFrame;
    private float distance;

    //Control State
    public enum State
    {
        Play,
        Draw,
        Hit,
        Dead,
        Won
    }
    public State state;
    
    // Start is called before the first frame update
    void Start()
    {
        box = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        cam = GetComponentInChildren<Camera>();
        brushDefault = new Vector3(10.0f, 10.0f, 10.0f);
        canvas = FindObjectOfType<Canvas>();

        swordSwing = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        shift = Input.GetKey(KeyCode.LeftShift);
        leftMouse = Input.GetMouseButton(0);
    }

    private void FixedUpdate()
    {
        
        if(!leftMouse)
            brush.transform.position = brushDefault;

        if (state == State.Play && state != State.Dead)
        {
            if (horizontal != 0 || vertical != 0)
                animator.SetBool("isWalking", true);
            else
                animator.SetBool("isWalking", false);
            box.velocity = new Vector2(horizontal * runSpeed, vertical * runSpeed);
            if (vertical < 0)
            {
                sword.transform.localPosition = sword.GetComponent<Sword>().UpPos;
                sword.transform.localRotation = sword.GetComponent<Sword>().UpDownRot;
                sword.GetComponent<SpriteRenderer>().flipY = true;
            }
            if (vertical > 0)
            {
                sword.transform.localPosition = sword.GetComponent<Sword>().DownPos;
                sword.transform.localRotation = sword.GetComponent<Sword>().UpDownRot;
                sword.GetComponent<SpriteRenderer>().flipY = false;
            }
            if (horizontal < 0)
            {
                sword.transform.localPosition = sword.GetComponent<Sword>().RightPos;
                sword.transform.localRotation = Quaternion.Euler(0, 0, 90);
                sword.GetComponent<SpriteRenderer>().flipY = false;
                sprite.flipX = true;
            }
            if (horizontal > 0)
            {
                sword.transform.localPosition = sword.GetComponent<Sword>().LeftPos;
                sword.transform.localRotation = Quaternion.Euler(0, 0, 90);
                sword.GetComponent<SpriteRenderer>().flipY = true;
                sprite.flipX = false;
            }
            if (leftMouse)
            {
                if (!mouseDownLastFrame)
                {
                    swordSwing.Play();
                    if (attackFramesRemaining <= 0) //Check that sword isn't already active
                    {
                        sword.GetComponent<SpriteRenderer>().enabled = true;
                        sword.GetComponent<BoxCollider2D>().enabled = true;
                        attackFramesRemaining = attackFrames;
                    }
                    mouseDownLastFrame = true;
                }
            }
            else
                mouseDownLastFrame = false;
            if (shift)
            {
                state = State.Draw;
                box.velocity = Vector2.zero;
                animator.SetBool("isWalking", false);
            }
        }

        else if (state == State.Draw && state != State.Dead)
        {
            if (!shift)
                state = State.Play;
            if (leftMouse)
            {
                if (!mouseDownLastFrame)
                    health -= paintInital;
                Vector3 temp;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 50f))
                {
                    temp = hit.point;
                    temp.z = 2.0f;
                    brush.transform.position = temp;
                    if (mouseDownLastFrame && mousePosLastFrame != temp)
                    {
                        distance = (temp.x - mousePosLastFrame.x) + (temp.y - mousePosLastFrame.y);
                        if (distance < 0)
                            distance *= -1;
                        distance = Mathf.Sqrt(distance);
                        distance *= paintMultiplier;
                        health -= distance;
                    }
                    mousePosLastFrame = temp;
                }
                mouseDownLastFrame = true;
            }
            else
                mouseDownLastFrame = false;
        }

        if (health <= 0)
            playerDies();

        if (attackFramesRemaining > 0)
        {
            attackFramesRemaining -= 1;
            if (attackFramesRemaining == 0)
            {
                sword.GetComponent<SpriteRenderer>().enabled = false;
                sword.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        UpdateUI();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.tag == "Enemy")
        {
            state = State.Hit;
            // Calculate Angle Between the collision point and the player
            ContactPoint2D contactPoint = collision.GetContact(0);
            Vector2 playerPosition = transform.position;
            Vector2 dir = contactPoint.point - playerPosition;

            // We then get the opposite (-Vector3) and normalize it
            dir = -dir.normalized;

            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            GetComponent<Rigidbody2D>().inertia = 0;

            // And finally we add force in the direction of dir and multiply it by force. 
            // This will push back the player
            GetComponent<Rigidbody2D>().AddForce(dir * 2.0f, ForceMode2D.Impulse);

            health -= collision.transform.GetComponent<Zombie>().damage;
            Invoke("ResetState", 0.2f);
        }

        if (collision.transform.name == "WinTrigger")
            {
                playerWon();
            }
   
    }

    void ResetState()
    {
        if (health <= 0)
            playerDies();
        state = State.Play;
    }

    void UpdateUI()
    {
        Image _health = canvas.GetComponentInChildren<Image>();
        _health.fillAmount = health;
    }

    void playerDies()
    {
        state = State.Dead;
        GameObject GameOverScreen = canvas.transform.GetChild(2).gameObject;
        GameOverScreen.SetActive(true);
    }
    
    void playerWon()
    {
        state = State.Won;
        GameObject VictoryScreen = canvas.transform.GetChild(3).gameObject;
        VictoryScreen.SetActive(true);
    }
}
