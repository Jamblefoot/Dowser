using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HurtUI : MonoBehaviour
{
    [SerializeField] GameObject slashPrefab;

    RectTransform canTran;
    // Start is called before the first frame update
    void Start()
    {
        canTran = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SlashAtPosition(Vector2 pos)
    {
        GameObject slash = Instantiate(slashPrefab, transform.position, transform.rotation, transform);
        RectTransform rt = slash.GetComponent<RectTransform>();
        rt.position = new Vector3(canTran.rect.width * 0.5f * pos.x, canTran.rect.height * 0.5f * pos.y, 0);
        rt.rotation = Quaternion.Euler(0, 0, Random.value * 360f);
        rt.localScale = new Vector3(Random.Range(1f, 2f), Random.Range(1f, 3f), 1);

        Destroy(slash, 0.5f);
    }
}
