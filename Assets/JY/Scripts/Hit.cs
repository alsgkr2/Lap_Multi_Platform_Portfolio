using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.gameObject.TryGetComponent(out JY_Player i))
                StartCoroutine(i.Hit());
            else if (other.gameObject.TryGetComponent(out ClientCube j))
                StartCoroutine(j.Hit());
            else if (other.gameObject.TryGetComponent(out Cube k))
                StartCoroutine(k.Hit());
        }
        GetComponent<MeshRenderer>().enabled = false;
    }
}
