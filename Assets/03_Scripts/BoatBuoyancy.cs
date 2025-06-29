using UnityEngine;

public class BoatBuoyancy : MonoBehaviour
{
    public Rigidbody rb;
    public Transform[] floatPoints;
    public float buoyancyForce = 10f;
    public float waterDrag = 0.1f;
    public float waterAngularDrag = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        foreach (var point in floatPoints)
        {
            ApplyBuoyancy(point);
        }
    }

    void ApplyBuoyancy(Transform point)
    {
        Vector3 worldPoint = point.position;
        float waveY = GetWaveHeight(worldPoint.x, worldPoint.z, Time.time);

        if (worldPoint.y < waveY)
        {
            float depth = waveY - worldPoint.y;
            Vector3 uplift = Vector3.up * buoyancyForce * depth;
            rb.AddForceAtPosition(uplift, worldPoint);

            // 물 저항 적용
            rb.AddForce(-rb.velocity * waterDrag * Time.fixedDeltaTime);
            rb.AddTorque(-rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime);
        }
    }

    float GetWaveHeight(float x, float z, float time)
    {
        float waveHeight = Mathf.Sin((x + time) * 0.5f) * 0.5f
                          + Mathf.Sin((z + time * 1.2f) * 0.3f) * 0.3f;
        return waveHeight;
    }
}
