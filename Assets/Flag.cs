using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    public Well well;

    public MeshRenderer lightRend;

    // Update is called once per frame
    void Update()
    {
        if(well.planted)
        {
            TurnOffLight();
            Destroy(this);
        }

    }

    public void TurnOffLight()
    {
        lightRend.material.DisableKeyword("_EMISSION");
    }
}
