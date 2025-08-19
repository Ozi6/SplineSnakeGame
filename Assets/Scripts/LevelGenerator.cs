using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance { get; private set; }
    public GameObject platformSegmentPrefab;
    public GameObject lengthGatePrefab;
    public GameObject colorGatePrefab;
    public Transform snakeHead;
    public float segmentLength = 50f;
    public float spawnAheadDistance = 100f;
    public float destroyBehindDistance = 100f;
    public int minGatesPerSegment = 0;
    public int maxGatesPerSegment = 3;
    public int minGateValue = -10;
    public int maxGateValue = 10;
    public float colorGateChance = 0.2f;
    private float nextSpawnZ = 0f;
    private ObjectPool<GameObject> segmentPool;
    private ObjectPool<GameObject> lengthGatePool;
    private ObjectPool<GameObject> colorGatePool;
    private List<GameObject> activeSegments = new List<GameObject>();
    private List<GameObject> activeGates = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        segmentPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(platformSegmentPrefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: Destroy,
            maxSize: 10
        );
        lengthGatePool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(lengthGatePrefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: Destroy,
            maxSize: 50
        );
        colorGatePool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(colorGatePrefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: Destroy,
            maxSize: 50
        );
    }
    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnSegment(nextSpawnZ);
            nextSpawnZ += segmentLength;
        }
    }
    void Update()
    {
        float snakeZ = snakeHead.position.z;
        if (snakeZ + spawnAheadDistance > nextSpawnZ - segmentLength)
        {
            SpawnSegment(nextSpawnZ);
            nextSpawnZ += segmentLength;
        }
        for (int i = activeSegments.Count - 1; i >= 0; i--)
        {
            GameObject seg = activeSegments[i];
            if (seg.transform.position.z + segmentLength < snakeZ - destroyBehindDistance)
            {
                segmentPool.Release(seg);
                activeSegments.RemoveAt(i);
            }
        }
        for (int i = activeGates.Count - 1; i >= 0; i--)
        {
            GameObject gate = activeGates[i];
            if (gate != null && gate.transform.position.z < snakeZ - destroyBehindDistance)
            {
                if (gate.GetComponent<LengthGateController>() != null)
                {
                    lengthGatePool.Release(gate);
                }
                else if (gate.GetComponent<ColorGateController>() != null)
                {
                    colorGatePool.Release(gate);
                }
                activeGates.RemoveAt(i);
            }
        }
    }
    void SpawnSegment(float zPos)
    {
        GameObject segment = segmentPool.Get();
        segment.transform.position = new Vector3(0, 0, zPos);
        activeSegments.Add(segment);
        int gateCount = Random.Range(minGatesPerSegment, maxGatesPerSegment + 1);
        for (int i = 0; i < gateCount; i++)
        {
            bool isColorGate = Random.value < colorGateChance;
            GameObject gate = isColorGate ? colorGatePool.Get() : lengthGatePool.Get();
            float gateZ = zPos + Random.Range(10f, segmentLength - 10f);
            float gateX = Random.Range(-3f, 3f);
            float gateY = 2f;
            gate.transform.position = new Vector3(gateX, gateY, gateZ);
            activeGates.Add(gate);
            if (isColorGate)
            {
                ColorGateController gateCtrl = gate.GetComponent<ColorGateController>();
                gateCtrl.gateColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
                gateCtrl.UpdateDisplay();
            }
            else
            {
                LengthGateController gateCtrl = gate.GetComponent<LengthGateController>();
                gateCtrl.value = Random.Range(minGateValue, maxGateValue + 1);
                gateCtrl.UpdateDisplay();
            }
        }
    }
}