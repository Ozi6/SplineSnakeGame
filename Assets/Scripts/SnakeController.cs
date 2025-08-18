using UnityEngine;
using Dreamteck.Splines;
using System.Collections.Generic;

public class SnakeController : MonoBehaviour
{
    public static SnakeController Instance { get; private set; }
    public Transform head;
    public float moveSpeed = 5f;
    public float segmentSpacing = 1f;
    public int initialLength = 5;
    public float shootInterval = 0.5f;
    public GameObject projectilePrefab;

    private SplineComputer spline;
    private int currentLength;
    private List<Vector3> trailPositions;
    private float distanceTraveled = 0f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        spline = GetComponent<SplineComputer>();
        currentLength = initialLength;
        trailPositions = new List<Vector3>();
        trailPositions.Add(head.position);
        UpdateSpline();
        InvokeRepeating("Shoot", 0f, shootInterval);
    }

    void Update()
    {
        Vector3 previousHeadPosition = head.position;
        head.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        float frameDist = Vector3.Distance(previousHeadPosition, head.position);
        distanceTraveled += frameDist;
        if (distanceTraveled >= segmentSpacing)
        {
            trailPositions.Insert(0, head.position);
            distanceTraveled = 0f;
            while (trailPositions.Count > currentLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            UpdateSpline();
        }
        else
        {
            if (trailPositions.Count > 0)
            {
                trailPositions[0] = head.position;
                UpdateSpline();
            }
        }
    }

    void UpdateSpline()
    {
        if (trailPositions.Count < 2) return;

        SplinePoint[] splinePoints = new SplinePoint[trailPositions.Count];
        for (int i = 0; i < trailPositions.Count; i++)
        {
            splinePoints[i] = new SplinePoint
            {
                position = trailPositions[i],
                normal = Vector3.up,
                size = 1f,
                color = Color.white
            };
        }

        spline.SetPoints(splinePoints);
        spline.RebuildImmediate();
    }

    public void ChangeLength(int delta)
    {
        currentLength += delta;
        currentLength = Mathf.Max(currentLength, 1);
        while (trailPositions.Count > currentLength)
            trailPositions.RemoveAt(trailPositions.Count - 1);
        UpdateSpline();
    }

    void Shoot()
    {
        GameObject proj = Instantiate(projectilePrefab, head.position + head.forward * 0.5f, head.rotation);
        proj.GetComponent<Rigidbody>().linearVelocity = head.forward * 20f;
        Destroy(proj, 5f);
    }
}