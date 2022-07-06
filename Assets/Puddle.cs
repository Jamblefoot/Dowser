using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : MonoBehaviour
{
    [SerializeField] LayerMask groundLayers;

    bool growing;

    float targetSize;

    Mesh mesh;
    Vector3[] vertices;
    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Grow(float size, float speed)
    {
        targetSize = size;
        if(!growing)
            StartCoroutine(GrowCo(speed));
    }

    IEnumerator GrowCo(float speed)
    {
        growing = true;
        RaycastHit hit;
        while(transform.localScale.x != targetSize)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(targetSize, targetSize, targetSize), Time.deltaTime * speed);

            
            for(int i = 0; i < vertices.Length; i++)
            {
                if(Physics.Raycast(transform.TransformPoint(vertices[i]) + Vector3.up * 2f, Vector3.down, out hit, 4f, groundLayers, QueryTriggerInteraction.Ignore))
                {
                    vertices[i] = transform.InverseTransformPoint(hit.point + Vector3.up * 0.05f);
                }
            }

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            yield return new WaitForFixedUpdate();
        }
        growing = false;
    }
}
