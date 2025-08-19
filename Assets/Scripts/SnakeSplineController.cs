using UnityEngine;
using Dreamteck.Splines;

public class SnakeSplineController : MonoBehaviour
{
    public static SnakeSplineController Instance { get; private set; }

    [Header("Snake Settings")]
    public Transform head;
    public float snakeLength = 5f;
    public float minAddDistance = 0.5f;

    [Header("Wave Effects")]
    public float waveAmplitude = 0.3f;
    public float waveFrequency = 2f;
    public float waveDecayRate = 2f;

    [Header("Growth Effects")]
    public GameObject growthEffectPrefab;
    public float growthEffectSpeed = 10f;
    public float growthEffectScale = 2f;

    [Header("Shooting")]
    public float shootInterval = 0.5f;
    public GameObject projectilePrefab;

    [Header("Color Settings")]
    public Color initialColor = Color.white;
    public float colorTransitionSpeed = 2f;

    private Color currentColor;
    private Color targetColor;
    private bool isTransitioningColor = false;
    private float colorTransitionTime = 0f;

    private SplineComputer spline;
    private SplineMesh splineMesh;
    private Vector3 lastAddedPosition;
    private float waveTime = 0f;
    private bool isWaving = false;
    private float lastStrafeDirection = 0f;
    private float strafeStopTime = 0f;

    private Renderer headRenderer;

    void Start()
    {
        Instance = this;
        currentColor = initialColor;
        targetColor = initialColor;
        spline = GetComponent<SplineComputer>();
        splineMesh = GetComponent<SplineMesh>();
        splineMesh.multithreaded = false;

        headRenderer = head.GetComponent<Renderer>();

        SplinePoint initialPoint = CreateSplinePoint(head.position);
        spline.SetPoints(new SplinePoint[] { initialPoint });
        lastAddedPosition = head.position;

        InvokeRepeating("Shoot", 0f, shootInterval);
    }

    void Update()
    {
        if (Vector3.Distance(head.position, lastAddedPosition) >= minAddDistance)
        {
            AddPoint(head.position);
            lastAddedPosition = head.position;
        }
        UpdateWaveEffect();
        UpdateColorTransition();
        TrimSplineToLength();
    }

    private void UpdateColorTransition()
    {
        if (isTransitioningColor)
        {
            colorTransitionTime += Time.deltaTime * colorTransitionSpeed;
            ApplyGradualColorChange();
            if (colorTransitionTime >= 1f)
            {
                isTransitioningColor = false;
                colorTransitionTime = 0f;
                currentColor = targetColor;
            }
        }
    }

    private void ApplyGradualColorChange()
    {
        SplinePoint[] points = spline.GetPoints();
        if (points.Length == 0) return;
        float waveProgress = colorTransitionTime;
        for (int i = 0; i < points.Length; i++)
        {
            float normalizedPosition = (float)(points.Length - 1 - i) / Mathf.Max(1, points.Length - 1);
            Color pointColor;
            if (normalizedPosition <= waveProgress)
                pointColor = targetColor;
            else
                pointColor = currentColor;
            points[i].color = pointColor;
        }
        spline.SetPoints(points);
        if (headRenderer != null)
            if (colorTransitionTime > 0f)
                headRenderer.material.color = targetColor;
    }

    private void UpdateWaveEffect()
    {
        if (isWaving)
        {
            waveTime += Time.deltaTime;
            ApplyWaveToSpline();
            if (waveTime > 3f / waveDecayRate)
            {
                isWaving = false;
                waveTime = 0f;
            }
        }
    }

    private void ApplyWaveToSpline()
    {
        SplinePoint[] points = spline.GetPoints();
        for (int i = 0; i < points.Length; i++)
        {
            float normalizedPosition = (float)i / (points.Length - 1);
            float waveOffset = Mathf.Sin(normalizedPosition * waveFrequency + waveTime * 3f) *
                              waveAmplitude * lastStrafeDirection *
                              Mathf.Exp(-waveTime * waveDecayRate);
            Vector3 rightDirection = Vector3.Cross(Vector3.up, head.forward).normalized;
            points[i].position += rightDirection * waveOffset * Time.deltaTime;
        }

        spline.SetPoints(points);
    }

    public void OnStrafeStop(float strafeDirection)
    {
        lastStrafeDirection = strafeDirection;
        isWaving = true;
        waveTime = 0f;
        strafeStopTime = Time.time;
    }

