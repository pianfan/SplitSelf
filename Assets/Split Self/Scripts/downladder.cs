using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class downladder : MonoBehaviour
{
    [Header("向下爬梯子设置")]
    public float climbSpeed = 5.0f; // 向下爬梯子速度
    public float detectionDistance = 1.0f; // 梯子检测距离
    
    [Header("层级设置")]
    public LayerMask ladderLayer; // 梯子层级掩码
    
    [Header("角色引用")]
    public GameObject playerObject; // 玩家角色对象
    
    private bool isClimbing = false; // 是否正在向下爬梯子
    private bool isNearLadder = false; // 是否在梯子附近
    private Transform playerTransform; // 玩家变换组件
    private Rigidbody2D playerRb; // 玩家刚体组件
    private platformcontrol playerPlatformControl; // 玩家控制器脚本
    private float originalGravityScale = 1f; // 保存原始重力值
    private bool gravityInitialized = false; // 标记重力是否已初始化
    
    void Start()
    {
        // 自动设置梯子层级掩码（如果未设置）
        if (ladderLayer.value == 0)
        {
            ladderLayer = 1 << 7; // 第7层 (Ladder)
            Debug.Log("Downladder: 自动设置梯子层级掩码");
        }
        
        // 如果没有指定玩家对象，尝试自动查找
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
            {
                // 查找包含platformcontrol脚本的对象
                platformcontrol playerScript = FindObjectOfType<platformcontrol>();
                if (playerScript != null)
                {
                    playerObject = playerScript.gameObject;
                }
            }
        }
        
        // 获取玩家组件
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            playerRb = playerObject.GetComponent<Rigidbody2D>();
            playerPlatformControl = playerObject.GetComponent<platformcontrol>();
            
            // 保存原始重力值
            if (playerRb != null)
            {
                originalGravityScale = playerRb.gravityScale;
                gravityInitialized = true;
                Debug.Log($"Downladder: 初始化完成，原始重力值: {originalGravityScale}");
            }
            else
            {
                Debug.LogError("Downladder: 未找到Rigidbody2D组件！");
            }
            
            Debug.Log("Downladder: 玩家对象设置完成");
        }
        else
        {
            Debug.LogError("Downladder: 未找到玩家对象！");
        }
        
        Debug.Log("Downladder初始化完成");
    }

    void Update()
    {
        // 检测梯子
        DetectLadder();
        
        // 处理向下爬梯子输入
        HandleClimb();
        
        // 添加调试信息
        if (Time.frameCount % 120 == 0) // 每2秒输出一次状态
        {
            Debug.Log($"Downladder状态 - 靠近梯子: {isNearLadder}, 正在向下爬梯子: {isClimbing}, 重力值: {playerRb?.gravityScale}, 原始重力: {originalGravityScale}");
        }
    }
    
    void DetectLadder()
    {
        if (playerTransform == null) return;
        
        // 检测左右两侧的梯子
        Vector2 playerPosition = playerTransform.position;
        
        // 右侧检测
        RaycastHit2D hitRight = Physics2D.Raycast(playerPosition, Vector2.right, detectionDistance, ladderLayer);
        // 左侧检测
        RaycastHit2D hitLeft = Physics2D.Raycast(playerPosition, Vector2.left, detectionDistance, ladderLayer);
        
        // 绘制调试射线
        Debug.DrawRay(playerPosition, Vector2.right * detectionDistance, Color.blue);
        Debug.DrawRay(playerPosition, Vector2.left * detectionDistance, Color.blue);
        
        // 检查是否在梯子附近
        bool wasNearLadder = isNearLadder;
        isNearLadder = (hitRight.collider != null || hitLeft.collider != null);
        
        if (isNearLadder && !wasNearLadder)
        {
            string ladderName = hitRight.collider != null ? hitRight.collider.name : hitLeft.collider.name;
            Debug.Log($"玩家接近梯子: {ladderName}，可以按Q键向下爬梯子");
        }
        else if (!isNearLadder && wasNearLadder)
        {
            Debug.Log("玩家离开梯子");
        }
    }
    
    void HandleClimb()
    {
        // 检查是否按下Q键
        if (Input.GetKey(KeyCode.Q))
        {
            // 检查是否在梯子附近且玩家不在移动中
            if (isNearLadder && !IsPlayerMoving())
            {
                StartClimbing();
            }
        }
        else
        {
            // 松开Q键停止爬梯子
            if (isClimbing)
            {
                StopClimbing();
            }
        }
    }
    
    void StartClimbing()
    {
        if (!isClimbing)
        {
            isClimbing = true;
            Debug.Log("开始向下爬梯子");
            
            if (playerRb != null)
            {
                playerRb.gravityScale = 0f;
                playerRb.velocity = Vector2.zero;
            }
        }
        
        // 向下移动
        Vector3 newPosition = playerTransform.position + Vector3.down * climbSpeed * Time.deltaTime;
        playerTransform.position = newPosition;
    }
    
    void StopClimbing()
    {
        if (isClimbing)
        {
            isClimbing = false;
            Debug.Log("停止向下爬梯子");
            
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
    
    bool IsPlayerMoving()
    {
        // 检查玩家是否正在移动
        if (playerPlatformControl != null)
        {
            return playerPlatformControl.IsMoving();
        }
        return false;
    }
    
    // 公共方法：重置脚本状态
    public void ResetDownLadder()
    {
        isClimbing = false;
        isNearLadder = false;
        
        // 确保重力被正确恢复
        if (playerRb != null)
        {
            if (gravityInitialized)
            {
                playerRb.gravityScale = originalGravityScale;
                Debug.Log($"Downladder重置完成，重力值: {originalGravityScale}");
            }
            else
            {
                // 如果重力没有初始化，重新初始化
                originalGravityScale = 1f;
                playerRb.gravityScale = originalGravityScale;
                gravityInitialized = true;
                Debug.Log("Downladder重置时重新初始化重力值: 1");
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
    
    // 公共方法：检查是否正在爬梯子
    public bool IsClimbing()
    {
        return isClimbing;
    }
    
    // 公共方法：检查是否在梯子附近
    public bool IsNearLadder()
    {
        return isNearLadder;
    }
    
    // 公共方法：手动开始爬梯子（用于测试）
    public void TestStartClimbing()
    {
        if (isNearLadder && !IsPlayerMoving())
        {
            StartClimbing();
        }
    }
    
    // 公共方法：手动停止爬梯子（用于测试）
    public void TestStopClimbing()
    {
        StopClimbing();
    }
}
