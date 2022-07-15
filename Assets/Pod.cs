using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pod : MonoBehaviour
{
    [SerializeField] MeshRenderer podRenderer;
    [SerializeField] int windowMaterialIndex = 0;

    Connector connector;
    public Rigidbody rigid;

    bool falling;

    [Range(0,1)]public float fill = 0f;
    float maxFill = 1000f;
    public float contents = 0f;
    public float waterAmount = 0f;
    public float oilAmount = 0f;
    public float bioAmount = 0f;

    [SerializeField] GameObject personPrefab;
    PersonAI person;

    [SerializeField] LayerMask groundLayers;

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
        
        SetLiquid(contents);
        //podRenderer.materials[windowMaterialIndex].SetFloat("_LiquidHeight", fill * 2 - 1);
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
            CheckGrounded();

            yield return new WaitForSeconds(Random.value * 5f);
        }

        rigid.isKinematic = true;
        falling = false;
    }

    public bool AddLiquid(float amount, int liquidType)
    {
        if(contents >= maxFill)
            return false;

        contents += amount;
        contents = Mathf.Clamp(contents, 0, maxFill);
        fill = contents / maxFill;

        switch(liquidType)
        {
            case 0:
                waterAmount += amount;
                break;
            case 1:
                oilAmount += amount;
                break;
            case 2:
                bioAmount += amount;
                break;
        }

        SetLiquidMaterial();

        if(contents >= maxFill && person == null)
            CreatePerson();

        return true;
    }
    public void SetLiquid(float amount, float oilPercent = 0, float bioPercent = 0)
    {
        contents = Mathf.Clamp(amount, 0f, maxFill);
        oilAmount = oilPercent * (amount / 100f);
        bioAmount = bioPercent * (amount / 100f);
        waterAmount = contents - oilAmount - bioAmount;
        fill = contents / maxFill;
        SetLiquidMaterial();
    }
    public Vector3 GetLiquid()
    {
        return new Vector3(contents, contents <= 0 ? 0 : (oilAmount / contents) * 100f, contents <= 0 ? 0 : (bioAmount / contents) * 100f);
    }

    void SetLiquidMaterial()
    {
        Material mat = podRenderer.materials[windowMaterialIndex];
        mat.SetFloat("_LiquidHeight", fill * 2f - 1f);
        mat.SetFloat("_OilHeight", oilAmount / contents);
        mat.SetFloat("_BioWeight", bioAmount / (bioAmount + waterAmount));
    }

    void CreatePerson()
    {
        person = Instantiate(personPrefab, transform.position, Quaternion.identity).GetComponent<PersonAI>();
    }

    bool CheckGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, groundLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.distance <= 3f)
                return true;
        }
        else
        {
            if (Physics.Raycast(transform.position + Vector3.up * 2000f, Vector3.down, out hit, 2000, groundLayers, QueryTriggerInteraction.Ignore))
            {
                transform.position = hit.point + Vector3.up * 3f;
                Debug.Log("pod was underground!");
                return true;
            }
        }

        return false;
    }
}
