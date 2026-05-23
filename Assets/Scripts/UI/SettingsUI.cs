using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [Header("Icon Settings")]
    [SerializeField] private Color iconOnColor;
    [SerializeField] private Color iconOnAccentColor;
    [Space]
    [SerializeField] private Color iconOffColor;
    [SerializeField] private Color iconOffAccentColor;

    public void ChangeIconColor(int value)
    {
        if (value == 1)
        {
            
        }
    }
}
