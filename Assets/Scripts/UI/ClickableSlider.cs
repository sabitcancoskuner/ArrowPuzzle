using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickableSlider : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;

    private Slider slider;
    private Image background;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        background = GetComponentInChildren<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ChangeSliderValue();
        ChangeSliderColor();
    }

    private void ChangeSliderValue()
    {
        if (slider.value == 1)
        {
            slider.value = 0f;
        }

        else
        {
            slider.value = 1f;
        }
    }

    private void ChangeSliderColor()
    {
        if (slider.value == 0) background.color = offColor;
        else if (slider.value == 1) background.color = onColor;
    }
}
