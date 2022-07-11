using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Connection : MonoBehaviour
{
    public Connector occupant;

    public UnityEvent connectionEstablished;
    public UnityEvent connectionLost;

    public Transform root;

    public float flow = 10;
    public int liquidType = 0;

    Pod pod;

    void Start()
    {
        pod = GetComponentInParent<Pod>();
    }

    public void BreakConnection()
    {
        if(occupant != null)
            occupant.BreakConnection();
    }

    public int GetLiquidType()
    {
        if(pod != null)
        {
            if(pod.oilAmount > 0)
                return 1;
            else if(pod.bioAmount > 0)
                return 2;
            else if(pod.waterAmount > 0)
                return 0;
        }

        return liquidType;
    }
    public void WithdrawLiquid(float amount, int liquidType)
    {
        if(pod == null) return;

        switch(liquidType)
        {
            case 0:
                pod.waterAmount -= amount;
                pod.waterAmount = Mathf.Max(0,pod.waterAmount);
                break;
            case 1:
                pod.oilAmount -= amount;
                pod.oilAmount = Mathf.Max(0, pod.oilAmount);
                break;
            case 2:
                pod.bioAmount -= amount;
                pod.bioAmount = Mathf.Max(0, pod.bioAmount);
                break;
        }
    }
}
