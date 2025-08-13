using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[System.Serializable]
public class TipLine
{
    public bool isSpeakerA; // true为说话人A
    public bool isSpeakerB; // true为说话人B
    [TextArea(2, 5)] public string content; // 对话内容
}

public class TipTrigger : MonoBehaviour
{
    public Transform player0;
    public Transform player1;
    public CameraControl cameraControl;

    public Sprite newSprite;
    public GameObject endCg;
    
    public Image speakerAImage; 
    public Image speakerBImage; 
    public Text speakerAText;   
    public Text speakerBText;   
    public Text jointSpeechText;    
    public AudioClip clickSound; 
    public Text globalTipText;
    public Text globalNameText;

    private InteractableItem _currentInteractable;
    
    private int _currentLineIndex;   
    private Color _transparentColor; 
    private Color _fullyTransparentColor;
    private AudioSource _audioSource; 

    private bool _isDialogueActive; 
    private List<TipLine> _currentDialogues; // 当前物品的对话序列
    private Action _onDialogueEnded;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        _transparentColor = new Color(Color.white.r, Color.white.g, Color.white.b, 0.2f);
        _fullyTransparentColor = new Color(Color.white.r, Color.white.g, Color.white.b, 0f);
        gameObject.SetActive(false); 
        ResetDialogueUI();
    }

    private void OnEnable()
    {
        HideGlobalTexts();
    }
    
    private void OnDisable()
    {
        // 只有当前有激活的物品且玩家在范围内，才恢复显示提示
        if (InteractableItem.CurrentInteractable != null && InteractableItem.CurrentInteractable.IsPlayerInRange)
        {
            InteractableItem.CurrentInteractable.ShowGlobalTexts();
        }
    }

    void Update()
    {
        if (!_isDialogueActive) return; // 对话未激活时不响应点击

        if (Input.GetMouseButtonDown(0))
        {
            PlayClickSound();
            
            if (_currentLineIndex < _currentDialogues.Count)
            {
                ShowNextDialogue();
            }
            else
            {
                HideDialoguePanel();
                if (_currentInteractable != null)
                {
                    // 处理可破坏逻辑
                    if (_currentInteractable.isDestructible)
                    {
                        Destroy(_currentInteractable.gameObject);
                    }
                    
                    // 处理镜子交换位置逻辑
                    if (_currentInteractable.isMirror)
                    {
                        SwapPlayersPosition();
                    }

                    if (_currentInteractable.isEnd)
                    {
                        endCg.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    private void SwapPlayersPosition()
    {
        // 检查玩家引用是否有效
        if (player0 == null || player1 == null)
        {
            Debug.LogError("请在TipTrigger中赋值player0和player1的引用！");
            return;
        }
    
        (player0.position, player1.position) = (player1.position, player0.position);
        (player0.rotation, player1.rotation) = (player1.rotation, player0.rotation);
        
        if (cameraControl._target == player0)
        {
            cameraControl._target = player1;
        }
        else
        {
            cameraControl._target = player0;
        }
        if (PlayerControl.instance != null)
            PlayerControl.instance.SwitchControl();
    }
    
    private void PlayClickSound()
    {
        _audioSource.PlayOneShot(clickSound);
    }

    public void ResetDialogueUI()
    {
        HideGlobalTexts();
        speakerAText.text = "";
        speakerBText.text = "";
        jointSpeechText.text = "";
        speakerAImage.color = _transparentColor;
        speakerBImage.color = _transparentColor;
        jointSpeechText.gameObject.SetActive(false);
    }

    public void StartDialogue(List<TipLine> dialogues, InteractableItem interactable, Action onEnded)
    {
        _currentInteractable = interactable;
        _currentDialogues = dialogues;
        _currentLineIndex = 0;
        _isDialogueActive = true;
        gameObject.SetActive(true);
        ResetDialogueUI();
        ShowNextDialogue();
        _onDialogueEnded = onEnded;
    }

    private void ShowNextDialogue()
    {
        if (_currentLineIndex >= _currentDialogues.Count)
        {
            HideDialoguePanel();
            return;
        }

        TipLine currentLine = _currentDialogues[_currentLineIndex];
        ResetTextVisibility();
        
        if (currentLine.isSpeakerA && currentLine.isSpeakerB)
        {
            HandleJointSpeech(currentLine);
        }
        else if (currentLine.isSpeakerA)
        {
            HandleSingleSpeaker(speakerAImage, speakerAText, currentLine.content);
        }
        else if (currentLine.isSpeakerB)
        {
            HandleSingleSpeaker(speakerBImage, speakerBText, currentLine.content);
        }
        else if (!currentLine.isSpeakerA && !currentLine.isSpeakerB)
        {
            HandleNarration(currentLine);
        }

        _currentLineIndex++;
    }

    private void ResetTextVisibility()
    {
        HideGlobalTexts();
        speakerAText.gameObject.SetActive(true);
        speakerBText.gameObject.SetActive(true);
        jointSpeechText.gameObject.SetActive(false);
        speakerAText.text = "";
        speakerBText.text = "";
        jointSpeechText.text = "";
    }
    
    private void HandleSingleSpeaker(Image speakerImage, Text textComponent, string content)
    {
        speakerImage.color = Color.white;
        Image otherImage = (speakerImage == speakerAImage) ? speakerBImage : speakerAImage;
        otherImage.color = _transparentColor;
        textComponent.text = content;
    }

    private void HandleJointSpeech(TipLine line)
    {
        speakerAImage.color = Color.white;
        speakerBImage.color = Color.white;
        speakerAText.gameObject.SetActive(false);
        speakerBText.gameObject.SetActive(false);
        jointSpeechText.gameObject.SetActive(true);
        jointSpeechText.text = line.content;
    }
    
    private void HandleNarration(TipLine line)
    {
        speakerAImage.color = _fullyTransparentColor;
        speakerBImage.color = _fullyTransparentColor;
        
        speakerAText.gameObject.SetActive(false);
        speakerBText.gameObject.SetActive(false);
        jointSpeechText.gameObject.SetActive(true);
        
        jointSpeechText.text = line.content;
    }
    
    private void HideDialoguePanel()
    {
        _isDialogueActive = false;
        gameObject.SetActive(false);
        _onDialogueEnded?.Invoke();
    }
    
    private void HideGlobalTexts()
    {
        if (globalTipText != null)
            globalTipText.gameObject.SetActive(false);
        if (globalNameText != null)
            globalNameText.gameObject.SetActive(false);
    }
}
