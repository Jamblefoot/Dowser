using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveMe : MonoBehaviour
{
    public int id = -1;
    public int type;
    public bool single;

    // Start is called before the first frame update
    void Start()
    {
        type = FigureOutType();
        Invoke("Init", 1.5f);
        
    }
    void Init()
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
