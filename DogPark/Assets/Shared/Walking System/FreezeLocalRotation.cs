using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeLocalRotation : MonoBehaviour
{
	private Quaternion initialLocalRotation;
    // Start is called before the first frame update
    void Start()
    {
        initialLocalRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = initialLocalRotation;
    }
}
