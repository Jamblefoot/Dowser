using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLightAdjust : MonoBehaviour
{
    ParticleSystem.ColorOverLifetimeModule psColorModule;

    Gradient color;
    Gradient finalColor;

    LightAdjust lightAdjust;

    float minLight = 0.2f;
    // Start is called before the first frame update
    void Awake()
    {
        psColorModule = GetComponentInChildren<ParticleSystem>().colorOverLifetime;
        color = psColorModule.color.gradient;

        finalColor = new Gradient();
    }

    void Start()
    {
        lightAdjust = FindObjectOfType<LightAdjust>();
    }

    // Update is called once per frame
    void Update()
    {
        //Color ambientColor = RenderSettings.ambientLight;
        Color ambientColor = lightAdjust.lite.color;
        ambientColor.r = Mathf.Max(minLight, ambientColor.r);
        ambientColor.g = Mathf.Max(minLight, ambientColor.g);
        ambientColor.b = Mathf.Max(minLight, ambientColor.b);
        finalColor.SetKeys(SetColorKeys(ambientColor), color.alphaKeys);
        psColorModule.color = new ParticleSystem.MinMaxGradient(finalColor);
    }

    GradientColorKey[] SetColorKeys(Color colorMult)
    {
        GradientColorKey[] keys = new GradientColorKey[color.colorKeys.Length];
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i] = color.colorKeys[i];
            keys[i].color *= colorMult;
        }

        return keys;
    }
}