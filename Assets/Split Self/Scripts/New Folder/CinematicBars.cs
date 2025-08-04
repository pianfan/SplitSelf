using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CinematicBars : MonoBehaviour
{
    public static CinematicBars instance;

    public RectTransform topBar;
    public RectTransform bottomBar;
    public float targetHeight = 75f;     // 最大黑边高度（单位：像素），这里初始化为2.5D的高度。2D的是35f
    public float transitionSpeed = 75f;  // 单位像素/秒，2D的是35f

    private float currentHeight = 0f;
    private float goalHeight = 0f;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 平滑过渡到目标高度
        if (Mathf.Abs(currentHeight - goalHeight) > 1f)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, goalHeight, transitionSpeed * Time.deltaTime);
            SetBarHeight(currentHeight);
        }
    }

    void SetBarHeight(float height)
    {
        topBar.sizeDelta = new Vector2(0f, height);
        bottomBar.sizeDelta = new Vector2(0f, height);
    }

    // 调用：展开黑边
    public void ShowBars()
    {
        goalHeight = targetHeight;
    }

    // 调用：收起黑边
    public void HideBars()
    {
        goalHeight = 0f;
    }

    // 调用：设定具体高度（用于剧本控制时差）
    public void SetBars(float height)
    {
        goalHeight = Mathf.Clamp(height, 0f, targetHeight);
    }
}
