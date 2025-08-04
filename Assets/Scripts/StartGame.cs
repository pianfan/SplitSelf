using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public GameObject transitionImage; // Ҫ��ʾ��ͼƬ
    public float waitTime = 1f;        // �ȴ�ʱ��
    public GameObject mainPanel;
    public GameObject otherPanel;

    public void OnStartButtonClick()
    {
        StartCoroutine(PlayStartSequence());
    }

    IEnumerator PlayStartSequence()
    {
        // ����ͼƬ
        if (transitionImage != null)
        {
            mainPanel.SetActive(false);
            otherPanel.SetActive(false);
            transitionImage.SetActive(true);
        }

        // �ȴ�
        yield return new WaitForSeconds(waitTime);

        // ������һ������
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
