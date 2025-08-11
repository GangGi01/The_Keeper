using UnityEngine;
using System.Collections;

public class EnvironmentTransition : MonoBehaviour
{
    [Header("Lighting")]
    public float lightingStart = 1.4f;
    public float lightingEnd = 0.85f;

    [Header("Unity Fog (RenderSettings)")]
    public FogMode fogMode = FogMode.Exponential;   // 추가: 모드 고정
    public Color fogColor = Color.gray;             // 추가: 색도 지정 가능
    public float fogDensityStart = 0f;
    public float fogDensityEnd = 0.2f;

    [Header("Skybox Fog (Material)")]
    public Material fogMaterialOverride;         // 비우면 현재 Skybox 복제 사용
    public string fogPropName = "_FogIntens";    // 실제 프로퍼티명
    public float skyboxFogStart = 0f;
    public float skyboxFogEnd = 1f;

    [Header("Transition")]
    public float transitionDuration = 2f;

    // 백업
    float _initAmbientIntensity;
    bool _initFogEnabled;
    FogMode _initFogMode;                // 추가
    Color _initFogColor;                 // 추가
    float _initFogDensity;
    Material _initSkyboxRef;             // 원래 참조
    float _initSkyboxFogValue;

    Material _runtimeSkybox;             // 런타임 인스턴스
    bool _isTransitioned = false;
    bool _isTransitioning = false;
    Coroutine _running;

    // ✅ 빌드에서 Fog 스트립 방지 & 초기 프레임 보장
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureFogVariant()
    {
        RenderSettings.fog = true;                   // 첫 프레임 전에 한 번 켜서 변종 보장
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.0001f;         // 거의 안 보일 만큼만
    }

    void Awake()
    {
        // 원래 환경값 백업
        _initAmbientIntensity = RenderSettings.ambientIntensity;
        _initFogEnabled = RenderSettings.fog;
        _initFogMode = RenderSettings.fogMode;       // 추가
        _initFogColor = RenderSettings.fogColor;     // 추가
        _initFogDensity = RenderSettings.fogDensity;
        _initSkyboxRef = RenderSettings.skybox;

        // 스카이박스 인스턴스화(자산 훼손 방지)
        var src = fogMaterialOverride != null ? fogMaterialOverride : RenderSettings.skybox;
        if (src != null)
        {
            _runtimeSkybox = new Material(src);
            RenderSettings.skybox = _runtimeSkybox;

            if (_runtimeSkybox.HasProperty(fogPropName))
                _initSkyboxFogValue = _runtimeSkybox.GetFloat(fogPropName);
        }

        // ✅ 전환 전 상태값을 확실히 세팅 (빌드에서 초기값 누락 방지)
        RenderSettings.fog = true;                   // 전환 준비 동안은 켜둠
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensityStart;
        if (_runtimeSkybox && _runtimeSkybox.HasProperty(fogPropName))
            _runtimeSkybox.SetFloat(fogPropName, skyboxFogStart);
    }

    void OnDisable()
    {
        // 원복
        RenderSettings.ambientIntensity = _initAmbientIntensity;
        RenderSettings.fog = _initFogEnabled;
        RenderSettings.fogMode = _initFogMode;       // 추가
        RenderSettings.fogColor = _initFogColor;     // 추가
        RenderSettings.fogDensity = _initFogDensity;

        if (_runtimeSkybox != null && _runtimeSkybox.HasProperty(fogPropName))
            _runtimeSkybox.SetFloat(fogPropName, _initSkyboxFogValue);

        RenderSettings.skybox = _initSkyboxRef;

        if (_running != null) StopCoroutine(_running);
        _running = null;
        _isTransitioning = false;
    }

    public void TriggerTransition()
    {
        if (_isTransitioning) return;
        bool forward = !_isTransitioned;
        _running = StartCoroutine(Transition(forward));
    }

    public void TriggerTransition(bool forward)
    {
        if (_isTransitioning) return;
        _running = StartCoroutine(Transition(forward));
    }

    IEnumerator Transition(bool forward)
    {
        if (_runtimeSkybox == null || !_runtimeSkybox.HasProperty(fogPropName))
        {
            Debug.LogWarning("Skybox runtime material or property not found.");
            yield break;
        }

        _isTransitioning = true;

        // ✅ 전환 시작 시 확실히 켜기
        RenderSettings.fog = true;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogColor = fogColor;

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

        // ✅ 끝났을 때 밀도가 사실상 0이면 꺼서 퍼포먼스 절약
        RenderSettings.fogDensity = f1;
        if (RenderSettings.fogDensity <= 0.0001f)
            RenderSettings.fog = false;

        _isTransitioned = forward;
        _isTransitioning = false;
        _running = null;
    }
}
