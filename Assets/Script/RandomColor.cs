using UnityEngine;

public class RandomColor : MonoBehaviour
{
    [Header("Subtle cycle")]
    [Tooltip("Hue cycles per second (small = very slow)")]
    [Range(0.001f, 1f)] public float cycleSpeed = 0.05f;
    [Tooltip("Saturation of the color (lower = more muted)")]
    [Range(0f, 1f)] public float saturation = 0.35f;
    [Tooltip("Brightness of the color")]
    [Range(0f, 1f)] public float brightness = 0.95f;
    public bool randomStartHue = true;
    [Tooltip("How smoothly color interpolates (higher = snappier)")]
    [Range(0.1f, 10f)] public float smoothness = 2f;

    Renderer meshRenderer;
    SpriteRenderer spriteRenderer;
    Material instMat;
    float startHue = 0f;

    void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (meshRenderer != null)
        {
            // material returns an instance so it is safe to modify
            instMat = meshRenderer.material;
        }
        else if (spriteRenderer != null)
        {
            instMat = spriteRenderer.material;
        }

        if (randomStartHue) startHue = Random.value;
    }

    void Update()
    {
        // subtle hue cycle
        float hue = Mathf.Repeat(startHue + Time.time * cycleSpeed, 1f);
        Color target = Color.HSVToRGB(hue, saturation, brightness);

        // smooth transition from current color
        Color current = GetCurrentColor();
        Color next = Color.Lerp(current, target, Mathf.Clamp01(smoothness * Time.deltaTime));
        ApplyColor(next);
    }

    Color GetCurrentColor()
    {
        if (spriteRenderer != null) return spriteRenderer.color;
        if (instMat != null)
        {
            if (instMat.HasProperty("_Color")) return instMat.GetColor("_Color");
            // fallback
            return instMat.color;
        }
        return Color.white;
    }

    void ApplyColor(Color c)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = c;
            return;
        }

        if (instMat != null)
        {
            if (instMat.HasProperty("_Color")) instMat.SetColor("_Color", c);
            else instMat.color = c;

            if (instMat.HasProperty("_EmissionColor"))
            {
                instMat.EnableKeyword("_EMISSION");
                instMat.SetColor("_EmissionColor", c * 0.1f);
            }
        }
    }

    void OnDestroy()
    {
        // cleanup instantiated material to avoid leaks in editor/playmode
        if (instMat != null)
        {
            Destroy(instMat);
        }
    }
}
