using TMPro;
using UnityEngine;

public class PlayMenuUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentLevelText;

    private void OnEnable()
    {
        if (PlayerPrefs.HasKey("CurrentLevel"))
        {
            currentLevelText.text = PlayerPrefs.GetInt("CurrentLevel").ToString();
        }
    }
}
