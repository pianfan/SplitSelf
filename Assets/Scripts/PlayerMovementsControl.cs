using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementsControl : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    public float speed = 3f;
    private Vector2 lastInput;
    private Vector2 _lastValidInput;

    private Rigidbody2D theRB;
    public Animator animator;
    public bool isControlled;


    private void Start()
    {
        theRB = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        _lastValidInput = new Vector2(0, -1);
    }

    public void MoveFromInput()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        lastInput = input;
        theRB.velocity = input * speed;
        
        UpdateAnimator(input);
    }

    public void MirrorMove(Vector2 inputToMirror)
    {
        Vector2 mirroredInput = new Vector2(-inputToMirror.x, inputToMirror.y).normalized;
        theRB.velocity =  (mirroredInput * speed);

        UpdateAnimator(mirroredInput);
    }

    public Vector2 GetLastInput()
    {
        return lastInput;
    }

    private void UpdateAnimator(Vector2 input)
    {
        if (input != Vector2.zero)
        {
            _lastValidInput = input;
            animator.SetFloat("InputX", input.x);
            animator.SetFloat("InputY", input.y);
        }
        
        animator.SetBool(IsMoving, input != Vector2.zero);
    }
}
