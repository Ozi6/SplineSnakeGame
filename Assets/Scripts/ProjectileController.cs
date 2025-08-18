using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float maxLifetime = 5f;
    private float lifeTime = 0f;

    private void OnCollisionEnter(Collision collision)
    {
        GateController gate = collision.gameObject.GetComponentInParent<GateController>();
        if (gate != null)
            gate.OnShot();
        Destroy(gameObject);
    }

    void Update()
    {
        lifeTime += Time.deltaTime;
        if (lifeTime >= maxLifetime)
            Destroy(gameObject);
    }
}