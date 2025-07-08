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
    public float waterLevel = 0.0f;

    [Header("Wave Settings (If no Material, use these)")]
    [SerializeField] private float waveSpeed = 0.2f;
    [SerializeField] private float waveFrequency = 0.1f;
    [SerializeField] private float waveDistance = 0.1f;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        // Material 값이 있으면 Material 값으로 덮어쓰기
        if (waterMaterial != null)
        {
            waveSpeed = waterMaterial.GetFloat("_WaveSpeed");
            waveFrequency = waterMaterial.GetFloat("_WaveFrequency");
            waveDistance = waterMaterial.GetFloat("_WaveDist");
        }
        else
        {
            Debug.LogWarning("⚠️ Water Material이 비어있어서, Inspector의 Wave 값 사용함");
        }
    }

    private void FixedUpdate()
    {
        foreach (var point in floatPoints)
        {
            ApplyBuoyancy(point);
        }
    }

    private void ApplyBuoyancy(Transform point)
    {
        Vector3 worldPoint = point.position;
        float waveY = GetWaveHeight(worldPoint.x, worldPoint.z, Time.time) + waterLevel;

        if (worldPoint.y < waveY)
        {
            float depth = waveY - worldPoint.y;
            Vector3 uplift = Vector3.up * buoyancyForce * depth;
            rb.AddForceAtPosition(uplift, worldPoint);

            rb.AddForce(-rb.velocity * waterDrag * Time.fixedDeltaTime);
            rb.AddTorque(-rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime);
        }
    }

    private float GetWaveHeight(float x, float z, float time)
    {
        // 잔물결용 파도 계산
        float wave = Mathf.Sin((x + time * waveSpeed) * waveFrequency) * waveDistance * 0.1f
                   + Mathf.Sin((z + time * waveSpeed * 1.3f) * (waveFrequency * 0.8f)) * waveDistance * 0.07f;
        return wave;
    }
}
