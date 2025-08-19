using UnityEngine;
using TMPro;

public abstract class BaseGate : MonoBehaviour
{
    public TextMeshPro valueText;
    public MeshRenderer gateRenderer;
    public float colorTransitionSpeed = 5f;

    protected Color targetColor;

    void Start()
    {
        UpdateDisplay();
    }

    void OnEnable()
    {
        UpdateDisplay();
    }

    void Update()
    {
        if (gateRenderer != null)
        {
            gateRenderer.material.color = Color.Lerp(
                gateRenderer.material.color,
                targetColor,
                Time.deltaTime * colorTransitionSpeed
            );
        }
    }

    public virtual void UpdateDisplay() { }

    public virtual void OnShot() { }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == SnakeSplineController.Instance.head)
        {
            ApplyEffect();
            if (LevelGenerator.Instance != null)
                gameObject.SetActive(false);
            else
                Destroy(gameObject);
        }
    }

    protected abstract void ApplyEffect();
}