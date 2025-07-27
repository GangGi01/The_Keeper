using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCameraFollower : MonoBehaviour
{
    [Header("배 기준 위치")]
    public Transform anchor;  // 배 위 기준 위치 (갑판 중앙 등)

    [Header("VR Rig")]
    public Transform vrRig;   // XR Origin 또는 XR Rig

    [Header("회전 보간 속도 (0: 즉시, 5~10: 부드럽게)")]
    public float rotationLerpSpeed = 5f;

    void LateUpdate()
    {
        if (anchor != null && vrRig != null)
        {
            // 위치 따라가기 (꿀렁거림 제거)
            vrRig.position = anchor.position;

            // 회전: Yaw만 따라가되 부드럽게
            Quaternion currentRotation = vrRig.rotation;
            Quaternion targetRotation = Quaternion.Euler(0, anchor.eulerAngles.y, 0);

            vrRig.rotation = Quaternion.Slerp(
                currentRotation,
                targetRotation,
                Time.deltaTime * rotationLerpSpeed
            );
        }
    }
}
