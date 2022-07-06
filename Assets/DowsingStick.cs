using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DowsingStick : MonoBehaviour
{
    public float dryRotation = -90;
    public float waterRotation = -45;
    [SerializeField] LayerMask groundLayers;
    public Ship ship;

    float perlinScale = 0.05f;

    [SerializeField] GameObject flag;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 mapPos = GameControl.instance.GetPlanetMapPos(transform.position);
        //transform.localRotation = Quaternion.Euler(Mathf.Lerp(dryRotation, waterRotation, Mathf.PerlinNoise(mapPos.x * perlinScale, mapPos.z * perlinScale)), 0, 0);
        transform.localRotation = Quaternion.Euler(Mathf.Lerp(dryRotation, waterRotation, GameControl.instance.GetLiquidAmount(transform.position)), 0, 0);

        if(Input.GetButtonDown("Fire2"))
        {
            PlaceWell();
        }
    }

    bool PlaceWell()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, 5f, groundLayers, QueryTriggerInteraction.Ignore))
        {
            float dist = 1000f;
            foreach(Flag flag in FindObjectsOfType<Flag>())
            {
                dist = Mathf.Min(dist, Vector3.Distance(hit.point, flag.transform.position));
            }
            if(dist <= 50f)
                return false;

            foreach(Well well in FindObjectsOfType<Well>())
            {
                dist = Mathf.Min(dist, Vector3.Distance(hit.point, well.transform.position));
            }

            if(dist > 50f)
            {
                GameObject well = ship.LaunchWellToPosition(hit.point);
                Flag f = Instantiate(flag, hit.point, Quaternion.identity).GetComponent<Flag>();
                f.well = well.GetComponent<Well>();
                return true;
            }
            else
            {
                Debug.Log("Too close to other well");
            }
        }

        return false;
    }
}
