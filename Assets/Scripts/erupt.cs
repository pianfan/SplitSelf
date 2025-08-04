using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class erupt : MonoBehaviour
{
    [Header("阻隔设置")]
    public bool blockLeft = true;   // 阻隔向左移动
    public bool blockRight = true;  // 阻隔向右移动
    
    [Header("调试设置")]
    public bool showDebugInfo = true; // 是否显示调试信息
    
    private GameObject playerInRange = null; // 在阻隔区域内的玩家
    
    void Start()
    {
        // 确保有BoxCollider2D组件
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
            Debug.Log("erupt: 自动添加BoxCollider2D组件");
        }
        
        // 设置为触发器，用于检测而不产生物理碰撞
        boxCollider.isTrigger = true;
        
        Debug.Log("erupt: 初始化完成");
    }

    void Update()
    {
        // 如果有玩家在阻隔区域内，应用阻隔
        if (playerInRange != null)
        {
            ApplyMovementRestriction(playerInRange);
        }
    }
    
    void ApplyMovementRestriction(GameObject player)
    {
        if (player == null) 
        {
            if (showDebugInfo)
            {
                Debug.Log("erupt: 玩家对象为空");
            }
            return;
        }
        
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null) 
        {
            if (showDebugInfo)
            {
                Debug.Log($"erupt: 玩家 {player.name} 没有Rigidbody2D组件");
            }
            return;
        }
        
        Vector2 velocity = rb.velocity;
        Vector2 originalVelocity = velocity;
        
        if (showDebugInfo && Time.frameCount % 60 == 0) // 每60帧输出一次
        {
            Debug.Log($"erupt: 玩家速度 - X: {velocity.x}, Y: {velocity.y}");
        }
        
        // 根据设置阻隔移动
        if (blockLeft && velocity.x < 0)
        {
            velocity.x = 0;
            if (showDebugInfo)
            {
                Debug.Log("erupt: 阻隔向左移动");
            }
        }
        if (blockRight && velocity.x > 0)
        {
            velocity.x = 0;
            if (showDebugInfo)
            {
                Debug.Log("erupt: 阻隔向右移动");
            }
        }
        
        // 应用阻隔后的速度
        if (velocity != originalVelocity)
        {
            rb.velocity = velocity;
            if (showDebugInfo)
            {
                Debug.Log($"erupt: 应用阻隔 - 原速度: {originalVelocity}, 新速度: {velocity}");
            }
        }
    }
    
    // 当玩家进入碰撞区域时
    void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebugInfo)
        {
            Debug.Log($"erupt: 检测到碰撞进入 - 对象: {other.gameObject.name}, 标签: {other.tag}");
        }
        
        // 检查是否是玩家角色
        if (other.CompareTag("Player") || other.GetComponent<platformcontrol>() != null)
        {
            playerInRange = other.gameObject;
            Debug.Log($"玩家进入阻隔区域: {gameObject.name}");
            
            // 检查玩家组件
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Debug.Log($"erupt: 玩家Rigidbody2D找到 - 当前速度: {rb.velocity}");
            }
            else
            {
                Debug.LogError($"erupt: 玩家 {other.gameObject.name} 没有Rigidbody2D组件！");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"erupt: 对象 {other.gameObject.name} 不是玩家角色");
            }
        }
    }
    
    // 当玩家在碰撞区域内时
    void OnTriggerStay2D(Collider2D other)
    {
        // 确保玩家在范围内
        if ((other.CompareTag("Player") || other.GetComponent<platformcontrol>() != null) && playerInRange == null)
        {
            playerInRange = other.gameObject;
            if (showDebugInfo)
            {
                Debug.Log($"erupt: 玩家在阻隔区域内: {gameObject.name}");
            }
        }
    }
    
    // 当玩家离开碰撞区域时
    void OnTriggerExit2D(Collider2D other)
    {
        if (showDebugInfo)
        {
            Debug.Log($"erupt: 检测到碰撞离开 - 对象: {other.gameObject.name}");
        }
        
        // 检查是否是玩家角色
        if (other.CompareTag("Player") || other.GetComponent<platformcontrol>() != null)
        {
            if (playerInRange == other.gameObject)
            {
                playerInRange = null;
                Debug.Log($"玩家离开阻隔区域: {gameObject.name}");
            }
        }
    }
}
