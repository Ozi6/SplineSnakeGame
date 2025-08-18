using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance { get; private set; }
    public GameObject platformSegmentPrefab;
    public GameObject gatePrefab;
    public Transform snakeHead;
    public float segmentLength = 50f;
    public float spawnAheadDistance = 100f;
    public float destroyBehindDistance = 100f;
    public int minGatesPerSegment = 0;
    public int maxGatesPerSegment = 3;
    public int minGateValue = -10;
    public int maxGateValue = 10;
    private float nextSpawnZ = 0f;
    private ObjectPool<GameObject> segmentPool;
    private ObjectPool<GameObject> gatePool;
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
        gatePool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(gatePrefab),
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
                gatePool.Release(gate);
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
            GameObject gate = gatePool.Get();
            float gateZ = zPos + Random.Range(10f, segmentLength - 10f);
            float gateX = Random.Range(-3f, 3f);
            float gateY = 2f;
            gate.transform.position = new Vector3(gateX, gateY, gateZ);
            activeGates.Add(gate);
            GateController gateCtrl = gate.GetComponent<GateController>();
            gateCtrl.value = Random.Range(minGateValue, maxGateValue + 1);
            gateCtrl.UpdateDisplay();
        }
    }
}