using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platformcontrol : MonoBehaviour
{
    [Header("重力设置")]
    public float gravity = 9.8f;
    public float maxFallSpeed = 10f;
    
    [Header("地面检测")]
    public LayerMask groundLayer;  // 在Inspector中选择Ground层级
    public LayerMask ladderLayer;  // 在Inspector中选择Ladder层级
    public LayerMask endLayer;     // 在Inspector中选择EndLayer层级
    public float groundCheckDistance = -0.1f; // 增加检测距离
    public Transform groundCheck; // 地面检测点
    
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float moveDistance = 4f; // 每次移动的固定距离
    public float moveTime = 0.2f;   // 移动动画时间
    
    [Header("质心设置")]
    public Transform centerOfMass; // 可选的质心参考点
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isOnEndLayer; // 新增：是否在结束层上
    private float verticalVelocity;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    
    // 点按移动相关变量
    private bool isMoving = false;
    private Vector2 targetPosition;
    private float moveTimer = 0f;
    private Vector2 startPosition;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // 设置质心
        if (centerOfMass != null)
        {
            rb.centerOfMass = centerOfMass.localPosition;
        }
        
        // 如果没有设置地面检测点，创建一个
        if (groundCheck == null)
        {
            GameObject checkPoint = new GameObject("GroundCheck");
            checkPoint.transform.SetParent(transform);
            // 调整地面检测点位置到脚部（根据角色大小调整）
            checkPoint.transform.localPosition = new Vector3(0, -7.0f, 0);
            groundCheck = checkPoint.transform;
        }
        
        // 添加初始化调试信息
        Debug.Log($"角色初始化完成 - 地面层级: {groundLayer}, 梯子层级: {ladderLayer}, 结束层级: {endLayer}, 检测距离: {groundCheckDistance}");
        Debug.Log($"地面层级掩码值: {groundLayer.value}, 梯子层级掩码值: {ladderLayer.value}, 结束层级掩码值: {endLayer.value}");
        
        // 如果LayerMask没有设置，自动设置
        if (groundLayer.value == 0)
        {
            groundLayer = 1 << 6; // 第6层 (Ground)
            Debug.Log("自动设置地面层级掩码");
        }
        if (ladderLayer.value == 0)
        {
            ladderLayer = 1 << 7; // 第7层 (Ladder)
            Debug.Log("自动设置梯子层级掩码");
        }
        if (endLayer.value == 0)
        {
            endLayer = 1 << 8; // 第8层 (EndLayer)
            Debug.Log("自动设置结束层级掩码");
        }
    }

    void Update()
    {
        // 地面检测
        CheckGrounded();
        
        // 处理输入
        HandleInput();
        
        // 处理移动动画
        UpdateMove();
        
        // 应用重力
        ApplyGravity();
        
        // 更新动画
        UpdateAnimations();
    }
    
    void UpdateMove()
    {
        if (isMoving)
        {
            moveTimer += Time.deltaTime;
            float progress = moveTimer / moveTime;
            
            if (progress >= 1f)
            {
                // 移动完成
                transform.position = targetPosition;
                isMoving = false;
                rb.velocity = Vector2.zero; // 停止移动
                Debug.Log("移动完成");
            }
            else
            {
                // 使用插值进行平滑移动
                Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, progress);
                transform.position = newPosition;
                
                // 计算移动速度用于动画
                Vector2 velocity = (targetPosition - startPosition) / moveTime;
                rb.velocity = velocity;
            }
        }
    }
    
    void CheckGrounded()
    {
        // 检测地面层
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        bool wasGrounded = isGrounded;
        isGrounded = groundHit.collider != null;
        
        // 检测结束层
        RaycastHit2D endHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, endLayer);
        bool wasOnEndLayer = isOnEndLayer;
        isOnEndLayer = endHit.collider != null;
        
        // 可视化地面检测（调试用）
        if (isGrounded)
        {
            Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, Color.green);
        }
        else if (isOnEndLayer)
        {
            Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, Color.yellow);
        }
        else
        {
            Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, Color.red);
        }
        
        // 添加调试信息
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"尝试跳跃 - 在地面上: {isGrounded}, 在结束层上: {isOnEndLayer}, 检测点位置: {groundCheck.position}");
        }
        
        // 状态变化时的调试信息
        if (isGrounded != wasGrounded)
        {
            if (isGrounded)
            {
                Debug.Log($"角色接触地面层 - 碰撞体: {groundHit.collider.gameObject.name}");
            }
            else
            {
                Debug.Log("角色离开地面层");
            }
        }
        
        if (isOnEndLayer != wasOnEndLayer)
        {
            if (isOnEndLayer)
            {
                Debug.Log($"角色接触结束层 - 碰撞体: {endHit.collider.gameObject.name}");
            }
            else
            {
                Debug.Log("角色离开结束层");
            }
        }
    }
    
    void HandleInput()
    {
        // 如果正在移动中，不处理新的输入
        if (isMoving)
        {
            return;
        }
        
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // 检测点按输入（只在按下瞬间触发）
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartMove(-1); // 向左移动
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartMove(1); // 向右移动
        }
        
        // 跳跃（保持原有功能）
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || isOnEndLayer))
        {
            Jump();
            Debug.Log("跳跃成功！");
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && !isOnEndLayer)
        {
            Debug.Log("无法跳跃：不在地面上");
        }
    }
    
    void StartMove(int direction)
    {
        if (!isGrounded && !isOnEndLayer) return; // 只有在地面上或结束层上才能移动
        
        // 记录起始位置
        startPosition = transform.position;
        
        // 计算目标位置
        targetPosition = startPosition + new Vector2(direction * moveDistance, 0);
        
        Debug.Log($"尝试移动 - 方向: {direction}, 起始位置: {startPosition}, 目标位置: {targetPosition}");
        
        // 检查目标位置是否可行（防止穿墙）
        if (CanMoveTo(targetPosition))
        {
            isMoving = true;
            moveTimer = 0f;
            
            // 翻转角色朝向
            spriteRenderer.flipX = direction < 0;
            
            Debug.Log($"开始移动 - 方向: {direction}, 距离: {moveDistance}");
        }
        else
        {
            Debug.Log($"移动被阻止 - 方向: {direction}, 目标位置: {targetPosition}");
        }
    }
    
    bool CanMoveTo(Vector2 targetPos)
    {
        // 简单的碰撞检测，防止移动到墙里
        Collider2D[] colliders = Physics2D.OverlapCircleAll(targetPos, 0.3f);
        Debug.Log($"检测目标位置: {targetPos}, 发现碰撞体数量: {colliders.Length}");
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                // 检查是否为地面层级、梯子层级或结束层级
                bool isGroundLayer = ((1 << collider.gameObject.layer) & groundLayer) != 0;
                bool isLadderLayer = ((1 << collider.gameObject.layer) & ladderLayer) != 0;
                bool isEndLayer = ((1 << collider.gameObject.layer) & endLayer) != 0;
                
                Debug.Log($"碰撞体: {collider.gameObject.name}, 层级: {collider.gameObject.layer}, 地面层: {isGroundLayer}, 梯子层: {isLadderLayer}, 结束层: {isEndLayer}");
                
                // 允许移动到地面层、梯子层和结束层
                if (isGroundLayer || isLadderLayer || isEndLayer)
                {
                    // 如果目标位置有地面、梯子或结束层碰撞体，允许移动
                    continue;
                }
                else
                {
                    // 如果有其他碰撞体，阻止移动
                    Debug.Log($"阻止移动到层级: {collider.gameObject.layer}, 对象: {collider.gameObject.name}");
                    return false;
                }
            }
        }
        Debug.Log("目标位置可以移动");
        return true;
    }
    
    void ApplyGravity()
    {
        if (!isGrounded && !isOnEndLayer)
        {
            // 应用重力
            verticalVelocity -= gravity * Time.deltaTime;
            
            // 限制最大下落速度
            verticalVelocity = Mathf.Max(verticalVelocity, -maxFallSpeed);
            
            // 更新Y轴速度
            Vector2 velocity = rb.velocity;
            velocity.y = verticalVelocity;
            rb.velocity = velocity;
        }
        else
        {
            // 在地面上或结束层上时重置垂直速度
            verticalVelocity = 0f;
            Vector2 velocity = rb.velocity;
            velocity.y = 0f;
            rb.velocity = velocity;
        }
    }
    
    void Jump()
    {
        verticalVelocity = jumpForce;
        Vector2 velocity = rb.velocity;
        velocity.y = jumpForce;
        rb.velocity = velocity;
    }
    
    void UpdateAnimations()
    {
        if (animator != null)
        {
            // 设置移动动画（基于是否正在移动）
            animator.SetBool("isMoving", isMoving);
            
            // 设置跳跃/下落动画
            animator.SetBool("isGrounded", isGrounded || isOnEndLayer);
            animator.SetFloat("verticalVelocity", rb.velocity.y);
        }
    }
    
    // 碰撞检测（可选，用于额外的地面检测）
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查是否与地面碰撞
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // 可以在这里添加额外的地面检测逻辑
        }
        
        // 检查是否与结束层碰撞
        if (((1 << collision.gameObject.layer) & endLayer) != 0)
        {
            Debug.Log($"角色与结束层碰撞: {collision.gameObject.name}");
        }
    }
    
    void OnCollisionExit2D(Collision2D collision)
    {
        // 离开地面时的处理
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // 可以在这里添加离开地面的逻辑
        }
        
        // 离开结束层时的处理
        if (((1 << collision.gameObject.layer) & endLayer) != 0)
        {
            Debug.Log($"角色离开结束层: {collision.gameObject.name}");
        }
    }

    // 公共方法：检查是否正在移动
    public bool IsMoving()
    {
        return isMoving;
    }
    
    // 公共方法：检查是否在地面上（包括结束层）
    public bool IsGrounded()
    {
        return isGrounded || isOnEndLayer;
    }
    
    // 公共方法：检查是否在结束层上
    public bool IsOnEndLayer()
    {
        return isOnEndLayer;
    }
}
