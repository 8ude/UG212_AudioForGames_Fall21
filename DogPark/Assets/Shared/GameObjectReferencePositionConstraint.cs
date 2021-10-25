using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;

public class GameObjectReferencePositionConstraint : MonoBehaviour
{
	public GameObjectReference target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    	if (!target.Value) return;
        transform.position = target.Value.transform.position;
    }
}
