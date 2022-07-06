using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAdjust : MonoBehaviour
{
    //float intensity;
    public Light lite;
    Color color;
    // Start is called before the first frame update
    void Start()
    {
        lite = GetComponent<Light>();
        color = lite.color;
        //intensity = lite.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        float dot = Vector3.Dot(transform.forward, Vector3.down) + 0.15f;
        float lightAmount = Mathf.Clamp(dot > 0f ? dot * 5 : 0f, 0f, 1f);
        lite.color = Color.Lerp(Color.black, color, lightAmount);

        //RenderSettings.ambientLight = lite.color;

        RenderSettings.skybox.SetFloat("_Exposure", lightAmount);
    }
}
