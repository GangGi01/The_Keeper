using UnityEngine;

public class ShaderPropertyFinder : MonoBehaviour
{
    public Material targetMaterial;

    void Start()
    {
        if (targetMaterial == null)
        {
            Debug.LogError("타겟 머티리얼이 비어있습니다.");
            return;
        }

        var shader = targetMaterial.shader;
        int count = shader.GetPropertyCount();

        Debug.Log($"[{shader.name}] Float/Range 프로퍼티 목록:");
        for (int i = 0; i < count; i++)
        {
            var type = shader.GetPropertyType(i);
            if (type == UnityEngine.Rendering.ShaderPropertyType.Float ||
                type == UnityEngine.Rendering.ShaderPropertyType.Range)
            {
                Debug.Log($"Property {i}: {shader.GetPropertyName(i)}");
            }
        }
    }
}