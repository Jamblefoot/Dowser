using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    public GameObject playerSpritePrefab;
    public GameObject wellSpritePrefab;
    public GameObject podSpritePrefab;

    RectTransform rectTran;

    RectTransform[] icons;

    // Start is called before the first frame update
    void Start()
    {
        rectTran = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        InstantiatePositions();
        StartCoroutine(UpdatePositions());
    }
    void OnDisable()
    {
        StopAllCoroutines();
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void InstantiatePositions()
    {
        //POOL THIS!
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        icons = new RectTransform[SaveControl.instance.saveObjects.Count];
        for (int i = 0; i < SaveControl.instance.saveObjects.Count; i++)
        {
            switch (SaveControl.instance.saveObjects[i].type)
            {
                case 0: //player
                    GameObject gob = Instantiate(playerSpritePrefab, transform.position, transform.rotation, transform);
                    SetUpIcon(gob, i);
                    break;
                case 1: //ship
                    break;
                case 2:// well
                    GameObject go = Instantiate(wellSpritePrefab, transform.position, transform.rotation, transform);
                    SetUpIcon(go, i);
                    break;
                case 3: //pod
                    GameObject g = Instantiate(podSpritePrefab, transform.position, transform.rotation, transform);
                    SetUpIcon(g, i);
                    break;
                case 4: //person
                    break;
            }
        }
    }

    void SetUpIcon(GameObject go, int i)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        Vector3 pos = SaveControl.instance.saveObjects[i].position;
        if (SaveControl.instance.saveObjects[i].tran != null)
            pos = GameControl.instance.GetPlanetMapPos(SaveControl.instance.saveObjects[i].tran.position);
        rt.localPosition = new Vector3(pos.x * (500f / GameControl.instance.planetMapDimensions.x), pos.z * (500f / GameControl.instance.planetMapDimensions.z), 0);
        icons[i] = rt;
    }

    IEnumerator UpdatePositions()
    {
        Vector3 pos = Vector3.zero;
        while(true)
        {
            yield return new WaitForSeconds(1f);

            if(SaveControl.instance.saveObjects.Count != icons.Length)
                InstantiatePositions();

            for(int i = 0; i < SaveControl.instance.saveObjects.Count; i++)
            {
                if(icons[i] != null)
                {
                    if(SaveControl.instance.saveObjects[i].tran != null)
                    {
                        pos = GameControl.instance.GetPlanetMapPos(SaveControl.instance.saveObjects[i].tran.position);
                        icons[i].localPosition = new Vector3(pos.x * (500f / GameControl.instance.planetMapDimensions.x), pos.z * (500f / GameControl.instance.planetMapDimensions.z), 0);
                        if(GameControl.instance.mapDebug)
                            icons[i].GetComponent<Image>().color = Color.cyan;
                    }
                    else 
                    {
                        if (GameControl.instance.mapDebug)
                            icons[i].GetComponent<Image>().color = Color.magenta;
                    }
                }
            }
        }
    }
}
