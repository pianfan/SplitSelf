using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 添加场景管理命名空间

public class Endforplayer : MonoBehaviour
{
    [Header("场景跳转设置")]
    public Object targetSceneAsset; // 目标场景资源，可直接拖拽场景文件
    
    [Header("UI调用计数设置")]
    public int requiredUICalls = 4; // 需要UIcontrol被调用的次数
    public bool showDebugInfo = true; // 是否显示调试信息
    
    private UIcontrol uiController; // UIcontrol脚本引用
    
    void Start()
    {
        // 查找UIcontrol脚本
        uiController = FindObjectOfType<UIcontrol>();
        if (uiController == null)
        {
            Debug.LogError("Endforplayer: 未找到UIcontrol脚本！");
        }
        else
        {
            Debug.Log("Endforplayer: 找到UIcontrol脚本");
        }
        
        Debug.Log($"Endforplayer: 初始化完成 - 需要UI调用次数: {requiredUICalls}");
    }

    void Update()
    {
        // 检查UI调用次数
        CheckUICallCount();
    }
    
    void CheckUICallCount()
    {
        if (uiController != null)
        {
            int currentCalls = uiController.GetUICallCount();
            
            if (showDebugInfo && Time.frameCount % 120 == 0) // 每2秒输出一次
            {
                Debug.Log($"Endforplayer: 当前UI调用次数: {currentCalls}/{requiredUICalls}");
            }
            
            // 检查是否达到跳转条件
            if (currentCalls >= requiredUICalls)
            {
                Debug.Log($"Endforplayer: 达到跳转条件 ({currentCalls}/{requiredUICalls})");
                JumpToTargetScene();
            }
        }
    }
    
    // 公共方法：检查UI调用次数（由UIcontrol调用）
    public void CheckUICallCount(int currentCalls)
    {
        if (showDebugInfo)
        {
            Debug.Log($"Endforplayer: 收到UI调用通知 - 当前次数: {currentCalls}/{requiredUICalls}");
        }
        
        // 检查是否达到跳转条件
        if (currentCalls >= requiredUICalls)
        {
            Debug.Log($"Endforplayer: 达到跳转条件 ({currentCalls}/{requiredUICalls})");
            JumpToTargetScene();
        }
    }
    
    void JumpToTargetScene()
    {
        string sceneToLoad = GetTargetSceneName();
        
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("Endforplayer: 目标场景资源未设置！请在Inspector中拖拽场景文件到targetSceneAsset字段");
            return;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"准备跳转到场景: {sceneToLoad}");
        }
        
        // 执行场景跳转
        try
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Endforplayer: 场景跳转失败 - {e.Message}");
        }
    }
    
    string GetTargetSceneName()
    {
        // 使用场景资源
        if (targetSceneAsset != null)
        {
            return targetSceneAsset.name;
        }
        
        return "";
    }
    
    // 公共方法：手动设置目标场景
    public void SetTargetScene(Object sceneAsset)
    {
        targetSceneAsset = sceneAsset;
        if (showDebugInfo)
        {
            Debug.Log($"目标场景已设置为: {sceneAsset?.name}");
        }
    }
    
    // 公共方法：手动触发场景跳转
    public void TriggerSceneJump()
    {
        JumpToTargetScene();
    }
    
    // 公共方法：检查是否达到跳转条件
    public bool IsReadyToJump()
    {
        if (uiController != null)
        {
            return uiController.GetUICallCount() >= requiredUICalls;
        }
        return false;
    }
}
