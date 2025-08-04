using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 对话数据结构，存储单条对话信息
[System.Serializable]
public class DialogueLine
{
    public bool isSpeakerA; // true为说话人A
    public bool isSpeakerB; // true为说话人B
    [TextArea(2, 5)] public string content; // 对话内容
}

public class DialogueSystem : MonoBehaviour
{
    public Image speakerAImage; // 说话人A的图片
    public Image speakerBImage; // 说话人B的图片
    public Text speakerAText;   // 说话人A的文本
    public Text speakerBText;   // 说话人B的文本
    public Text jointSpeechText;    // 同时说话文本
    public List<DialogueLine> dialogueLines; // 对话列表
    public AudioClip clickSound; // 点击音效

    private int _currentLineIndex;   // 当前对话索引
    private Color _originalColor;    // 原始颜色
    private Color _transparentColor; // 半透明颜色
    private AudioSource _audioSource; // 音频源组件

    void Start()
    {
        // 获取或添加音频源组件
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 初始化颜色（保存原始颜色并创建半透明版本）
        _originalColor = speakerAImage.color;
        _transparentColor = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0.2f);

        // 初始化UI状态
        ResetDialogueUI();
        // 显示第一句对话
        ShowNextDialogue();
    }

    void Update()
    {
        // 点击屏幕任意位置继续对话
        if (Input.GetMouseButtonDown(0))
        {
            // 播放点击音效
            PlayClickSound();
            
            if (_currentLineIndex < dialogueLines.Count)
            {
                // 显示下一句对话
                ShowNextDialogue();
            }
            else
            {
                HideDialoguePanel();
            }
        }
    }

    // 播放点击音效
    private void PlayClickSound()
    {
        _audioSource.PlayOneShot(clickSound);
    }

    // 重置对话UI状态
    public void ResetDialogueUI()
    {
        speakerAText.text = "";
        speakerBText.text = "";
        jointSpeechText.text = "";
        speakerAImage.color = _transparentColor;
        speakerBImage.color = _transparentColor;
        
        jointSpeechText.gameObject.SetActive(false);
    }

    // 显示下一句对话
    public void ShowNextDialogue()
    {
        if (_currentLineIndex >= dialogueLines.Count)
        {
            // 对话结束，隐藏面板
            HideDialoguePanel();
            return;
        }

        // 获取当前对话行数据
        DialogueLine currentLine = dialogueLines[_currentLineIndex];
        
        ResetTextVisibility();
        
        // 处理三种情况：A单独说、B单独说、两人同时说
        if (currentLine.isSpeakerA && currentLine.isSpeakerB)
        {
            // 两人同时说话
            HandleJointSpeech(currentLine);
        }
        else if (currentLine.isSpeakerA)
        {
            // A单独说话
            HandleSingleSpeaker(speakerAImage, speakerAText, currentLine.content);
        }
        else if (currentLine.isSpeakerB)
        {
            // B单独说话
            HandleSingleSpeaker(speakerBImage, speakerBText, currentLine.content);
        }

        _currentLineIndex++;
    }

    // 重置文本可见性
    private void ResetTextVisibility()
    {
        speakerAText.gameObject.SetActive(true);
        speakerBText.gameObject.SetActive(true);
        jointSpeechText.gameObject.SetActive(false);
        
        speakerAText.text = "";
        speakerBText.text = "";
        jointSpeechText.text = "";
    }
    
    // 处理单人说话
    private void HandleSingleSpeaker(Image speakerImage, Text textComponent, string content)
    {
        // 说话人不透明，另一人半透明
        speakerImage.color = _originalColor;
        Image otherImage = (speakerImage == speakerAImage) ? speakerBImage : speakerAImage;
        otherImage.color = _transparentColor;
        
        // 显示当前说话人的文本
        textComponent.text = content;
    }

    // 处理两人同时说话
    private void HandleJointSpeech(DialogueLine line)
    {
        // 两人都不透明
        speakerAImage.color = _originalColor;
        speakerBImage.color = _originalColor;
        
        // 隐藏个人文本，显示共用文本
        speakerAText.gameObject.SetActive(false);
        speakerBText.gameObject.SetActive(false);
        jointSpeechText.gameObject.SetActive(true);
        
        // 设置共用文本内容
        jointSpeechText.text = line.content;
    }
    
    // 对话结束后隐藏面板
    private void HideDialoguePanel()
    {
        gameObject.SetActive(false);
    }
}
