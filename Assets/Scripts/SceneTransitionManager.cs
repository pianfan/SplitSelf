using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("过渡效果设置")]
    public Image fadeImage; // 用于全白过渡的图片（需在Inspector赋值）
    public float fadeDuration = 0.5f; // 过渡动画时长（秒）

    private void Awake()
    {
        // 初始化过渡图片状态（默认全透明，隐藏）
        if (fadeImage != null)
        {
            fadeImage.color = new Color(1, 1, 1, 0); // 全白但透明
            fadeImage.raycastTarget = false; // 不阻挡点击
        }
    }

    /// <summary>
    /// 切换到目标场景（带全白过渡效果）
    /// </summary>
    /// <param name="sceneName">目标场景名称（需在Build Settings中添加）</param>
    public void TransitionToScene(string sceneName)
    {
        if (fadeImage == null)
        {
            Debug.LogError("请在SceneTransitionManager中赋值fadeImage！");
            return;
        }

        // 开始过渡协程
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    /// <summary>
    /// 协程：先淡出（全白），再加载场景，最后淡入（透明）
    /// </summary>
    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        // 第一步：画面渐变为全白
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration); // 从透明到不透明
            fadeImage.color = new Color(1, 1, 1, alpha);
            fadeImage.raycastTarget = true; // 过渡时阻挡玩家操作
            yield return null;
        }

        // 确保完全变白
        fadeImage.color = Color.white;

        // 第二步：加载目标场景
        yield return SceneManager.LoadSceneAsync(sceneName);

        // 第三步：画面从全白渐变为透明
        elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration); // 从不透明到透明
            fadeImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        // 确保完全透明，且不阻挡操作
        fadeImage.color = new Color(1, 1, 1, 0);
        fadeImage.raycastTarget = false;
    }
}
