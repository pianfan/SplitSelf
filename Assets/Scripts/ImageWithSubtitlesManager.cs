using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ImageWithSubtitles
{
    public Sprite image;
    [TextArea(3, 10)] public string subtitles;
    public float textSpeed = 0.05f; // 逐字显示速度（未点击时）
}

public class ImageWithSubtitlesManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    
    public Image imageComponent;               // 图片显示组件
    public Text subtitleText;                  // 字幕文本组件
    public List<ImageWithSubtitles> imageSequence; // 图片和字幕序列
    
    public float transitionDuration = 0.5f;     // 过渡动画持续时间
    public Color transitionColor = Color.black; // 过渡颜色（全黑）

    private int _currentIndex;                 // 当前显示的索引
    private Color _originalImageColor;         // 图片原始颜色
    private Color _originalTextColor;          // 原始文本颜色
    private bool _isTransitioning;             // 是否正在过渡
    private bool _isSkipping;                  // 是否正在跳过
    private bool _isTyping;                    // 是否正在逐字显示文字
    private bool _isWaitingForClick;           // 是否等待点击切换画面
    private readonly List<Coroutine> _activeCoroutines = new List<Coroutine>(); // 跟踪活跃的协程

    public bool isEnd;
    public string targetSceneName;             // 结束后要跳转的场景名称
    public float delayBeforeLoad = 0.5f;         // 淡出后延迟多久跳转场景
    
    void Start()
    {
        // 初始化组件引用
        if (imageComponent == null)
            imageComponent = GetComponent<Image>();
        
        if (subtitleText == null)
            Debug.LogError("请指定字幕Text组件！");
        
        // 检查序列是否有效
        if (imageSequence == null || imageSequence.Count == 0)
        {
            Debug.LogError("请在序列中添加图片和字幕！");
            return;
        }

        // 初始化状态
        _originalImageColor = imageComponent.color;
        _originalTextColor = subtitleText.color;
        subtitleText.text = "";
        _currentIndex = 0;
        
        // 开始显示第一张图片
        ShowCurrentImageAndSubtitles();
    }

    void Update()
    {
        // 点击检测（忽略过渡中和跳过状态）
        if (Input.GetMouseButtonDown(0) && !_isTransitioning && !_isSkipping)
        {
            if (_isTyping)
            {
                // 正在打字时点击：立即显示全部文字
                StopAllActiveCoroutines();
                _isTyping = false;
                ShowAllSubtitles();
                _isWaitingForClick = true;
            }
            else if (_isWaitingForClick)
            {
                StartCoroutine(_currentIndex == imageSequence.Count - 1 ? FadeOutAndHide() : TransitionToNext());
                _isWaitingForClick = false;
            }
        }
    }

    // 显示当前索引的图片和字幕
    private void ShowCurrentImageAndSubtitles()
    {
        // 停止所有活跃的协程
        StopAllActiveCoroutines();
        
        if (_currentIndex >= imageSequence.Count) return;
        
        // 设置当前图片并确保可见
        imageComponent.sprite = imageSequence[_currentIndex].image;
        imageComponent.color = _originalImageColor;
        imageComponent.enabled = true;
        subtitleText.enabled = true;
        subtitleText.color = _originalTextColor;
        subtitleText.text = "";
        
        // 重置状态
        _isTyping = true;
        _isWaitingForClick = false;
        
        // 开始逐行显示字幕并跟踪协程
        Coroutine coroutine = StartCoroutine(ShowSubtitlesLineByLine());
        _activeCoroutines.Add(coroutine);
    }

    // 逐行显示字幕（可被点击中断）
    private IEnumerator ShowSubtitlesLineByLine()
    {
        var currentData = imageSequence[_currentIndex];
        string[] lines = currentData.subtitles.Split('\n');
        string fullText = currentData.subtitles; // 完整文字备份
        
        foreach (string line in lines)
        {
            // 逐字显示当前行
            for (int i = 0; i <= line.Length; i++)
            {
                if (!_isTyping) break; // 若被点击中断，退出循环
                subtitleText.text = fullText.Substring(0, GetCharacterIndex(fullText, lines, line, i));
                yield return new WaitForSeconds(currentData.textSpeed);
            }
            
            if (!_isTyping) break; // 若被点击中断，退出循环
            yield return null;
        }

        // 自然完成打字（未被点击中断）
        if (_isTyping)
        {
            _isTyping = false;
            ShowAllSubtitles(); // 确保文字完整显示
            _isWaitingForClick = true; // 等待点击切换
        }
    }

    // 计算当前行在完整文本中的索引（处理换行符）
    private int GetCharacterIndex(string fullText, string[] lines, string currentLine, int currentCharIndex)
    {
        int index = 0;
        foreach (var line in lines)
        {
            if (line == currentLine)
            {
                return index + currentCharIndex;
            }
            index += line.Length + 1; // +1 是换行符的长度
        }
        return fullText.Length;
    }

    // 立即显示全部字幕
    private void ShowAllSubtitles()
    {
        if (_currentIndex < imageSequence.Count)
        {
            subtitleText.text = imageSequence[_currentIndex].subtitles;
        }
    }

    // 过渡到下一张图片
    private IEnumerator TransitionToNext()
    {
        if (_isTransitioning || _isSkipping) yield break;
        _isTransitioning = true;

        // 渐变为过渡色（全黑）
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            imageComponent.color = Color.Lerp(_originalImageColor, transitionColor, t);
            subtitleText.color = Color.Lerp(_originalTextColor, Color.clear, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        imageComponent.color = transitionColor;
        subtitleText.text = "";

        // 更新索引
        _currentIndex++;

        // 从过渡色渐变回来
        elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            imageComponent.color = Color.Lerp(transitionColor, _originalImageColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        imageComponent.color = _originalImageColor;

        // 显示新图片的字幕
        ShowCurrentImageAndSubtitles();
        
        _isTransitioning = false;
    }

    // 最后一张图片播放完后淡出并隐藏
    private IEnumerator FadeOutAndHide()
    {
        // 渐变为过渡色（全黑）
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            imageComponent.color = Color.Lerp(_originalImageColor, transitionColor, t);
            subtitleText.color = Color.Lerp(_originalTextColor, Color.clear, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 完全隐藏图片和字幕
        imageComponent.enabled = false;
        subtitleText.enabled = false;
        subtitleText.text = "";

        if (isEnd)
        {
            yield return new WaitForSeconds(delayBeforeLoad);
            if (!string.IsNullOrEmpty(targetSceneName))
                SceneManager.LoadScene(targetSceneName);
        }
        
        else if (dialoguePanel != null)
            dialoguePanel.gameObject.SetActive(true);
    }
    
    // 停止所有活跃的协程
    private void StopAllActiveCoroutines()
    {
        foreach (var coroutine in _activeCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        _activeCoroutines.Clear();
    }
}