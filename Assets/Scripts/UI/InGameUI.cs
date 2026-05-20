using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1)]
public class InGameUI : MonoBehaviour
{
    [Header("Difficulty Text")]
    [SerializeField] private TextMeshProUGUI textObject;
    [SerializeField] private Color easyColor;
    [SerializeField] private Color mediumColor;
    [SerializeField] private Color hardColor;
    [SerializeField] private Color expertColor;

    [Header("Hearts")]
    [SerializeField] private List<Image> hearts;

    [Header("Guideline Buttons")]
    [SerializeField] private GameObject openGuideLineButton;
    [SerializeField] private GameObject closeGuideLineButton;

    [Header("Vignette Flash")]
    [SerializeField] private Image flashImage;
    [SerializeField] private int targetAlpha = 80;
    [SerializeField] private float flashDuration = 0.08f;
    private Sequence flashSeq;

    private void OnEnable()
    {
        LevelManager.Instance.OnLevelLoaded += SetDifficultyText;

        BoardManager.Instance.OnWrongMove += DecreaseHeart;
        BoardManager.Instance.OnWrongMove += Flash;
        BoardManager.Instance.OnShowGuideLine += SetGuidelineButtonsStatus;
    }

    private void OnDisable()
    {
        LevelManager.Instance.OnLevelLoaded -= SetDifficultyText;

        BoardManager.Instance.OnWrongMove -= DecreaseHeart;
        BoardManager.Instance.OnWrongMove -= Flash;
        BoardManager.Instance.OnShowGuideLine -= SetGuidelineButtonsStatus;
    }

    private void SetDifficultyText(LevelData data)
    {
        LevelDifficulty difficulty = data.difficulty;

        textObject.text = difficulty.ToString();
        textObject.color = GetTextColor(difficulty);
    }

    private void SetGuidelineButtonsStatus(bool show)
    {
        if (show)
        {
            openGuideLineButton.SetActive(false);
            closeGuideLineButton.SetActive(true);
        }
        else
        {
            openGuideLineButton.SetActive(true);
            closeGuideLineButton.SetActive(false);
        }
    }

    private Color GetTextColor(LevelDifficulty difficulty)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Easy:
                return easyColor;

            case LevelDifficulty.Medium:
                return mediumColor;

            case LevelDifficulty.Hard:
                return hardColor;

            case LevelDifficulty.Expert:
                return expertColor;

            default:
                return Color.white;
        }
    }

    private void Flash()
    {
        if (flashSeq.isAlive)
            flashSeq.Stop();

        flashImage.gameObject.SetActive(true);

        float target = targetAlpha / 255f;
        flashSeq = Sequence.Create()
            .Group(Tween.Alpha(flashImage, target, flashDuration, Ease.InQuad))
            .Chain(Tween.Alpha(flashImage, 0, flashDuration, Ease.InQuad))
            .OnComplete(() =>
            {
               flashImage.gameObject.SetActive(false); 
            });
    }

    private void DecreaseHeart()
    {
        Image heart = hearts.Last();
        heart.gameObject.SetActive(false);
        hearts.Remove(heart);
    }
}
