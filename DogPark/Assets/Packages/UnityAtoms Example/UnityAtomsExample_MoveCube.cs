using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

public class UnityAtomsExample_MoveCube : MonoBehaviour
{
  public FloatReference speed;
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    transform.position += Input.GetAxis("Horizontal") * Time.deltaTime * speed.Value * Vector3.right;
  }
}
