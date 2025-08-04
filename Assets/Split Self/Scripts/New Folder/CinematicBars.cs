using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CinematicBars : MonoBehaviour
{
    public static CinematicBars instance;

    public RectTransform topBar;
    public RectTransform bottomBar;
    public float targetHeight = 75f;     // ���ڱ߸߶ȣ���λ�����أ��������ʼ��Ϊ2.5D�ĸ߶ȡ�2D����35f
    public float transitionSpeed = 75f;  // ��λ����/�룬2D����35f

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
        // ƽ�����ɵ�Ŀ��߶�
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

    // ���ã�չ���ڱ�
    public void ShowBars()
    {
        goalHeight = targetHeight;
    }

    // ���ã�����ڱ�
    public void HideBars()
    {
        goalHeight = 0f;
    }

    // ���ã��趨����߶ȣ����ھ籾����ʱ�
    public void SetBars(float height)
    {
        goalHeight = Mathf.Clamp(height, 0f, targetHeight);
    }
}
