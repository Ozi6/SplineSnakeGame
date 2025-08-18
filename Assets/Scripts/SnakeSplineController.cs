using UnityEngine;
using Dreamteck.Splines;

public class SnakeSplineController : MonoBehaviour
{
    public static SnakeSplineController Instance { get; private set; }
    public Transform head;
    public float snakeLength = 5f;
    public float minAddDistance = 0.5f;
    private SplineComputer spline;
    private SplineMesh splineMesh;
    private Vector3 lastAddedPosition;
    public float shootInterval = 0.5f;
    public GameObject projectilePrefab;

    void Start()
    {
        Instance = this;
        spline = GetComponent<SplineComputer>();
        splineMesh = GetComponent<SplineMesh>();
        splineMesh.multithreaded = false;

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

        TrimSpline();
    }

    private SplinePoint CreateSplinePoint(Vector3 position)
    {
        SplinePoint point = new SplinePoint(position);
        point.size = 1f;
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

    private void TrimSpline()
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

        splineMesh.clipFrom = 0f;
        splineMesh.clipTo = 1f;
        splineMesh.RebuildImmediate();
    }

    public void Grow(float amount)
    {
        snakeLength += amount;
    }

    void Shoot()
    {
        GameObject proj = Instantiate(projectilePrefab, head.position + head.forward * 0.5f, head.rotation);
        proj.GetComponent<Rigidbody>().linearVelocity = head.forward * 20f;
        Destroy(proj, 5f);
    }
}