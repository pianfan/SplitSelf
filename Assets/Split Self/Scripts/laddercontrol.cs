using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laddercontrol : MonoBehaviour
{
    [Header("梯子检测设置")]
    public float ladderDistance; // 角色与梯子的距离
    public float rayLength;      // 射线检测长度
    public LayerMask ladderLayer;        // 梯子所在的Layer

    [Header("攀爬速度设置")]
    public float climbSpeed = 3.0f;     // 攀爬移动速度

    private Transform playerTransform;
    private bool isNearLadder = false;
    private Transform ladderTransform;
    private bool isClimbing = false;
    private Rigidbody2D playerRb;
    private float originalGravityScale = 1f; // 保存原始重力值
    private bool gravityInitialized = false; // 标记重力是否已初始化

    void Start()
    {
        playerTransform = transform;
        playerRb = GetComponent<Rigidbody2D>();
        
        // 保存原始重力值
        if (playerRb != null)
        {
            originalGravityScale = playerRb.gravityScale;
            gravityInitialized = true;
            Debug.Log($"laddercontrol: 初始化完成，原始重力值: {originalGravityScale}");
        }
        else
        {
            Debug.LogError("laddercontrol: 未找到Rigidbody2D组件！");
        }
        
        // 自动设置梯子层级掩码（如果未设置）
        if (ladderLayer.value == 0)
        {
            ladderLayer = 1 << 7; // 第7层 (Ladder)
            Debug.Log("laddercontrol: 自动设置梯子层级掩码");
        }
    }

    void Update()
    {
        DetectLadder();
        HandleClimb();
        
        // 添加调试信息
        if (Time.frameCount % 120 == 0) // 每2秒输出一次状态
        {
            string ladderName = ladderTransform != null ? ladderTransform.name : "无";
            Debug.Log($"laddercontrol状态 - 靠近梯子: {isNearLadder}, 正在攀爬: {isClimbing}, 重力值: {playerRb?.gravityScale}, 梯子对象: {ladderName}, 原始重力: {originalGravityScale}");
        }
    }

    void DetectLadder()
    {
        // 检测左右两侧的梯子
        Vector2 rightDirection = playerTransform.right;
        Vector2 leftDirection = -playerTransform.right;
        
        // 检测右侧梯子
        RaycastHit2D rightHit = Physics2D.Raycast(playerTransform.position, rightDirection, rayLength, ladderLayer);
        // 检测左侧梯子
        RaycastHit2D leftHit = Physics2D.Raycast(playerTransform.position, leftDirection, rayLength, ladderLayer);

        // 绘制调试射线
        Debug.DrawRay(playerTransform.position, rightDirection * rayLength, Color.blue);
        Debug.DrawRay(playerTransform.position, leftDirection * rayLength, Color.cyan);

        // 优先检测右侧，如果没有则检测左侧
        if (rightHit.collider != null)
        {
            ladderTransform = rightHit.collider.transform;
            Vector2 ladderPos = ladderTransform.position;
            float distance = Mathf.Abs(playerTransform.position.x - ladderPos.x);
            isNearLadder = distance < ladderDistance;
            if (isNearLadder)
            {
                Debug.Log($"检测到右侧梯子: {ladderTransform.name}, 距离: {distance}");
            }
        }
        else if (leftHit.collider != null)
        {
            ladderTransform = leftHit.collider.transform;
            Vector2 ladderPos = ladderTransform.position;
            float distance = Mathf.Abs(playerTransform.position.x - ladderPos.x);
            isNearLadder = distance < ladderDistance;
            if (isNearLadder)
            {
                Debug.Log($"检测到左侧梯子: {ladderTransform.name}, 距离: {distance}");
            }
        }
        else
        {
            isNearLadder = false;
            ladderTransform = null;
        }
    }

    void HandleClimb()
    {
        // 检查是否正在移动，如果是则不处理梯子交互
        platformcontrol playerPlatformControl = GetComponent<platformcontrol>();
        if (playerPlatformControl != null && playerPlatformControl.IsMoving())
        {
            // 如果正在移动，不处理梯子交互
            return;
        }
        
        // 只有在靠近梯子时才能攀爬
        if (isNearLadder && ladderTransform != null)
        {
            if (Input.GetKey(KeyCode.E))
            {
                StartClimbing();
            }
            else
            {
                StopClimbing();
            }
        }
        else
        {
            StopClimbing();
        }
    }

    void StartClimbing()
    {
        if (!isClimbing)
        {
            isClimbing = true;
            Debug.Log("开始攀爬");
            
            // 禁用重力
            if (playerRb != null)
            {
                playerRb.gravityScale = 0f;
                playerRb.velocity = Vector2.zero;
            }
        }
        
        // 向上攀爬移动
        Vector3 newPosition = playerTransform.position + Vector3.up * climbSpeed * Time.deltaTime;
        playerTransform.position = newPosition;
    }

    void StopClimbing()
    {
        if (isClimbing)
        {
            isClimbing = false;
            Debug.Log("停止攀爬");
            
            // 恢复重力
            if (playerRb != null && gravityInitialized)
            {
                playerRb.gravityScale = originalGravityScale;
                Debug.Log($"恢复重力值: {originalGravityScale}");
            }
            else if (playerRb != null && !gravityInitialized)
            {
                // 如果重力没有初始化，设置为默认值1
                playerRb.gravityScale = 1f;
                originalGravityScale = 1f;
                gravityInitialized = true;
                Debug.Log("重力未初始化，设置为默认值1");
            }
        }
    }
    
    // 公共方法：重置脚本状态
    public void ResetLadderControl()
    {
        isClimbing = false;
        isNearLadder = false;
        ladderTransform = null;
        
        // 确保重力被正确恢复
        if (playerRb != null)
        {
            if (gravityInitialized)
            {
                playerRb.gravityScale = originalGravityScale;
                Debug.Log($"laddercontrol重置完成，重力值: {originalGravityScale}");
            }
            else
            {
                // 如果重力没有初始化，重新初始化
                originalGravityScale = 1f;
                playerRb.gravityScale = originalGravityScale;
                gravityInitialized = true;
                Debug.Log("laddercontrol重置时重新初始化重力值: 1");
            }
        }
    }
    
    // 公共方法：强制恢复重力
    public void ForceRestoreGravity()
    {
        if (playerRb != null)
        {
            if (gravityInitialized)
            {
                playerRb.gravityScale = originalGravityScale;
                Debug.Log($"强制恢复重力值: {originalGravityScale}");
            }
            else
            {
                playerRb.gravityScale = 1f;
                originalGravityScale = 1f;
                gravityInitialized = true;
                Debug.Log("强制恢复重力值: 1 (重新初始化)");
            }
        }
    }
}
