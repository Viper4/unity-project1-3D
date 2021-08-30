using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    Light myLight;

    public bool smooth = true;

    public float intensityScrollSpeed = 1f;
    public float intensityBase = 1f;
    public float intensityJumpScale = 0.1f;
    public float positionScrollSpeed = 1f;
    public float positionJumpScale = 0.1f;

    Vector3 lightPosition;

    private void Awake()
    {
        myLight = GetComponent<Light>();

        lightPosition = myLight.transform.localPosition;
    }

    private void Update()
    {
        float intensityNoise = intensityBase + (intensityJumpScale * Mathf.PerlinNoise(Time.time * intensityScrollSpeed, 1f + Time.time * intensityScrollSpeed));
        Vector3 randomPosition = lightPosition + PositionDelta(positionScrollSpeed, positionJumpScale);
        if (smooth)
        {
            myLight.intensity = intensityNoise;
        }
        else
        {
            myLight.intensity = intensityNoise > (intensityBase + intensityJumpScale) / 2 ? intensityBase : intensityJumpScale;
        }
        myLight.transform.localPosition = randomPosition;
    }

    private Vector3 PositionDelta(float positionScrollSpeed, float scale)
    {
        float x = Mathf.PerlinNoise(Time.time * positionScrollSpeed, 1f + Time.time * positionScrollSpeed) - 0.5f;
        float y = Mathf.PerlinNoise(2f + Time.time * positionScrollSpeed, 3f + Time.time * positionScrollSpeed) - 0.5f;
        float z = Mathf.PerlinNoise(4f + Time.time * positionScrollSpeed, 5f + Time.time * positionScrollSpeed) - 0.5f;
        return new Vector3(x, y, z) * scale;
    }
}