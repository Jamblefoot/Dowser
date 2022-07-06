using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WorldStreamer2;

public class DayNightControl : MonoBehaviour
{
    [SerializeField] Text latitudeText;
    [SerializeField] Text longitudeText;

    WorldMover worldMover;


    /*[SerializeField]*/ int planetCircumHor = 3000; //So this would be the horizontal loop size
    int planetCircumVer = 3000;

    Vector3 planetMapDimensions = new Vector3(3000, 0, 3000);

    // Start is called before the first frame update
    void Start()
    {
        worldMover = FindObjectOfType<WorldMover>();
    }

    // Update is called once per frame
    void Update()
    {
        if(GameControl.instance.inMenu) return;

        //Vector3 offset = Vector3.ProjectOnPlane(worldMover.playerPositionMovedLooped, Vector3.up);
        //float latitude = Mathf.Clamp(offset.magnitude, 0, planetCircumVer * 0.5f) / (planetCircumVer * 0.5f);//Mathf.Cos(transform.position.x / (planetCircumVer * 0.5f));
        //float longitude = Vector3.SignedAngle(offset, Vector3.forward, Vector3.up) / 180;//Mathf.Cos(transform.position.z / (planetCircumHor * 0.5f));
        //transform.rotation = Quaternion.Euler(longitude * 360, latitude * 360, 0);
        //transform.LookAt(worldMover.currentMove, Vector3.up);

        Vector3 pos = GameControl.instance.GetPlanetMapPos(transform.position);
        float distToCorner = Mathf.Sqrt(Mathf.Pow(planetMapDimensions.x * 0.5f, 2) + Mathf.Pow(planetMapDimensions.z * 0.5f, 2));
        float latitude = Vector3.Distance(pos, planetMapDimensions * 0.5f) / distToCorner;
        float longitude = Vector3.SignedAngle(pos, Vector3.forward, Vector3.up) / 360;
        int side = (int)Mathf.Sign(longitude);
        longitude += 0.5f;

        //if latitude < 0.5f in north
        //if latitude > 0.5f in south
        int upside = latitude < 0.5f ? 1 : -1;

        transform.rotation = Quaternion.Euler(side * latitude * 360, -longitude * 360, 0);

        latitudeText.text = latitude.ToString();
        longitudeText.text = longitude.ToString();
    }
}