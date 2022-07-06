using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    [SerializeField] Transform needle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(90, 0, 0, Space.Self);
        needle.LookAt(GameControl.instance.GetMapNorthWorldPos(), transform.up);
        transform.Rotate(-90,0,0,Space.Self);
        //localRotation = Quaternion.Euler(0f, GameControl.instance.GetMapNorthWorldPos(), 0f)
    }
}
