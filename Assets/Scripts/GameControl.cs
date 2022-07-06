using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldStreamer2;

public class GameControl : MonoBehaviour
{
    public static GameControl instance;

    public bool inMenu;

    public float time;

    WorldMover worldMover;
    Vector3 offset = new Vector3(1000f, 0f, 1000f);
    Vector3 planetMapDimensions = new Vector3(3000, 0, 3000);
    float planetCircumVert = 3000f;
    float planetCircumHor = 3000f;

    float liquidPerlinScale = 0.05f;

    public Material waterMaterial;
    public Material oilMaterial;

    // Start is called before the first frame update
    void Awake()
    {
        if(GameControl.instance != null)
            DestroyImmediate(gameObject);
        else GameControl.instance = this;


    }

    void Start()
    {
        worldMover = FindObjectOfType<WorldMover>();
    }

    // Update is called once per frame
    void Update()
    {
        if(inMenu)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            time += Time.deltaTime;
        }
    }

    public Vector3 GetPlanetMapPos(Vector3 pos)
    {
        Vector3 result = pos + worldMover.currentMove + offset;
        return new Vector3(result.x % planetCircumHor, 0, result.z % planetCircumVert);
    }
    public Vector3 GetMapNorthWorldPos()// this asserts north pole to be the center of the map, south is the corners
    {
        Vector3 center = planetMapDimensions * 0.5f - offset - worldMover.currentMove;
        return center;
    }
    public float GetLiquidAmount(Vector3 pos)
    {
        Vector3 mapPos = GetPlanetMapPos(pos);
        return Mathf.PerlinNoise(mapPos.x * liquidPerlinScale, mapPos.z * liquidPerlinScale);
    }
}
