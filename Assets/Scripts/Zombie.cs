using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    public AudioClip see;
    public AudioClip hit;
    public AudioClip die;
    AudioSource sound;

    GameObject player;
    BoxCollider2D box;
    Rigidbody2D body;
    Animator animator;
    SpriteRenderer sprite;
    public int health = 3;
    public float speed = 1;
    public float detectionRadius = 1.0f;
    public float damage = 0.05f;
    public LayerMask playerLayer;

    private Vector2 direction;
    private bool dead = false;

    private bool playerDetected;
    private bool playerDetectedLastFrame;

    // Start is called before the first frame update
    void Start()
    {
        sound = GetComponent<AudioSource>();
        player = GameObject.Find("Player");
        body = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        playerDetected = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        if(playerDetected && !dead)
        {
            if (!playerDetectedLastFrame)
            {
                sound.clip = see;
                sound.Play();
                playerDetectedLastFrame = true;
            }
            animator.SetBool("isWalking", true);
            if (player.GetComponent<PlayerController>().state == PlayerController.State.Play)
            {
                direction = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
                body.MovePosition(direction);
                if (player.transform.position.x < transform.position.x)
                    sprite.flipX = true;
                else
                    sprite.flipX = false;
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
            playerDetectedLastFrame = false;
        }

        if (health == 0 && !dead)
        {
            Destroy();
        }
    }

    public void Destroy()
    {
        dead = true;
        sprite.enabled = false;
        box.enabled = false;
        sound.clip = die;
        sound.Play();
        GameObject.Destroy(gameObject, 5.0f);
    }

    public void Damage()
    {
        health -= 1;
        if (health != 0)
        {
            sound.clip = hit;
            sound.Play();
        }
    }
}
