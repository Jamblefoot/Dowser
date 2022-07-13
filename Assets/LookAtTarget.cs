using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform target;
    Transform tran;
    // Start is called before the first frame update
    void Start()
    {
        tran = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(target == null) return;
        
        tran.LookAt(target.position);
    }
}
