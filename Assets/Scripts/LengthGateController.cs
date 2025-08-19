using UnityEngine;
using TMPro;

public class LengthGateController : BaseGate
{
    public int value = 5;
    public Color positiveColor = new Color(0f, 1f, 0f, 0.5f);
    public Color negativeColor = new Color(1f, 0f, 0f, 0.5f);

    public override void UpdateDisplay()
    {
        if (valueText != null)
            valueText.text = (value >= 0 ? "+" : "") + value.ToString();
        if (gateRenderer != null)
            targetColor = value >= 0 ? positiveColor : negativeColor;
    }

    public override void OnShot()
    {
        value++;
        UpdateDisplay();
    }

    protected override void ApplyEffect()
    {
        SnakeSplineController.Instance.Grow(value);
    }
}