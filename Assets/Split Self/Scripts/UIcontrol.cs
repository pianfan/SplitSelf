using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 添加UI命名空间

public class UIcontrol : MonoBehaviour
{
    [Header("角色引用")]
    public GameObject playerObject; // 玩家角色对象
    public GameObject shadowObject; // 影子角色对象
    
    [Header("UI面板设置")]
    public GameObject gameOverPanel; // 游戏结束UI面板
    public Button restartButton; // 重新开始按钮
    public Button quitButton; // 退出按钮
    
    [Header("边界检测设置")]
    public float fallThreshold = -10f; // 掉落阈值（Y坐标）
    public float leftBoundary = -20f; // 左边界
    public float rightBoundary = 20f; // 右边界
    
    [Header("重置设置")]
    public Vector3 initialPlayerPosition = new Vector3(0f, 0f, 0f); // 玩家初始位置
    public Vector3 initialCameraPosition = new Vector3(0f, 0f, -10f); // 摄像机初始位置
    
    [Header("UI调用计数设置")]
    public int uiCallCount = 0; // 当前UI调用次数
    public bool showUICallDebug = true; // 是否显示UI调用调试信息
    
    private Camera mainCamera; // 主摄像机引用
    private bool gameOverTriggered = false; // 游戏结束是否已触发
    
    void Start()
    {
        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // 初始化UI面板（隐藏）
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // 设置按钮事件
        SetupButtonEvents();
        
        Debug.Log("UI控制器初始化完成");
    }

    void Update()
    {
        // 检查角色是否掉出画面
        CheckCharacterFallOut();
    }
    
    void CheckCharacterFallOut()
    {
        if (gameOverTriggered) return; // 如果已经触发游戏结束，不再检查
        
        bool playerFellOut = false;
        bool shadowFellOut = false;
        
        // 检查玩家角色
        if (playerObject != null)
        {
            playerFellOut = IsCharacterOutOfBounds(playerObject);
        }
        
        // 检查影子角色
        if (shadowObject != null)
        {
            shadowFellOut = IsCharacterOutOfBounds(shadowObject);
        }
        
        // 如果任一角色掉出画面，触发游戏结束
        if (playerFellOut || shadowFellOut)
        {
            TriggerGameOver();
        }
    }
    
    bool IsCharacterOutOfBounds(GameObject character)
    {
        if (character == null) return false;
        
        Vector3 characterPosition = character.transform.position;
        
        // 检查Y坐标（掉落）
        if (characterPosition.y < fallThreshold)
        {
            Debug.Log($"角色 {character.name} 掉落出画面，Y坐标: {characterPosition.y}");
            return true;
        }
        
        // 检查X坐标（左右边界）
        if (characterPosition.x < leftBoundary || characterPosition.x > rightBoundary)
        {
            Debug.Log($"角色 {character.name} 移出画面边界，X坐标: {characterPosition.x}");
            return true;
        }
        
        // 检查是否在摄像机视野外（可选）
        if (mainCamera != null)
        {
            Vector3 screenPoint = mainCamera.WorldToViewportPoint(characterPosition);
            if (screenPoint.x < -0.1f || screenPoint.x > 1.1f || screenPoint.y < -0.1f || screenPoint.y > 1.1f)
            {
                Debug.Log($"角色 {character.name} 在摄像机视野外，屏幕坐标: {screenPoint}");
                return true;
            }
        }
        
        return false;
    }
    
    void TriggerGameOver()
    {
        if (gameOverTriggered) return; // 防止重复触发
        
        gameOverTriggered = true;
        
        // 增加UI调用次数
        IncrementUICallCount();
        
        // 显示游戏结束面板
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("游戏结束面板已显示");
        }
        else
        {
            Debug.LogWarning("游戏结束面板未设置！");
        }
        
        // 暂停游戏时间（可选）
        Time.timeScale = 0f;
        
