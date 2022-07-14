using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject playerSpritePrefab;
    public GameObject wellSpritePrefab;
    public GameObject podSpritePrefab;

    RectTransform rectTran;
    // Start is called before the first frame update
    void Start()
    {
        rectTran = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        for(int i = 0; i < SaveControl.instance.saveObjects.Count; i++)
        {
            switch(SaveControl.instance.saveObjects[i].type)
            {
                case 0: //player
                    GameObject gob = Instantiate(playerSpritePrefab, transform.position, transform.rotation, transform);
                    RectTransform rtr = gob.GetComponent<RectTransform>();
                    Vector3 posi = SaveControl.instance.saveObjects[i].position;
                    if(SaveControl.instance.saveObjects[i].tran != null)
                        posi = GameControl.instance.GetPlanetMapPos(SaveControl.instance.saveObjects[i].tran.position);
                    rtr.localPosition = new Vector3(posi.x * (500f / GameControl.instance.planetMapDimensions.x), posi.z * (500f / GameControl.instance.planetMapDimensions.z), 0);
                    break;
                case 1: //ship
                    break;
                case 2:// well
                    GameObject go = Instantiate(wellSpritePrefab, transform.position, transform.rotation, transform);
                    RectTransform rt = go.GetComponent<RectTransform>();
                    Vector3 pos = SaveControl.instance.saveObjects[i].position;
                    if (SaveControl.instance.saveObjects[i].tran != null)
                        pos = GameControl.instance.GetPlanetMapPos(SaveControl.instance.saveObjects[i].tran.position);
                    rt.localPosition = new Vector3(pos.x * (500f/GameControl.instance.planetMapDimensions.x), pos.z * (500f/GameControl.instance.planetMapDimensions.z), 0);
                    break;
                case 3: //pod
                    GameObject g = Instantiate(podSpritePrefab, transform.position, transform.rotation, transform);
                    RectTransform r = g.GetComponent<RectTransform>();
                    Vector3 p = SaveControl.instance.saveObjects[i].position;
                    if (SaveControl.instance.saveObjects[i].tran != null)
                        p = GameControl.instance.GetPlanetMapPos(SaveControl.instance.saveObjects[i].tran.position);
                    r.localPosition = new Vector3(p.x * (500f / GameControl.instance.planetMapDimensions.x), p.z * (500f / GameControl.instance.planetMapDimensions.z), 0);
                    break;
                case 4: //person
                    break;
            }
        }
    }
    void OnDisable()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
