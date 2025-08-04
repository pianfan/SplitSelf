using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController0 : MonoBehaviour
{
    public static PlayerController0 instance;

    public float moveSpeed;
    private bool isMoving;
    private Rigidbody2D theRB;

    private Animator anima;
    private SpriteRenderer theSR;
    private float inputX, inputY;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        anima = GetComponent<Animator>();
        anima.Play("Player_Idle");
        theSR = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(inputX, inputY).normalized;
        theRB.velocity = input * moveSpeed;

        //判断是否移动
        if (input != Vector2.zero)
            anima.SetBool("isMoving", true);
        else
            anima.SetBool("isMoving", false);

        //控制动画水平翻转
        if (theRB.velocity.x < 0)
            theSR.flipX = false;
        else if (theRB.velocity.x > 0)
            theSR.flipX = true;

 
        anima.SetFloat("InputX", inputX);
        anima.SetFloat("InputY", inputY);
    }
}
