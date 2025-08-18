using UnityEngine;
using TMPro;

public class GateController : MonoBehaviour
{
    public int value = 5;
    public TextMeshPro valueText;
    public MeshRenderer gateRenderer;
    public Color positiveColor = new Color(0f, 1f, 0f, 0.5f);
    public Color negativeColor = new Color(1f, 0f, 0f, 0.5f);

    void Start()
    {
        UpdateDisplay();
    }

    void OnEnable()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (valueText != null)
            valueText.text = (value >= 0 ? "+" : "") + value.ToString();
        if (gateRenderer != null)
            gateRenderer.material.color = value >= 0 ? positiveColor : negativeColor;
    }

    public void OnShot()
    {
        value++;
        UpdateDisplay();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == SnakeController.Instance.head)
        {
            SnakeController.Instance.ChangeLength(value);
            if (LevelGenerator.Instance != null)
                gameObject.SetActive(false);
            else
                Destroy(gameObject);
        }
    }
}