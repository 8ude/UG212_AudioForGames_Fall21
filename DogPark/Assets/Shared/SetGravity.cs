using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetGravity : MonoBehaviour
{
	public float gravity = -9.81f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Physics.gravity = new Vector3(0f, gravity, 0f);   
    }
}
