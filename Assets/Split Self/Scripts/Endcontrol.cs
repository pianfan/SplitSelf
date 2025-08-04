using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 添加场景管理命名空间

public class Endcontrol : MonoBehaviour
{
    [Header("场景跳转设置")]
    public Object targetSceneAsset; // 目标场景资源（可拖拽赋值）
    public float jumpDelay = 2f; // 跳转延时（秒）
    
    [Header("层级检测设置")]
    public LayerMask endLayer; // 结束层级掩码
    
    [Header("影子角色引用")]
    public GameObject shadowObject; // 影子角色对象
    
    private bool sceneJumpTriggered = false; // 场景跳转是否已触发
    private bool isShadowOnEndLayer = false; // 影子是否在结束层级上
    private string targetSceneName; // 实际的目标场景名称
    
    void Start()
    {
        // 从拖拽的场景资源中获取场景名称
        if (targetSceneAsset != null)
        {
            targetSceneName = targetSceneAsset.name;
            Debug.Log($"从场景资源获取目标场景名称: {targetSceneName}");
        }
        else
        {
            Debug.LogWarning("未设置目标场景资源！请在Inspector中拖拽场景文件到Target Scene Asset字段");
        }
        
        // 自动设置结束层级掩码（如果未设置）
        if (endLayer.value == 0)
        {
            // 尝试查找endlayer层级
            int endLayerIndex = LayerMask.NameToLayer("endlayer");
            if (endLayerIndex != -1)
            {
                endLayer = 1 << endLayerIndex;
                Debug.Log($"Endcontrol: 自动设置结束层级掩码 (endlayer, 层级索引: {endLayerIndex})");
            }
            else
            {
                Debug.LogError("未找到endlayer层级！请确保在Layer设置中创建了endlayer层级");
            }
        }
        
        // 如果没有指定影子对象，尝试自动查找
        if (shadowObject == null)
        {
            shadowObject = GameObject.FindGameObjectWithTag("Shadow");
            if (shadowObject == null)
            {
                // 查找包含shadowmove脚本的对象
                shadowmove shadowScript = FindObjectOfType<shadowmove>();
                if (shadowScript != null)
                {
                    shadowObject = shadowScript.gameObject;
                }
            }
        }
        
        Debug.Log($"Endcontrol初始化完成 - 目标场景: {targetSceneName}, 延时: {jumpDelay}秒, 检测层级: endlayer");
    }

    void Update()
    {
        // 检查影子角色是否在结束层级上
        CheckShadowOnEndLayer();
        
        // 如果影子在结束层级上且未触发跳转，开始跳转流程
        if (isShadowOnEndLayer && !sceneJumpTriggered)
        {
            TriggerSceneJump();
        }
    }
    
    void CheckShadowOnEndLayer()
    {
        if (shadowObject == null) return;
        
        // 使用射线检测影子角色是否在结束层级上
        Vector2 shadowPosition = shadowObject.transform.position;
        Vector2 rayDirection = Vector2.down;
        float rayDistance = 1.0f; // 射线检测距离
        
        // 绘制调试射线
        Debug.DrawRay(shadowPosition, rayDirection * rayDistance, Color.red);
        
        // 执行射线检测
        RaycastHit2D hit = Physics2D.Raycast(shadowPosition, rayDirection, rayDistance, endLayer);
        
        if (hit.collider != null)
        {
            // 影子角色接触到结束层级
            if (!isShadowOnEndLayer)
            {
                isShadowOnEndLayer = true;
                Debug.Log($"影子角色接触到结束层级: {hit.collider.gameObject.name} (层级: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
            }
        }
        else
        {
            // 影子角色离开结束层级
            if (isShadowOnEndLayer)
            {
                isShadowOnEndLayer = false;
                Debug.Log("影子角色离开结束层级");
            }
        }
    }
    
    void TriggerSceneJump()
    {
        if (sceneJumpTriggered) return; // 防止重复触发
        
        sceneJumpTriggered = true;
        Debug.Log($"开始场景跳转流程 - 目标场景: {targetSceneName}, 延时: {jumpDelay}秒");
        
        // 开始延时跳转协程
        StartCoroutine(JumpToSceneWithDelay());
    }
    
    IEnumerator JumpToSceneWithDelay()
    {
        Debug.Log($"等待 {jumpDelay} 秒后跳转到场景: {targetSceneName}");
        
        // 等待指定时间
        yield return new WaitForSeconds(jumpDelay);
        
        // 执行场景跳转
        JumpToTargetScene();
    }
    
    void JumpToTargetScene()
    {
        Debug.Log($"正在跳转到场景: {targetSceneName}");
        
        try
        {
            // 检查场景是否存在
            Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
            if (targetScene.IsValid())
            {
                // 跳转到目标场景
                SceneManager.LoadScene(targetSceneName);
                Debug.Log($"成功跳转到场景: {targetSceneName}");
            }
            else
            {
                Debug.LogError($"场景不存在: {targetSceneName}");
                // 可以在这里添加备用逻辑，比如显示错误信息
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"场景跳转失败: {e.Message}");
        }
    }
    
    // 公共方法：手动触发场景跳转（用于测试）
    public void TestSceneJump()
    {
        if (!sceneJumpTriggered)
        {
            TriggerSceneJump();
        }
    }
    
    // 公共方法：重置跳转状态
    public void ResetJumpState()
    {
        sceneJumpTriggered = false;
        isShadowOnEndLayer = false;
        Debug.Log("Endcontrol跳转状态已重置");
    }
    
    // 公共方法：设置目标场景名称
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
        Debug.Log($"目标场景已设置为: {targetSceneName}");
    }
    
    // 公共方法：设置目标场景资源
    public void SetTargetSceneAsset(Object sceneAsset)
    {
        targetSceneAsset = sceneAsset;
        if (sceneAsset != null)
        {
            targetSceneName = sceneAsset.name;
            Debug.Log($"目标场景资源已设置为: {targetSceneName}");
        }
    }
    
    // 公共方法：设置跳转延时
    public void SetJumpDelay(float delay)
    {
        jumpDelay = delay;
        Debug.Log($"跳转延时已设置为: {jumpDelay}秒");
    }
}
