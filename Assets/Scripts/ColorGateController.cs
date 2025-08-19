using UnityEngine;
using TMPro;

public class ColorGateController : BaseGate
{
    public Color gateColor = Color.white;

    public override void UpdateDisplay()
    {
        if (valueText != null)
            valueText.text = "Color";
        if (gateRenderer != null)
            targetColor = new Color(gateColor.r, gateColor.g, gateColor.b, 0.5f);
    }

    protected override void ApplyEffect()
    {
        SnakeSplineController.Instance.ChangeColor(gateColor);
    }
}