using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldStreamer2;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    public static GameControl instance;

    public bool inMenu;
    [SerializeField] GameObject menuCanvas;
    [SerializeField] GameObject mapCanvas;
    [SerializeField] GameObject tipCanvas;
    [SerializeField] Text alertText;

    public float time;
    bool displayingAlert;

    public bool mapDebug = false;

    public PlayerControl player;

    WorldMover worldMover;
    Vector3 offset = new Vector3(1000f, 0f, 1000f);
    public Vector3 planetMapDimensions = new Vector3(3000, 0, 3000);
    //float planetCircumVert = 3000f;
    //float planetCircumHor = 3000f;

    float liquidPerlinScale = 0.05f;

    public Material waterMaterial;
    public Material oilMaterial;

    public LayerMask groundLayers;

    public GameObject monsterPrefab;
    GameObject[] monsters = new GameObject[10];

    [SerializeField] AudioClip[] footstepSounds;
    [SerializeField] AudioClip[] squishSounds;
    [SerializeField] AudioClip[] bounceSounds;
    [SerializeField] AudioClip[] hitSounds;
    [SerializeField] AudioClip[] thudSounds;

    // Start is called before the first frame update
    void Awake()
    {
        if(GameControl.instance != null)
            DestroyImmediate(gameObject);
        else GameControl.instance = this;

        worldMover = FindObjectOfType<WorldMover>();

        
    }

    void Start()
    {
        if (menuCanvas != null) menuCanvas.SetActive(false);

        player = FindObjectOfType<PlayerControl>();

        StartCoroutine(SpawnMonster());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Cancel"))
            inMenu = !inMenu;

        if(Input.GetKeyDown(KeyCode.M))
        {
            mapCanvas.SetActive(!mapCanvas.activeSelf);
        }
        if(Input.GetKeyDown(KeyCode.T))
        {
            tipCanvas.SetActive(!tipCanvas.activeSelf);
        }
        if(mapCanvas.activeSelf)
        {
            if(Input.GetKeyDown(KeyCode.L))
                mapDebug = !mapDebug;
        }
        else mapDebug = false;

        if(inMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            if(!menuCanvas.activeSelf)
                menuCanvas.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            time += Time.deltaTime;
            if(menuCanvas.activeSelf)
                menuCanvas.SetActive(false);
        }
    }

    public Vector3 GetPlanetMapPos(Vector3 pos) //this pos is a world pos, returns planet relative
    {
        Vector3 result = pos - worldMover.currentMove + offset;
        result.x %= planetMapDimensions.x;
        result.z %= planetMapDimensions.z;
        while(result.x < 0) result.x += planetMapDimensions.x;
        while(result.x > planetMapDimensions.x) result.x -= planetMapDimensions.x;
        while(result.z < 0) result.z += planetMapDimensions.z;
        while(result.z > planetMapDimensions.z) result.z -= planetMapDimensions.z;
        return result;//new Vector3(result.x % planetMapDimensions.x, pos.y, result.z % planetMapDimensions.z);
    }
    public Vector3 GetPlayerWorldPos(Vector3 pos)
    {
        Vector3 result = pos - offset;
        return new Vector3(result.x, pos.y, result.z);
    }
    public Vector3 GetMovedWorldPos(Vector3 pos) //this pos is a planet relative pos, returns world relative
    {
        Vector3 result =  pos - offset;
        result += worldMover.currentMove;
        //if(result.x < -planetMapDimensions.x * 0.5f)
        return new Vector3(result.x, pos.y, result.z);//(result.x % planetMapDimensions.x, pos.y, result.z % planetMapDimensions.z);
    }
    public Vector3 GetWorldPosRelative(Vector3 pos, Vector3 relativePos)// both these pos's are planet relative
    {
        Vector3 result = pos;
        float middle = planetMapDimensions.x * 0.5f;
        Vector2 offsetFromCenter = new Vector2(middle - relativePos.x, middle - relativePos.z);
        if(pos.x > planetMapDimensions.x - offsetFromCenter.x)
            result.x -= planetMapDimensions.x;
        else if(pos.x < -offsetFromCenter.x)
            result.x += planetMapDimensions.x;
        if(pos.z > planetMapDimensions.z - offsetFromCenter.y)
            result.z -= planetMapDimensions.z;
        else if(pos.z < -offsetFromCenter.y)
            result.z += planetMapDimensions.z;

        return GetMovedWorldPos(result);
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

    IEnumerator SpawnMonster()
    {
        RaycastHit hit;
        while(true)
        {
            int count = Random.Range(1, 6);
            Vector3 pos = player.tran.position + new Vector3(Random.Range(50f, 400f), 0, Random.Range(50f, 400f));
            if(Physics.Raycast(pos + Vector3.up * 1000, Vector3.down, out hit, Mathf.Infinity, groundLayers, QueryTriggerInteraction.Ignore))
            {
                pos = hit.point;
            }
            else yield return new WaitForSeconds(Random.Range(30f, 60f));
            for(int i = 0; i < monsters.Length; i++)
            {
                if(count <= 0) break;
                if(monsters[i] == null)
                {
                    monsters[i] = Instantiate(monsterPrefab, pos, Quaternion.identity);
                    count -= 1;
                }
            }

            yield return new WaitForSeconds(Random.Range(30f, 360f));
        }
    }

    public void DisplayAlert(string alert, float seconds)
    {
        if(displayingAlert) StopCoroutine("DisplayAlertCo");
        StartCoroutine(DisplayAlertCo(alert, seconds));
    }
    IEnumerator DisplayAlertCo(string alert, float seconds)
    {
        displayingAlert = true;
        alertText.text = alert;

        yield return new WaitForSeconds(seconds);

        alertText.text = "";
        displayingAlert = false;
    }

    public void Quit()
    {
        Application.Quit();
    }
    public void Restart()
    {
        SceneManager.LoadScene(0);//SceneManager.GetActiveScene().buildIndex);
    }

    public AudioClip GetFootstep()
    {
        return footstepSounds[Random.Range(0, footstepSounds.Length)];
    }
    public AudioClip GetSquish()
    {
        return squishSounds[Random.Range(0, squishSounds.Length)];
    }
    public AudioClip GetBounce()
    {
        return bounceSounds[Random.Range(0, bounceSounds.Length)];
    }
    public AudioClip GetHit()
    {
        return hitSounds[Random.Range(0, hitSounds.Length)];
    }
    public AudioClip GetThud()
    {
        return thudSounds[Random.Range(0, thudSounds.Length)];
    }
}
