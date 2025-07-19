using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatThrottleController : MonoBehaviour
{
    [Header("Throttle (Lever) Settings")]
    public Transform throttleTransform; // 레버 오브젝트

    [Header("Boat Settings")]
    public Rigidbody boatRigidbody;
    public float maxSpeed = 10f;
    [SerializeField] private float maxThrottleAngle = 5f; // 최대 레버 기울기 (앞뒤)

    void Update()
    {
        float throttleValue = GetThrottleValue();
        ApplyThrottle(throttleValue);
    }

    float GetThrottleValue()
    {
        // 레버가 X축으로 앞뒤로 기울어진다고 가정
        float angle = throttleTransform.localEulerAngles.x;

        // Unity는 0~360도로 반환하므로 -180~180으로 보정
        if (angle > 180f)
            angle -= 360f;

        // 최대 각도로 나눠서 -1 ~ 1 범위로 정규화
        float normalized = Mathf.Clamp(angle / maxThrottleAngle, -1f, 1f);

        return normalized;
    }

    void ApplyThrottle(float throttle)
    {
        // 보트의 앞 방향으로 힘 가하기
        Vector3 forwardForce = transform.forward * throttle * maxSpeed;
        boatRigidbody.AddForce(forwardForce, ForceMode.Acceleration);

        Debug.Log($"[Throttle] angle: {throttleTransform.localEulerAngles.x}, normalized: {throttle}");



    }
}
