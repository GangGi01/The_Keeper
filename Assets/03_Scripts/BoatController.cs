using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    [Header("Handle Settings")]
    public Transform handleTransform; // 핸들 (Cylinder)

    [Header("Boat Settings")]
    public Rigidbody boatRigidbody;
    public float turnSpeed = 50f; // 회전 강도
    public float maxHandleRotation = 45f; // 핸들의 최대 회전 각도

    void Update()
    {
        // 핸들의 로컬 Z축 회전값 읽기 (핸들을 Z축으로 눕혔다고 가정)
        float handleRotation = handleTransform.localEulerAngles.z;

        // -180 ~ 180도로 변환
        if (handleRotation > 180f)
            handleRotation -= 360f;

        // -maxHandleRotation ~ +maxHandleRotation → -1 ~ 1 로 정규화
        float normalizedTurn = Mathf.Clamp(handleRotation / maxHandleRotation, -1f, 1f);

        // 배 회전 적용
        RotateBoat(normalizedTurn);
    }

    void RotateBoat(float turnAmount)
    {
        // Y축 회전 토크 추가
        boatRigidbody.AddTorque(Vector3.up * turnAmount * turnSpeed * Time.deltaTime, ForceMode.Acceleration);
    }
}
