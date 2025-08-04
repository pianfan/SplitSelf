using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ImageWithSubtitles
{
    public Sprite image;
    [TextArea(3, 10)] public string subtitles;
    public float textSpeed = 0.05f;
}

public class ImageWithSubtitlesManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    
    public Image imageComponent;               // 图片显示组件
    public Text subtitleText;                  // 字幕文本组件
    public Button skipButton;                  // 跳过按钮
    public List<ImageWithSubtitles> imageSequence; // 图片和字幕序列
    
    public float transitionDuration = 0.5f;     // 过渡动画持续时间
    public Color transitionColor = Color.black; // 过渡颜色（全黑）

    private int _currentIndex;                 // 当前显示的索引
    private Color _originalImageColor;         // 图片原始颜色
    private Color _originalTextColor;          // 原始文本颜色
    private bool _isTransitioning;             // 是否正在过渡
    private bool _isSkipping;                  // 是否正在跳过
    private readonly List<Coroutine> _activeCoroutines = new List<Coroutine>(); // 跟踪活跃的协程

    public bool isEnd;
    
    void Start()
    {
        // 初始化组件引用
        if (imageComponent == null)
            imageComponent = GetComponent<Image>();
        
        if (subtitleText == null)
            Debug.LogError("请指定字幕Text组件！");

        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked); // 绑定按钮点击事件
        
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
        
        // 开始显示第一张图片
        ShowCurrentImageAndSubtitles();
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
        
        // 开始逐行显示字幕并并跟踪这个协程
        Coroutine coroutine = StartCoroutine(ShowSubtitlesLineByLine());
        _activeCoroutines.Add(coroutine);
    }

    // 逐行显示字幕
    private IEnumerator ShowSubtitlesLineByLine()
    {
        var currentData = imageSequence[_currentIndex];
        string[] lines = currentData.subtitles.Split('\n');
        
        foreach (string line in lines)
        {
            // 检查是否需要跳过
            if (_isSkipping) break;
            
            for (int i = 0; i <= line.Length; i++)
            {
                if (_isSkipping) break;
                subtitleText.text = line.Substring(0, i);
                yield return new WaitForSeconds(currentData.textSpeed);
            }
            if (_isSkipping) break;
            yield return new WaitForSeconds(0.5f); // 行间隔
        }

        // 如果正在跳过，直接结束
        if (_isSkipping)
        {
            if (isEnd)
            {
                yield break;
            }
            StartCoroutine(FadeOutAndHide());
            yield break;
        }
        
        // 所有字幕显示完毕后等待
        yield return new WaitForSeconds(1f);
        
        if (_currentIndex == imageSequence.Count - 1 && isEnd)
        {
        }
        else
        {
            StartCoroutine(_currentIndex == imageSequence.Count - 1 ? FadeOutAndHide() : TransitionToNext());
        }
    }

    // 过渡到下一张图片
    private IEnumerator TransitionToNext()
    {
        if (_isTransitioning || _isSkipping) yield break;
        _isTransitioning = true;

        // 渐变为过渡色（全黑）
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration && !_isSkipping)
        {
            float t = elapsedTime / transitionDuration;
            imageComponent.color = Color.Lerp(_originalImageColor, transitionColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (_isSkipping)
        {
            if (isEnd)
            {
                yield break;
            }
            StartCoroutine(FadeOutAndHide());
            yield break;
        }
        
        imageComponent.color = transitionColor;

        // 更新索引
        _currentIndex++;

        // 从过渡色渐变回来
        elapsedTime = 0f;
        while (elapsedTime < transitionDuration && !_isSkipping)
        {
            float t = elapsedTime / transitionDuration;
            imageComponent.color = Color.Lerp(transitionColor, _originalImageColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (_isSkipping)
        {
            if (isEnd)
            {
                yield break;
            }
            StartCoroutine(FadeOutAndHide());
            yield break;
        }
        
        imageComponent.color = _originalImageColor;

        // 显示新图片的字幕
        ShowCurrentImageAndSubtitles();
        
        _isTransitioning = false;
    }

    // 最后一张图片播放完后淡出并隐藏
    private IEnumerator FadeOutAndHide()
    {
        if (isEnd)
        {
            yield break; 
        }
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
        skipButton.gameObject.SetActive(false);
        
        if (dialoguePanel != null)
            dialoguePanel.gameObject.SetActive(true);
    }
    
    // 跳过按钮点击事件
    private void OnSkipClicked()
    {
        if (_isSkipping) return;
        
        _isSkipping = true;
        StopAllActiveCoroutines();
        if (isEnd)
        {
            return; 
        }
        StartCoroutine(FadeOutAndHide());
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
