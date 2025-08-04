using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRestrict : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    Vector2 input;

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    
    [Header("移动设置")]
    public float moveSpeed; // 移动速度，在Inspector中设置
    public bool usePlayerMovementSpeed = true; // 是否使用PlayerMovement的速度

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    void FixedUpdate()
    {
        // 获取移动速度
        float currentSpeed = moveSpeed;
        
        // 如果启用使用PlayerMovement速度，尝试获取PlayerMovement组件
        if (usePlayerMovementSpeed)
        {
            PlayerMovement playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                currentSpeed = playerMovement.speed;
            }
            else
            {
                // 如果找不到PlayerMovement组件，使用默认速度
                Debug.LogWarning("MoveRestrict: 未找到PlayerMovement组件，使用默认速度");
            }
        }
        
        Vector2 newPos = _rigidbody.position + input * currentSpeed * Time.fixedDeltaTime;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        _rigidbody.MovePosition(newPos);
    }
}
