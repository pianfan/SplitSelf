using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public GameObject transitionImage; // 要显示的图片
    public float waitTime = 1f;        // 等待时间
    public GameObject mainPanel;
    public GameObject otherPanel;

    public void OnStartButtonClick()
    {
        StartCoroutine(PlayStartSequence());
    }

    IEnumerator PlayStartSequence()
    {
        // 播放图片
        if (transitionImage != null)
        {
            mainPanel.SetActive(false);
            otherPanel.SetActive(false);
            transitionImage.SetActive(true);
        }

        // 等待
        yield return new WaitForSeconds(waitTime);

        // 加载下一个场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
