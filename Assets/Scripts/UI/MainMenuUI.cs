using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    private GameObject currentActiveUI;

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelSelection;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject infoMenu;

    private void Awake()
    {
        currentActiveUI = mainMenu;
    }

    public void LoadUI(GameObject ui)
    {
        if (currentActiveUI != null) currentActiveUI.SetActive(false);
        currentActiveUI = ui;
        currentActiveUI.SetActive(true);
    }
}
