using UnityEngine;

public class BoatBuoyancy : MonoBehaviour
{
    [Header("Water Material (WaterWorks)")]
    public Material waterMaterial;

    [Header("Boat Settings")]
    public Rigidbody rb;
    public Transform[] floatPoints;

    [Header("Buoyancy Settings")]
    public float buoyancyForce = 15f;
    public float waterDrag = 0.5f;
    public float waterAngularDrag = 1.5f;

    // Wave parameters from Water Material
    private float waveSpeed;
    private float waveFrequency;
    private float waveDistance;

    [SerializeField]
    float waterLevel = 0.0f;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (waterMaterial == null) Debug.LogError("🚨 Water Material을 넣어주세요.");

        // Material에서 파도 정보 가져오기
        waveSpeed = waterMaterial.GetFloat("_WaveSpeed");
        waveFrequency = waterMaterial.GetFloat("_WaveFrequency");
        waveDistance = waterMaterial.GetFloat("_WaveDist");
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
        float waveY = GetWaveHeight(worldPoint.x, worldPoint.z, Time.time) + waterLevel;

        if (worldPoint.y < waveY)
        {
            float depth = waveY - worldPoint.y;
            Vector3 uplift = Vector3.up * buoyancyForce * depth;
            rb.AddForceAtPosition(uplift, worldPoint);

            // 물 저항
            rb.AddForce(-rb.velocity * waterDrag * Time.fixedDeltaTime);
            rb.AddTorque(-rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime);
        }
    }

    float GetWaveHeight(float x, float z, float time)
    {
        // Wave 시뮬레이션 (Water Material과 동기화)
        float wave = Mathf.Sin((x + time * waveSpeed) * waveFrequency) * waveDistance * 0.1f
                   + Mathf.Sin((z + time * waveSpeed * 1.3f) * (waveFrequency * 0.8f)) * waveDistance * 0.07f;
        return wave;
    }
}
