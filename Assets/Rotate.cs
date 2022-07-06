using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public int rotAxis = 0;
    public float rotSpeed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion rot = Quaternion.identity;
        switch(rotAxis)
        {
            case 0: // x axis
                rot = Quaternion.Euler(rotSpeed * Time.deltaTime, 0, 0);
                break;
            case 1: // y axis
                rot = Quaternion.Euler(0, rotSpeed * Time.deltaTime, 0);
                break;
            case 2: // z axis
                rot = Quaternion.Euler(0, 0, rotSpeed * Time.deltaTime);
                break;
        }
        transform.localRotation = transform.localRotation * rot;
    }
}
