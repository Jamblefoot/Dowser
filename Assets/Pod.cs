using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pod : MonoBehaviour
{
    Connector connector;
    public Rigidbody rigid;

    bool falling;
    // Start is called before the first frame update
    void Awake()
    {
        connector = GetComponentInChildren<Connector>();
        rigid = GetComponent<Rigidbody>();
        rigid.sleepThreshold = 1f;
    }
    void Start()
    {
        

        if(connector.connection == null)
            Fall();
        
        
    }

    /*// Update is called once per frame
    void Update()
    {
        if(!rigid.isKinematic)
        {
            if(rigid.velocity.sqrMagnitude < 0.01f )
                rigid.isKinematic = true;
        }
    }*/

    public void Fall()
    {
        if(!falling)
            StartCoroutine(FallCo());
    }

    IEnumerator FallCo()
    {
        falling = true;
        rigid.isKinematic = false;
        connector.BreakConnection();
        rigid.WakeUp();
        while(!rigid.IsSleeping())
        {
            yield return new WaitForSeconds(Random.value * 5f);
        }

        rigid.isKinematic = true;
        falling = false;
    }
}
