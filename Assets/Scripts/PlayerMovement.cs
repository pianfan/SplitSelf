using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int InputX = Animator.StringToHash("InputX");
    private static readonly int InputY = Animator.StringToHash("InputY");
    public float speed;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private float _inputX, _inputY;
    private float _stopX, _stopY;
    
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        Vector2 input = new Vector2(_inputX, _inputY).normalized;
        _rigidbody.velocity = input * speed;
    }

    void Update()
    {
        _inputX = Input.GetAxisRaw("Horizontal");
        _inputY = Input.GetAxisRaw("Vertical");

        _animator.SetBool(IsMoving, new Vector2(_inputX, _inputY) != Vector2.zero);
        if (_inputX != 0 || _inputY != 0)
        {
            _stopX = _inputX;
            _stopY = _inputY;
        }
        _animator.SetFloat(InputX, _stopX);
        _animator.SetFloat(InputY, _stopY);
    }
}