    private SplinePoint CreateSplinePoint(Vector3 position)
    {
        SplinePoint point = new SplinePoint(position);
        point.size = 1f;
        point.color = currentColor;
        return point;
    }

    private void AddPoint(Vector3 position)
    {
        SplinePoint newPoint = CreateSplinePoint(position);
        SplinePoint[] currentPoints = spline.GetPoints();
        SplinePoint[] newPoints = new SplinePoint[currentPoints.Length + 1];
        System.Array.Copy(currentPoints, newPoints, currentPoints.Length);
        newPoints[currentPoints.Length] = newPoint;
        spline.SetPoints(newPoints);
    }

    private void TrimSplineToLength()
    {
        float totalLength = spline.CalculateLength();
        if (totalLength > snakeLength)
        {
            float excessLength = totalLength - snakeLength;
            SplinePoint[] currentPoints = spline.GetPoints();
            float accumulatedLength = 0f;
            int pointsToRemove = 0;
            for (int i = 0; i < currentPoints.Length - 1; i++)
            {
                float segmentLength = Vector3.Distance(currentPoints[i].position, currentPoints[i + 1].position);
                if (accumulatedLength + segmentLength < excessLength)
                {
                    accumulatedLength += segmentLength;
                    pointsToRemove++;
                }
                else
                    break;
            }
            if (pointsToRemove > 0 && currentPoints.Length > pointsToRemove + 1)
            {
                SplinePoint[] newPoints = new SplinePoint[currentPoints.Length - pointsToRemove];
                System.Array.Copy(currentPoints, pointsToRemove, newPoints, 0, newPoints.Length);
                spline.SetPoints(newPoints);
            }
        }
    }

    public void Grow(float amount)
    {
        float oldLength = snakeLength;
        snakeLength += amount;
        snakeLength = Mathf.Max(snakeLength, 1f);
        if (amount > 0 && growthEffectPrefab != null)
            StartCoroutine(CreateGrowthEffect());
        else if (amount < 0)
        {
            TrimSplineToLength();
            if (growthEffectPrefab != null)
                StartCoroutine(CreateShrinkEffect());
        }
    }

    public void ChangeColor(Color newColor)
    {
        if (newColor == currentColor && newColor == targetColor) return;

        targetColor = newColor;
        isTransitioningColor = true;
        colorTransitionTime = 0f;
    }

    private System.Collections.IEnumerator CreateGrowthEffect()
    {
        SplinePoint[] points = spline.GetPoints();
        if (points.Length < 2) yield break;
        Vector3 startPos = head.position;
        Vector3 endPos = points[0].position;
        GameObject effect = Instantiate(growthEffectPrefab, startPos, Quaternion.identity);
        effect.transform.localScale = Vector3.one * growthEffectScale;
        Renderer effectRenderer = effect.GetComponent<Renderer>();
        if (effectRenderer != null)
            effectRenderer.material.color = Color.yellow;
        float journey = 0f;
        float speed = growthEffectSpeed;

        while (journey <= 1f)
        {
            journey += Time.deltaTime * speed;
            SplinePoint[] currentPoints = spline.GetPoints();
            if (currentPoints.Length > 0)
                endPos = currentPoints[0].position - head.forward * 2f;
            effect.transform.position = Vector3.Lerp(startPos, endPos, journey);
            float scale = Mathf.Lerp(growthEffectScale, 0.1f, journey);
            effect.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        Destroy(effect);
    }

    private System.Collections.IEnumerator CreateShrinkEffect()
    {
        SplinePoint[] points = spline.GetPoints();
        if (points.Length < 2) yield break;
        Vector3 startPos = points[0].position;
        Vector3 endPos = points[points.Length - 1].position;
        GameObject effect = Instantiate(growthEffectPrefab, startPos, Quaternion.identity);
        effect.transform.localScale = Vector3.one * growthEffectScale;
        Renderer effectRenderer = effect.GetComponent<Renderer>();
        if (effectRenderer != null)
            effectRenderer.material.color = Color.red;
        float journey = 0f;
        float speed = growthEffectSpeed;
        while (journey <= 1f)
        {
            journey += Time.deltaTime * speed;
            effect.transform.position = Vector3.Lerp(startPos, endPos, journey);
            float scale = Mathf.Lerp(growthEffectScale, 0.1f, journey);
            effect.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        Destroy(effect);
    }

    void Shoot()
    {
        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, head.position + head.forward * 0.5f, head.rotation);
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = head.forward * 20f;
            Destroy(proj, 5f);
        }
    }
}