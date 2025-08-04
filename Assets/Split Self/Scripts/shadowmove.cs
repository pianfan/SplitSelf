using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shadowmove : MonoBehaviour
{
    [Header("影子设置")]
    public float memorySteps; // 记忆步数
    public GameObject playerObject; // 玩家角色对象
    public Vector3 spawnOffset = Vector3.zero; // 生成位置偏移
    public Transform spawnPoint; // 出生点（父级空物体）
    
    [Header("移动设置")]
    public float moveDistance; // 每次移动的固定距离
    public float moveTime;   // 移动动画时间
    public Vector3 finalPositionOffset = Vector3.zero; // 最终位置偏移量

    public float climbSpeed = 5.0f;
    
    [Header("地面检测设置")]
    public LayerMask groundLayer; // 地面层级
    public float groundCheckDistance = 0.3f; // 地面检测距离
    public Transform groundCheck; // 地面检测点
    
    [Header("梯子检测设置")]
    public LayerMask ladderLayer = 7; // 梯子层级
    public float ladderDetectionDistance = 1.0f; // 梯子检测距离
    
    [Header("向下爬梯子设置")]
    public float downClimbSpeed = 5.0f; // 向下爬梯子速度
    public float downClimbTime = 0.5f; // 向下爬梯子动画时间
    
    [Header("重力设置")]
    public float gravity = 9.8f;
    public float maxFallSpeed = 10f;
    
    private platformcontrol playerController; // 玩家控制器引用
    private List<int> playerMoveHistory = new List<int>(); // 玩家移动历史
    private List<int> shadowMoveQueue = new List<int>(); // 影子移动队列
    
    // 影子移动相关变量
    private bool isMoving = false;
    private Vector2 targetPosition;
    private float moveTimer = 0f;
    private Vector2 startPosition;
    private int currentMoveIndex = 0; // 当前执行的移动索引
    
    // 影子爬梯子相关变量
    private bool isClimbing = false;
    private Vector2 climbTargetPosition;
    private float climbTimer = 0f;
    private Vector2 climbStartPosition;
    private float climbTime = 0.5f; // 爬梯子动画时间
    
    // 影子向下爬梯子相关变量
    private bool isDownClimbing = false;
    private Vector2 downClimbTargetPosition;
    private float downClimbTimer = 0f;
    private Vector2 downClimbStartPosition;
    
    // 地面检测相关变量
    private bool isGrounded = false;
    private float verticalVelocity = 0f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; // 添加SpriteRenderer引用
    
    private bool shadowSpawned = false; // 影子是否已生成
    private float stepCounter = 0f; // 玩家步数计数器
    
    void Start()
    {
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 自动设置地面层级掩码（如果未设置）
        if (groundLayer.value == 0)
        {
            groundLayer = 1 << 6; // 第6层 (Ground)
            Debug.Log("Shadowmove: 自动设置地面层级掩码");
        }
        
        // 自动设置梯子层级掩码（如果未设置）
        if (ladderLayer.value == 0)
        {
            ladderLayer = 1 << 7; // 第7层 (Ladder)
            Debug.Log("Shadowmove: 自动设置梯子层级掩码");
        }
        
        // 获取玩家控制器
        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<platformcontrol>();
        }
        
        // 设置出生点（优先使用指定的出生点，否则使用父级位置）
        if (spawnPoint == null)
        {
            spawnPoint = transform.parent; // 使用父级作为出生点
        }
        
        // 创建地面检测点
        if (groundCheck == null)
        {
            GameObject checkPoint = new GameObject("ShadowGroundCheck");
            checkPoint.transform.SetParent(transform);
            checkPoint.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = checkPoint.transform;
            Debug.Log("Shadowmove: 创建地面检测点");
        }
        
        // 初始化影子位置到出生点
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position + spawnOffset;
            Debug.Log($"影子初始化位置: {transform.position}, 出生点: {spawnPoint.position}");
        }
        else
        {
            // 如果没有出生点，使用玩家位置
            if (playerObject != null)
            {
                transform.position = playerObject.transform.position + spawnOffset;
            }
        }
        
        // 隐藏影子，等待生成
        SetShadowVisible(false);
        
        Debug.Log($"Shadowmove初始化完成 - 地面层级: {groundLayer.value}, 检测距离: {groundCheckDistance}");
    }

    void Update()
    {
        if (playerController == null || playerObject == null) return;
        
        // 地面检测
        CheckGrounded();
        
        // 监听玩家移动
        ListenToPlayerMovement();
        
        // 处理影子移动
        UpdateShadowMovement();
        
        // 应用重力
        ApplyGravity();
        
        // 检查是否应该生成影子
        CheckShadowSpawn();
    }
    
    void ListenToPlayerMovement()
    {
        // 检测玩家是否开始新的移动
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RecordPlayerMove(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            RecordPlayerMove(1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            RecordPlayerMove(0); // 使用0表示E键爬梯子指令
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            RecordPlayerMove(2); // 使用2表示Q键向下爬梯子指令
        }
    }
    
    void RecordPlayerMove(int direction)
    {
        // 记录玩家移动
        playerMoveHistory.Add(direction);
        stepCounter++;
        
        string moveDescription = "";
        switch (direction)
        {
            case -1:
                moveDescription = "向左移动";
                break;
            case 0:
                moveDescription = "E键爬梯子";
                break;
            case 1:
                moveDescription = "向右移动";
                break;
            case 2:
                moveDescription = "Q键向下爬梯子";
                break;
            default:
                moveDescription = "未知移动";
                break;
        }
        
        Debug.Log($"记录玩家移动: {moveDescription} (方向值: {direction}), 总步数: {stepCounter}");
        
        // 如果影子已生成，同步执行影子的记忆步数
        if (shadowSpawned)
        {
            ExecuteShadowMoveWithPlayer(direction);
        }
    }
    
    void CheckShadowSpawn()
    {
        // 当玩家完成记忆步数后，立即生成影子
        if (!shadowSpawned && stepCounter >= memorySteps)
        {
            SpawnShadow();
        }
    }
    
    void SpawnShadow()
    {
        shadowSpawned = true;
        
        // 设置影子位置到出生点
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position + spawnOffset;
            Debug.Log($"影子生成在出生点: {spawnPoint.position}");
        }
        else
        {
            // 如果没有出生点，使用玩家位置
            if (playerObject != null)
            {
                transform.position = playerObject.transform.position + spawnOffset;
            }
        }
        
        // 显示影子
        SetShadowVisible(true);
        
        // 准备影子的移动队列（使用玩家前memorySteps步的移动）
        PrepareShadowMoveQueue();
        
        Debug.Log($"影子生成！记忆步数: {memorySteps}, 当前玩家步数: {stepCounter}");
    }
    
    void PrepareShadowMoveQueue()
    {
        shadowMoveQueue.Clear();
        
        // 获取玩家移动历史的前memorySteps步
        int stepsToUse = Mathf.Min((int)memorySteps, playerMoveHistory.Count);
        
        for (int i = 0; i < stepsToUse; i++)
        {
            shadowMoveQueue.Add(playerMoveHistory[i]);
        }
        
        Debug.Log($"影子移动队列准备完成，队列长度: {shadowMoveQueue.Count}, 使用的步数: {stepsToUse}");
    }
    
    void UpdateShadowMovement()
    {
        if (!shadowSpawned) return;
        
        // 如果影子正在移动，更新移动动画
        if (isMoving)
        {
            moveTimer += Time.deltaTime;
            float progress = moveTimer / moveTime;
            
            if (progress >= 1f)
            {
                // 移动完成
                transform.position = targetPosition;
                isMoving = false;
                
                // 移动完成后立即检测地面
                CheckGroundedAfterMove();
                
                Debug.Log($"影子移动完成，位置: {transform.position}");
            }
            else
            {
                // 使用插值进行平滑移动
                Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, progress);
                transform.position = newPosition;
            }
        }
        
        // 如果影子正在爬梯子，更新爬梯子动画
        if (isClimbing)
        {
            climbTimer += Time.deltaTime;
            float progress = climbTimer / climbTime;
            
            if (progress >= 1f)
            {
                // 爬梯子完成
                transform.position = climbTargetPosition;
                isClimbing = false;
                
                // 爬梯子完成后也检测地面
                CheckGroundedAfterMove();
                
                Debug.Log($"影子爬梯子完成，位置: {transform.position}");
            }
            else
            {
                // 使用插值进行平滑爬梯子
                Vector2 newPosition = Vector2.Lerp(climbStartPosition, climbTargetPosition, progress);
                transform.position = newPosition;
            }
        }
        
        // 如果影子正在向下爬梯子，更新向下爬梯子动画
        if (isDownClimbing)
        {
            downClimbTimer += Time.deltaTime;
            float progress = downClimbTimer / downClimbTime;
            
            if (progress >= 1f)
            {
                // 向下爬梯子完成
                transform.position = downClimbTargetPosition;
                isDownClimbing = false;
                
                // 向下爬梯子完成后也检测地面
                CheckGroundedAfterMove();
                
                Debug.Log($"影子向下爬梯子完成，位置: {transform.position}");
            }
            else
            {
                // 使用插值进行平滑向下爬梯子
                Vector2 newPosition = Vector2.Lerp(downClimbStartPosition, downClimbTargetPosition, progress);
                transform.position = newPosition;
            }
        }
    }
    
    void ExecuteShadowMoveWithPlayer(int playerDirection)
    {
        // 检查影子是否还有记忆步数可以执行
        if (currentMoveIndex < shadowMoveQueue.Count)
        {
            // 获取影子的记忆步数
            int shadowDirection = shadowMoveQueue[currentMoveIndex];
            
            // 执行影子的移动（与玩家同步）
            bool instructionExecuted = ExecuteShadowMove(shadowDirection);
            
            // 移动到下一个记忆步数
            currentMoveIndex++;
            
            string playerMoveDesc = GetMoveDescription(playerDirection);
            string shadowMoveDesc = GetMoveDescription(shadowDirection);
            
            if (instructionExecuted)
            {
                Debug.Log($"影子同步执行移动: {shadowMoveDesc} (方向值: {shadowDirection}), 索引: {currentMoveIndex - 1}, 玩家移动: {playerMoveDesc} (方向值: {playerDirection})");
            }
            else
            {
                Debug.Log($"影子跳过无效指令: {shadowMoveDesc} (方向值: {shadowDirection}), 索引: {currentMoveIndex - 1}, 玩家移动: {playerMoveDesc} (方向值: {playerDirection})");
            }
        }
        else
        {
            Debug.Log("影子已完成所有记忆步数，不再移动");
        }
    }
    
    string GetMoveDescription(int direction)
    {
        switch (direction)
        {
            case -1:
                return "向左移动";
            case 0:
                return "E键爬梯子";
            case 1:
                return "向右移动";
            case 2:
                return "Q键向下爬梯子";
            default:
                return "未知移动";
        }
    }
    
    bool ExecuteShadowMove(int direction)
    {
        if (isMoving || isClimbing || isDownClimbing) return false; // 如果正在移动或爬梯子，不执行新的动作
        
        // 处理E键爬梯子指令
        if (direction == 0)
        {
            // 在执行E键指令前，先检测是否有梯子可以爬
            if (CheckLadderNearby())
            {
                ExecuteShadowClimb();
                Debug.Log("影子检测到E键指令且附近有梯子，执行爬梯子");
                return true;
            }
            else
            {
                Debug.Log("影子检测到E键指令，但附近没有梯子，跳过此指令，等待下一个玩家输入");
                // 跳过这个E键指令，不执行任何动作，等待下一个玩家输入
                return false;
            }
        }
        
        // 处理Q键向下爬梯子指令
        if (direction == 2)
        {
            // 检查是否有梯子可以向下爬
            if (CheckLadderNearby())
            {
                ExecuteShadowDownClimb();
                Debug.Log("影子检测到Q键指令且附近有梯子，执行向下爬梯子");
                return true;
            }
            else
            {
                Debug.Log("影子检测到Q键指令，但附近没有梯子，跳过此指令，等待下一个玩家输入");
                // 跳过这个Q键指令，不执行任何动作，等待下一个玩家输入
                return false;
            }
        }
        
        // 只有在地面上才能移动
        if (!isGrounded) 
        {
            Debug.Log("影子尝试移动但不在平面上，跳过移动指令");
            return false;
        }
        
        // 记录起始位置
        startPosition = transform.position;
        
        // 计算目标位置
        targetPosition = startPosition + new Vector2(direction * moveDistance, 0);
        
        // 自动朝向移动方向
        UpdateShadowDirection(direction);
        
        // 开始移动
        isMoving = true;
        moveTimer = 0f;
        
        Debug.Log($"影子执行移动: {direction}, 目标位置: {targetPosition}");
        return true;
    }
    
    void ExecuteShadowClimb()
    {
        // 影子执行爬梯子动作
        Debug.Log("影子执行爬梯子指令");
        
        // 如果正在移动或爬梯子，不执行新的爬梯子动作
        if (isMoving || isClimbing || isDownClimbing) return;
        
        // 记录爬梯子起始位置
        climbStartPosition = transform.position;
        
        // 计算爬梯子目标位置（向上移动一定距离）
        climbTargetPosition = climbStartPosition + Vector2.up * 2f + (Vector2)finalPositionOffset; // 向上移动2个单位
        
        // 开始爬梯子动画
        isClimbing = true;
        climbTimer = 0f;
        
        Debug.Log($"影子开始爬梯子，起始位置: {climbStartPosition}, 目标位置: {climbTargetPosition}, 应用偏移量: {finalPositionOffset}");
    }
    
    void ExecuteShadowDownClimb()
    {
        // 影子执行向下爬梯子动作
        Debug.Log("影子执行向下爬梯子指令");
        
        // 如果正在移动或爬梯子，不执行新的爬梯子动作
        if (isMoving || isClimbing || isDownClimbing) return;
        
        // 记录向下爬梯子起始位置
        downClimbStartPosition = transform.position;
        
        // 计算向下爬梯子目标位置（向下移动一定距离）
        downClimbTargetPosition = downClimbStartPosition + Vector2.down * 2f; // 向下移动2个单位
        
        // 开始向下爬梯子动画
        isDownClimbing = true;
        downClimbTimer = 0f;
        
        Debug.Log($"影子开始向下爬梯子，起始位置: {downClimbStartPosition}, 目标位置: {downClimbTargetPosition}");
    }
    
    bool CheckLadderNearby()
    {
        // 检测左右两侧是否有梯子
        Vector2 shadowPosition = transform.position;
        
        // 右侧检测
        RaycastHit2D hitRight = Physics2D.Raycast(shadowPosition, Vector2.right, ladderDetectionDistance, ladderLayer);
        // 左侧检测
        RaycastHit2D hitLeft = Physics2D.Raycast(shadowPosition, Vector2.left, ladderDetectionDistance, ladderLayer);
        
        // 绘制调试射线
        Debug.DrawRay(shadowPosition, Vector2.right * ladderDetectionDistance, Color.yellow);
        Debug.DrawRay(shadowPosition, Vector2.left * ladderDetectionDistance, Color.yellow);
        
        bool hasLadder = (hitRight.collider != null || hitLeft.collider != null);
        
        if (hasLadder)
        {
            Debug.Log($"影子检测到梯子: 右侧={hitRight.collider != null}, 左侧={hitLeft.collider != null}");
        }
        
        return hasLadder;
    }
    
    void UpdateShadowDirection(int direction)
    {
        if (spriteRenderer != null)
        {
            // 根据移动方向翻转精灵
            // direction < 0 表示向左移动，flipX = true 面向左
            // direction > 0 表示向右移动，flipX = false 面向右
            spriteRenderer.flipX = direction < 0;
            
            Debug.Log($"影子朝向更新: {(direction < 0 ? "左" : "右")}");
        }
    }
    
    void SetShadowVisible(bool visible)
    {
        // 设置影子的可见性
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
        }
        
        // 设置Rigidbody2D的启用状态
        if (rb != null)
        {
            rb.simulated = visible;
        }
        
        // 如果有其他渲染组件，也可以在这里设置
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = visible;
        }
    }
    
    // 公共方法：重置影子
    public void ResetShadow()
    {
        shadowSpawned = false;
        stepCounter = 0f;
        currentMoveIndex = 0;
        playerMoveHistory.Clear();
        shadowMoveQueue.Clear();
        isMoving = false;
        isClimbing = false; // 重置爬梯子状态
        isDownClimbing = false; // 重置向下爬梯子状态
        
        // 重置影子位置到出生点
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position + spawnOffset;
            Debug.Log($"影子重置到出生点: {spawnPoint.position}");
        }
        
        SetShadowVisible(false);
        
        Debug.Log("影子已重置");
    }
    
    // 公共方法：设置记忆步数
    public void SetMemorySteps(float steps)
    {
        memorySteps = steps;
        Debug.Log($"记忆步数设置为: {steps}");
    }

    void CheckGrounded()
    {
        if (groundCheck == null) 
        {
            Debug.LogWarning("Shadowmove: 地面检测点为空！");
            return;
        }
        
        // 使用射线检测是否在地面上
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        bool wasGrounded = isGrounded;
        isGrounded = hit.collider != null;
        
        // 可视化地面检测（调试用）
        Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, isGrounded ? Color.blue : Color.red);
        
        // 添加详细的调试信息
        if (isGrounded != wasGrounded)
        {
            if (isGrounded)
            {
                Debug.Log($"Shadowmove: 检测到地面 - 位置: {groundCheck.position}, 碰撞体: {hit.collider.gameObject.name}, 层级: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            }
            else
            {
                Debug.Log($"Shadowmove: 离开地面 - 位置: {groundCheck.position}");
            }
        }
        
        // 定期输出检测信息（每60帧一次）
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Shadowmove: 地面检测状态 - 位置: {groundCheck.position}, 检测距离: {groundCheckDistance}, 地面层级: {groundLayer.value}, 是否在地面: {isGrounded}");
        }
    }
    
    void ApplyGravity()
    {
        if (!isGrounded)
        {
            // 应用重力
            verticalVelocity -= gravity * Time.deltaTime;
            
            // 限制最大下落速度
            verticalVelocity = Mathf.Max(verticalVelocity, -maxFallSpeed);
            
            // 更新Y轴速度
            if (rb != null)
            {
                Vector2 velocity = rb.velocity;
                velocity.y = verticalVelocity;
                rb.velocity = velocity;
            }
        }
        else
        {
            // 在地面上时重置垂直速度
            verticalVelocity = 0f;
            if (rb != null)
            {
                Vector2 velocity = rb.velocity;
                velocity.y = 0f;
                rb.velocity = velocity;
            }
        }
    }

    void CheckGroundedAfterMove()
    {
        // 立即检测是否在地面上
        CheckGrounded();
        
        if (!isGrounded)
        {
            Debug.Log($"影子移动后检测到不在平面上，当前位置: {transform.position}，开始应用重力掉落");
            // 立即开始应用重力，让影子掉落到地面
            StartCoroutine(FallToGround());
        }
        else
        {
            Debug.Log($"影子移动后检测到在地面上，位置: {transform.position}");
        }
    }
    
    IEnumerator FallToGround()
    {
        Debug.Log("影子开始重力掉落过程");
        
        // 等待一帧确保地面检测更新
        yield return null;
        
        // 持续应用重力直到接触地面
        while (!isGrounded)
        {
            ApplyGravity();
            yield return null;
        }
        
        Debug.Log($"影子已掉落到地面，最终位置: {transform.position}");
    }
}

