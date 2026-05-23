using UnityEngine;
using UnityEngine.UI;

public class SettingsIcon : MonoBehaviour
{
    [Header("Icon and Accent")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconAccent;

    [Header("Icon Settings")]
    [SerializeField] private Color iconOnColor;
    [Space]
    [SerializeField] private Color iconOffColor;

    public void ChangeIconColor(float value)
    {
        if (value == 1f) SliderSwitchOn();
        else if (value == 0f) SliderSwitchOff();
    }
    
    private void SliderSwitchOn()
    {
        iconImage.color = iconOnColor;
        iconAccent.color = new Color(iconOnColor.r, iconOnColor.g, iconOnColor.b, 40f/255f);
    }

    private void SliderSwitchOff()
    {
        iconImage.color = iconOffColor;
        iconAccent.color = new Color(iconOffColor.r, iconOffColor.g, iconOffColor.b, 40f/255f);
    }
}
