using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveMe : MonoBehaviour
{
    public int id = -1;
    public int type;
    public bool single;

    public bool initialize = true;

    // Start is called before the first frame update
    void Start()
    {
        type = FigureOutType();
        if(initialize)
            Invoke("Init", 0.5f);
        
    }
    public void Init()
    {
        if (id < 0)
            SaveControl.instance.AddSaveObject(this);
    }

    int FigureOutType()
    {
        if(GetComponent<PlayerControl>()) return 0;
        if(GetComponent<Ship>()) return 1;
        if(GetComponent<Well>()) return 2;
        if(GetComponent<Pod>()) return 3;
        if(GetComponent<PersonAI>()) return 4;

        return -1;
    }
}
