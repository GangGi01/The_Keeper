using UnityEngine;
using System.Collections;

public class EnvironmentTransition : MonoBehaviour
{
    [Header("Lighting")]
    public float lightingStart = 1.4f;
    public float lightingEnd = 0.85f;

    [Header("Unity Fog (RenderSettings)")]
    public float fogDensityStart = 0f;
    public float fogDensityEnd = 0.2f;

    [Header("Skybox Fog (Material)")]
    public Material fogMaterialOverride;   // 비워두면 현재 Skybox를 복제해서 사용
    public string fogPropName = "_FogIntens"; // 너가 찾은 실제 이름
    public float skyboxFogStart = 0f;
    public float skyboxFogEnd = 1f;

    public float transitionDuration = 2f;

    // 백업용
    float _initAmbientIntensity;
    bool _initFogEnabled;
    float _initFogDensity;
    Material _initSkyboxRef;   // 원래 참조
    float _initSkyboxFogValue;

    Material _runtimeSkybox;   // 플레이 중 사용할 인스턴스
    bool _isTransitioned = false; // 토글 상태

    void Awake()
    {
        // 원래 환경값 백업
        _initAmbientIntensity = RenderSettings.ambientIntensity;
        _initFogEnabled = RenderSettings.fog;
        _initFogDensity = RenderSettings.fogDensity;
        _initSkyboxRef = RenderSettings.skybox;

        // 스카이박스 인스턴스화(자산 훼손 방지)
        var src = fogMaterialOverride != null ? fogMaterialOverride : RenderSettings.skybox;
        if (src != null)
        {
            _runtimeSkybox = new Material(src);
            RenderSettings.skybox = _runtimeSkybox;

            // 초기 스카이박스 Fog 값 백업
            if (_runtimeSkybox.HasProperty(fogPropName))
                _initSkyboxFogValue = _runtimeSkybox.GetFloat(fogPropName);
        }
    }

    void OnDisable()
    {
        // 플레이 종료/컴포넌트 비활성 시 원복
        RenderSettings.ambientIntensity = _initAmbientIntensity;
        RenderSettings.fog = _initFogEnabled;
        RenderSettings.fogDensity = _initFogDensity;

        if (_runtimeSkybox != null && _runtimeSkybox.HasProperty(fogPropName))
            _runtimeSkybox.SetFloat(fogPropName, _initSkyboxFogValue);

        // 원래 스카이박스 참조 복원
        RenderSettings.skybox = _initSkyboxRef;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            // 토글: 현재 상태에 따라 반대로 보간
            if (_isTransitioned)
                StartCoroutine(Transition(false)); // 되돌리기
            else
                StartCoroutine(Transition(true));  // 목표로 전환

            _isTransitioned = !_isTransitioned;
        }
    }

    IEnumerator Transition(bool forward)
    {
        if (_runtimeSkybox == null || !_runtimeSkybox.HasProperty(fogPropName))
        {
            Debug.LogWarning("Skybox runtime material or property not found.");
            yield break;
        }

        RenderSettings.fog = true; // 전환 중엔 켜두기

        float elapsed = 0f;
        float a0 = forward ? lightingStart : lightingEnd;
        float a1 = forward ? lightingEnd : lightingStart;

        float f0 = forward ? fogDensityStart : fogDensityEnd;
        float f1 = forward ? fogDensityEnd : fogDensityStart;

        float s0 = forward ? skyboxFogStart : skyboxFogEnd;
        float s1 = forward ? skyboxFogEnd : skyboxFogStart;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            RenderSettings.ambientIntensity = Mathf.Lerp(a0, a1, t);
            RenderSettings.fogDensity = Mathf.Lerp(f0, f1, t);
            _runtimeSkybox.SetFloat(fogPropName, Mathf.Lerp(s0, s1, t));

            yield return null;
        }
    }
}
