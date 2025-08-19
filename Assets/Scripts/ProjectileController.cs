using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float maxLifetime = 5f;
    private float lifeTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        LengthGateController gate = other.GetComponentInParent<LengthGateController>();
        if (gate != null)
        {
            gate.OnShot();
            Destroy(gameObject);
        }
    }

    void Update()
    {
        lifeTime += Time.deltaTime;
        if (lifeTime >= maxLifetime)
            Destroy(gameObject);
    }
}