        Debug.Log("游戏结束触发 - 角色掉出画面");
    }
    
    // 增加UI调用次数
    public void IncrementUICallCount()
    {
        uiCallCount++;
        
        if (showUICallDebug)
        {
            Debug.Log($"UIcontrol: UI调用次数增加 - 当前: {uiCallCount}");
        }
        
        // 通知Endforplayer脚本
        NotifyEndforplayer();
    }
    
    // 通知Endforplayer脚本
    void NotifyEndforplayer()
    {
        Endforplayer endScript = FindObjectOfType<Endforplayer>();
        if (endScript != null)
        {
            endScript.CheckUICallCount(uiCallCount);
        }
        else
        {
            if (showUICallDebug)
            {
                Debug.LogWarning("UIcontrol: 未找到Endforplayer脚本");
            }
        }
    }
    
    // 公共方法：获取当前UI调用次数
    public int GetUICallCount()
    {
        return uiCallCount;
    }
    
    // 公共方法：设置UI调用次数
    public void SetUICallCount(int count)
    {
        uiCallCount = count;
        
        if (showUICallDebug)
        {
            Debug.Log($"UIcontrol: UI调用次数设置为: {uiCallCount}");
        }
        
        // 通知Endforplayer脚本
        NotifyEndforplayer();
    }
    
    // 公共方法：重置UI调用次数
    public void ResetUICallCount()
    {
        uiCallCount = 0;
        
        if (showUICallDebug)
        {
            Debug.Log("UIcontrol: UI调用次数已重置");
        }
    }
    
    void SetupButtonEvents()
    {
        // 设置重新开始按钮事件
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        // 设置退出按钮事件
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }
    
    public void RestartGame()
    {
        Debug.Log("重新开始游戏");
        
        // 恢复游戏时间
        Time.timeScale = 1f;
        
        // 隐藏游戏结束面板
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // 重置游戏状态
        ResetGameState();
        
        // 重置角色位置到初始位置
        ResetCharacterPositions();
        
        // 重置所有游戏相关脚本
        ResetAllGameScripts();
        
        Debug.Log("游戏已重置到初始状态");
    }
    
    void ResetCharacterPositions()
    {
        // 重置玩家角色位置
        if (playerObject != null)
        {
            // 获取玩家角色的初始位置（可以从脚本中获取或设置默认位置）
            Vector3 initialPlayerPosition = GetInitialPlayerPosition();
            playerObject.transform.position = initialPlayerPosition;
            
            // 重置玩家角色的物理状态
            Rigidbody2D playerRb = playerObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }
            
            Debug.Log($"玩家角色重置到初始位置: {initialPlayerPosition}");
        }
        
        // 重置影子角色位置和状态
        if (shadowObject != null)
        {
            // 重置影子脚本状态
            shadowmove shadowScript = shadowObject.GetComponent<shadowmove>();
            if (shadowScript != null)
            {
                shadowScript.ResetShadow();
            }
            
            // 重置影子角色的物理状态
            Rigidbody2D shadowRb = shadowObject.GetComponent<Rigidbody2D>();
            if (shadowRb != null)
            {
                shadowRb.velocity = Vector2.zero;
                shadowRb.angularVelocity = 0f;
            }
            
            Debug.Log("影子角色已重置");
        }
    }
    
    void ResetAllGameScripts()
    {
        // 重置玩家控制器脚本
        if (playerObject != null)
        {
            platformcontrol playerScript = playerObject.GetComponent<platformcontrol>();
            if (playerScript != null)
            {
                // 重置玩家控制器的状态
                ResetPlayerController(playerScript);
            }
        }
        
        // 重置影子脚本
        if (shadowObject != null)
        {
            shadowmove shadowScript = shadowObject.GetComponent<shadowmove>();
            if (shadowScript != null)
            {
                shadowScript.ResetShadow();
            }
        }
        
        // 重置其他游戏相关脚本（如果有的话）
        // 例如：游戏管理器、关卡管理器等
        ResetOtherGameScripts();
    }
    
    void ResetPlayerController(platformcontrol playerScript)
    {
        // 重置玩家控制器的状态
        // 这里可以添加重置玩家控制器特定状态的代码
        // 例如：重置移动状态、重力状态等
        
        // 重置梯子控制脚本
        laddercontrol ladderScript = playerObject.GetComponent<laddercontrol>();
        if (ladderScript != null)
        {
            ladderScript.ResetLadderControl();
            Debug.Log("梯子控制脚本已重置");
        }
        
        // 重置向下爬梯子脚本
        downladder downLadderScript = playerObject.GetComponent<downladder>();
        if (downLadderScript != null)
        {
            downLadderScript.ResetDownLadder();
            Debug.Log("向下爬梯子脚本已重置");
        }
        
        Debug.Log("玩家控制器已重置");
    }
    
    void ResetOtherGameScripts()
    {
        // 重置其他游戏相关脚本
        // 例如：游戏管理器、关卡管理器、音效管理器等
        
        // 查找并重置游戏管理器
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
        {
            // 重置游戏管理器的状态
            Debug.Log("游戏管理器已重置");
        }
        
        // 重置摄像机位置（如果需要）
        if (mainCamera != null)
        {
            // 重置摄像机到初始位置
            Vector3 initialCameraPosition = GetInitialCameraPosition();
            mainCamera.transform.position = initialCameraPosition;
            Debug.Log($"摄像机重置到初始位置: {initialCameraPosition}");
        }
    }
    
    Vector3 GetInitialPlayerPosition()
    {
        // 返回在Inspector中设置的初始位置
        return initialPlayerPosition;
    }
    
    Vector3 GetInitialCameraPosition()
    {
        // 返回在Inspector中设置的初始位置
        return initialCameraPosition;
    }
    
    public void QuitGame()
    {
        Debug.Log("退出游戏");
        
        // 恢复游戏时间
        Time.timeScale = 1f;
        
        // 退出游戏（在编辑器中停止播放，在构建版本中退出应用）
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void ResetGameState()
    {
        gameOverTriggered = false;
        
        // 重置角色位置（可选）
        if (playerObject != null)
        {
            // 可以在这里重置玩家位置
            Debug.Log("重置玩家状态");
        }
        
        if (shadowObject != null)
        {
            // 可以在这里重置影子状态
            shadowmove shadowScript = shadowObject.GetComponent<shadowmove>();
            if (shadowScript != null)
            {
                shadowScript.ResetShadow();
            }
            Debug.Log("重置影子状态");
        }
    }
    
    // 公共方法：手动触发游戏结束（用于测试）
    public void TestGameOver()
    {
        TriggerGameOver();
    }
    
    // 公共方法：重置游戏结束状态
    public void ResetGameOverState()
    {
        gameOverTriggered = false;
        Time.timeScale = 1f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
}
