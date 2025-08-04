using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 添加场景管理命名空间

public class mousekick : MonoBehaviour
{
    [Header("场景跳转设置")]
    public Object targetSceneAsset; // 目标场景资源，可直接拖拽场景文件
    
    [Header("点击设置")]
    public LayerMask clickableLayers = -1; // 可点击的层级
    public bool showDebugInfo = true; // 是否显示调试信息
    
    private Camera mainCamera; // 主摄像机引用
    
    void Start()
    {
        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("mousekick: 未找到摄像机！");
        }
        
        Debug.Log("mousekick: 鼠标点击脚本初始化完成");
    }

    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }
    
    void HandleMouseClick()
    {
        if (mainCamera == null) return;
        
        // 获取鼠标在世界坐标中的位置
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // 确保在2D平面上
        
        // 使用射线检测点击的对象
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, clickableLayers);
        
        if (hit.collider != null)
        {
            // 检查点击的对象是否有碰撞器
            if (hit.collider.gameObject != gameObject) // 避免点击自己
            {
                OnObjectClicked(hit.collider.gameObject);
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"鼠标点击位置: {mousePosition}, 未检测到碰撞器");
            }
        }
    }
    
    void OnObjectClicked(GameObject clickedObject)
    {
        if (showDebugInfo)
        {
            Debug.Log($"点击了对象: {clickedObject.name}");
        }
        
        // 检查是否有碰撞器组件
        Collider2D collider = clickedObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            // 执行场景跳转
            JumpToTargetScene();
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"对象 {clickedObject.name} 没有碰撞器组件");
            }
        }
    }
    
    void JumpToTargetScene()
    {
        string sceneToLoad = GetTargetSceneName();
        
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("mousekick: 目标场景资源未设置！请在Inspector中拖拽场景文件到targetSceneAsset字段");
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
            Debug.LogError($"mousekick: 场景跳转失败 - {e.Message}");
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
    
    // 公共方法：检查目标场景是否存在
    public bool IsTargetSceneValid()
    {
        string sceneName = GetTargetSceneName();
        if (string.IsNullOrEmpty(sceneName)) return false;
        
        // 检查场景是否存在于构建设置中
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameFromPath == sceneName)
            {
                return true;
            }
        }
        
        return false;
    }
}
