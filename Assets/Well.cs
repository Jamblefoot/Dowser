using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Well : MonoBehaviour
{
    bool moving = false;

    Vector3 velocity;
    float spinSpeed;

    Rigidbody rigid;
    Transform tran;

    [SerializeField] ParticleSystem smoke;
    [SerializeField] ParticleSystem liquid;

    [SerializeField] MeshRenderer[] lights;

    bool pumping;
    public bool planted;

    [SerializeField] LayerMask groundLayer;

    float richness = 0;
    int output = 0; //this will be code for type of output

    public Ship ship;
    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        tran = transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToLocation(Vector3 pos)
    {
        if(!moving)
            StartCoroutine(MoveCo(pos));
    }

    IEnumerator MoveCo(Vector3 pos)
    {
        moving = true;
        while((tran.position - pos).sqrMagnitude > 0.5f && !planted)
        {
            Vector3 position = tran.position;
            float dist = (position - pos).sqrMagnitude;

            RaycastHit hit;
            if(Physics.Raycast(position, Vector3.down, out hit, Mathf.Max(rigid.velocity.magnitude * Time.deltaTime, 4f), groundLayer, QueryTriggerInteraction.Ignore))
            {
                transform.position = hit.point + Vector3.up * Mathf.Min(4f, hit.distance);
                transform.LookAt(hit.point, Vector3.up);
                planted = true;
                StartCoroutine(Drill(hit.point));
            }
            else
            {
                Vector3 topOffset = new Vector3(pos.x - position.x, 0, pos.z - position.z);
                if(topOffset.magnitude > 50f)
                {
                    transform.LookAt(position + topOffset, Vector3.up);
                    rigid.AddForce(transform.forward * rigid.mass * 2f, ForceMode.Force);
                }
                else
                {
                    transform.LookAt(pos, Vector3.up);
                    rigid.AddForce(transform.forward * rigid.mass, ForceMode.Force);
                }
            }

            
            //rigid.AddForce(transform.forward * rigid.mass * 20f, ForceMode.Force);
            //transform.LookAt(pos, Vector3.up);
            
            Transform child = transform.GetChild(0);
            spinSpeed = 1000f * Mathf.Min(1f, dist);
            child.localRotation = child.localRotation * Quaternion.Euler(0,0, spinSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        moving = false;
        rigid.isKinematic = true;
        

        
    }

    IEnumerator Drill(Vector3 pos)
    {
        Transform child = tran.GetChild(0);
        while((tran.position - pos).magnitude > 0.1f)
        {
            float dist = (tran.position - pos).magnitude;
            spinSpeed = 1000f * Mathf.Min(1f, dist);
            child.localRotation = child.localRotation * Quaternion.Euler(0, 0, spinSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        if (smoke != null)
        {
            var smokeEmission = smoke.emission;
            smokeEmission.rateOverTime = 0;
        }
        if (!pumping)
            StartCoroutine(StartPumping());
    }

    IEnumerator StartPumping()
    {
        pumping = true;

        float rand = Random.value;
        if(rand <= 0.1f)
        {
            //nothing
            output = 0;
        }
        else if(rand <= 0.5f)
        {
            //water
            output = 1;
        }
        else if(rand <= 0.8f)
        {
            //oil
            output = 2;
        }
        else
        {
            //monsters
            output = 3;
        }

        Material material = null;

        switch(output)
        {
            case 0: //nothing
                break;
            case 1: //water
                material = GameControl.instance.waterMaterial;
                break;
            case 2: //oil
                material = GameControl.instance.oilMaterial;
                break;
            case 3: //monster
                break;
        }

        liquid.GetComponent<ParticleSystemRenderer>().material = material;

        yield return null;

        if(output != 0)
        {
            //TODO make some initial gurgling noise

            yield return new WaitForSeconds(Random.value * 5f);

            richness = GameControl.instance.GetLiquidAmount(transform.position);

            if(output != 3)
            {
                if(liquid != null && liquid.gameObject.activeSelf)
                {
                    var liquidEmission = liquid.emission;
                    liquidEmission.rateOverTime = richness * 10;
                    liquid.Play();
                }

                int lightLevel = Mathf.CeilToInt(richness * lights.Length);
                for(int i = 0; i < lightLevel; i++)
                {
                    lights[i].material.EnableKeyword("_EMISSION");
                    lights[i].GetComponent<Light>().enabled = true;

                    ship.LaunchPodToPosition(transform.position);
                }

                Puddle pud = GetComponentInChildren<Puddle>();
                pud.Grow(1 + richness * 4f, richness);
                pud.GetComponent<MeshRenderer>().material = material;
            }
            else 
            {
                if(liquid != null && liquid.gameObject.activeSelf)
                    liquid.GetComponent<Spawner>().Spawn();
                lights[0].material.EnableKeyword("_EMISSION");
                lights[0].GetComponent<Light>().enabled = true;

                ship.LaunchPodToPosition(transform.position);
            }

            Connection con = GetComponentInChildren<Connection>();
            con.flow = richness * 20;
            con.liquidType = output - 1;
        }
        else
        {
            GetComponentInChildren<Connection>().enabled = false;
        }
    }
}
