using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class InteractableItem : MonoBehaviour
{
    [Header("互动设置")]
    public TipTrigger dialoguePanel;    // 唯一的对话面板
    public List<DialogueLine> itemDialogues;// 该物品的专属对话
    public bool isDestructible;
    public bool isMirror;
    public bool isDoor;
    public bool isEnd;
    public string targetSceneName;
    public SceneTransitionManager transitionManager;

    [Header("全局提示文本（所有物品共用）")]
    public Text globalTipText;  // 场景中唯一的提示Text
    public Text globalNameText; // 场景中唯一的名称Text
    public string itemName;     // 物品名称

    public bool IsPlayerInRange => _isPlayerInRange;
    public static InteractableItem CurrentInteractable { get; private set; }
    private bool _isPlayerInRange;
    private bool _isInteracting;
    
    void Awake()
    {
        HideGlobalTexts();
        _isInteracting = false;
    }

    void Update()
    {
        if (_isPlayerInRange)
        {
            CurrentInteractable = this; // 更新当前激活物品
            ShowGlobalTexts();

            if (Input.GetKeyDown(KeyCode.E) && !_isInteracting)
            {
                HideGlobalTexts(); 
                _isInteracting = true;
                if (isDoor && transitionManager != null)
                {
                    // 检查目标场景名称是否填写
                    if (string.IsNullOrEmpty(targetSceneName))
                    {
                        Debug.LogError("请在门物品的InteractableItem组件中填写targetSceneName！");
                        _isInteracting = false;
                        return;
                    }
                    // 调用场景切换
                    transitionManager.TransitionToScene(targetSceneName);
                }
                else
                {
                    // 非门物品，正常打开对话
                    ToggleDialogue();
                }
                CurrentInteractable = null;
            }
        }
        else
        {
            if (CurrentInteractable == this) 
            {
                HideGlobalTexts();
                CurrentInteractable = null;
                _isInteracting = false;
            }
        }
    }

    void ToggleDialogue()
    {
        if (dialoguePanel != null)
        {
            // 将全局提示文本传递给对话面板（确保面板能控制）
            dialoguePanel.globalTipText = globalTipText;
            dialoguePanel.globalNameText = globalNameText;
            dialoguePanel.StartDialogue(itemDialogues, this, OnDialogueEnded);
        }
        else
        {
            _isInteracting = false;
        }
    }

    private void OnDialogueEnded()
    {
        _isInteracting = false;
    }
    
    public void ShowGlobalTexts()
    {
        if (globalNameText != null)
        {
            globalNameText.text = itemName;
            globalNameText.gameObject.SetActive(true);
        }
        if (globalTipText != null)
            globalTipText.gameObject.SetActive(true);
    }

    public void HideGlobalTexts()
    {
        if (globalNameText != null)
            globalNameText.gameObject.SetActive(false);
        if (globalTipText != null)
            globalTipText.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            
            // 离开时关闭对话面板（如果是当前物品触发的）
            if (dialoguePanel != null && dialoguePanel.gameObject.activeSelf)
            {
                dialoguePanel.gameObject.SetActive(false);
            }
        }
    }

    // 调试：显示当前激活的物品
    void OnDrawGizmos()
    {
        Gizmos.color = _isPlayerInRange ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}