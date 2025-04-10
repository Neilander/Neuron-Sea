using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 关卡完成面板 - 显示关卡完成后的分数和评级
/// </summary>
public class LevelCompletePanel : MonoBehaviour, ILevelCompletePanel
{
    [Header("UI引用")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text collectiblesText;

    [Header("评级显示")]
    [SerializeField] private GameObject[] stars; // 星级评分的星星图标
    [SerializeField] private Image rankImage;    // 评级图标
    [SerializeField] private Sprite[] rankSprites; // 不同等级的图标：S, A, B, C, D

    [Header("动画设置")]
    [SerializeField] private float scoreCountDuration = 1.5f; // 分数计数动画持续时间
    [SerializeField] private float starShowDelay = 0.5f;      // 星星显示延迟

    private int currentScore;
    private Coroutine scoreAnimationCoroutine;

    /// <summary>
    /// 设置关卡得分并显示
    /// </summary>
    /// <param name="score">关卡得分</param>
    public void SetScore(int score)
    {
        currentScore = score;

        // 停止之前可能正在运行的动画
        if (scoreAnimationCoroutine != null)
            StopCoroutine(scoreAnimationCoroutine);

        // 开始新的动画
        scoreAnimationCoroutine = StartCoroutine(AnimateScoreAndRank(score));
    }

    /// <summary>
    /// 设置关卡详细数据
    /// </summary>
    public void SetLevelDetails(float completionTime, int collectiblesCollected, int collectiblesTotal)
    {
        if (timeText != null)
        {
            System.TimeSpan time = System.TimeSpan.FromSeconds(completionTime);
            timeText.text = string.Format("{0:D2}:{1:D2}.{2:D2}",
                time.Minutes, time.Seconds, (int)(time.Milliseconds / 10));
        }

        if (collectiblesText != null)
        {
            collectiblesText.text = string.Format("{0}/{1}", collectiblesCollected, collectiblesTotal);
        }
    }

    /// <summary>
    /// 分数和评级动画
    /// </summary>
    private IEnumerator AnimateScoreAndRank(int score)
    {
        // 隐藏所有星星
        if (stars != null)
        {
            foreach (var star in stars)
            {
                if (star != null)
                    star.SetActive(false);
            }
        }

        // 分数从0开始递增
        float startTime = Time.time;
        float endTime = startTime + scoreCountDuration;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / scoreCountDuration;
            int currentCountingScore = Mathf.FloorToInt(Mathf.Lerp(0, score, t));

            if (scoreText != null)
                scoreText.text = currentCountingScore.ToString();

            yield return null;
        }

        // 确保最终显示正确的得分
        if (scoreText != null)
            scoreText.text = score.ToString();

        // 根据分数显示评级
        int rank = GetRankFromScore(score);
        ShowRank(rank);

        // 显示星星动画
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (i < rank && stars[i] != null)
                {
                    yield return new WaitForSeconds(starShowDelay);
                    stars[i].SetActive(true);

                    // 可以在这里添加星星激活的动画效果
                    stars[i].transform.localScale = Vector3.zero;
                    LeanTween.scale(stars[i], Vector3.one, 0.3f).setEaseOutBack();
                }
            }
        }
    }

    /// <summary>
    /// 根据分数计算评级
    /// </summary>
    private int GetRankFromScore(int score)
    {
        // 根据分数范围返回等级 (0-5)
        if (score >= 9000) return 5; // S
        if (score >= 7500) return 4; // A
        if (score >= 6000) return 3; // B
        if (score >= 4500) return 2; // C
        if (score >= 3000) return 1; // D
        return 0; // F
    }

    /// <summary>
    /// 显示评级
    /// </summary>
    private void ShowRank(int rank)
    {
        if (rankText != null)
        {
            string rankString = "F";
            switch (rank)
            {
                case 5: rankString = "S"; break;
                case 4: rankString = "A"; break;
                case 3: rankString = "B"; break;
                case 2: rankString = "C"; break;
                case 1: rankString = "D"; break;
            }

            rankText.text = rankString;
        }

        // 显示对应的评级图标
        if (rankImage != null && rankSprites != null && rank < rankSprites.Length && rank >= 0)
        {
            rankImage.sprite = rankSprites[rank];
        }
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    public void ContinueGame()
    {
        // 这里可以实现继续到下一关的逻辑
        ClosePanel();
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        // 这里可以实现返回主菜单的逻辑
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// 重新开始当前关卡
    /// </summary>
    public void RestartLevel()
    {
        // 这里可以实现重新开始关卡的逻辑
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}