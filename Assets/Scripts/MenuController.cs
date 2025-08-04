using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject imagePanel;

    public void ShowImagePanel()
    {
        mainPanel.SetActive(false);
        imagePanel.SetActive(true);
    }

    public void ReturnToMain()
    {
        imagePanel.SetActive(false);
        mainPanel.SetActive(true);
    }
}
