using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeSkybox : MonoBehaviour
{
	public Transform camera;
	public Vector2 scrollRate;
	public float smooth;

	private Renderer _renderer;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    Vector2 offset = Vector2.zero;
    // Update is called once per frame
    void Update()
    {
    	float r = Vector3.Distance(camera.transform.position, transform.position);

    	float yScale = (r/transform.localScale.y)*Mathf.Deg2Rad*180f;
    	float xScale = Mathf.Round((r/transform.localScale.x)*Mathf.Deg2Rad*360f); // round to ensure continuous wrap around

    	float pitch = camera.eulerAngles.y;
    	float yaw = Mathf.Repeat(camera.eulerAngles.x+90f, 360f)-90f;
        offset = Vector2.Lerp(offset, new Vector2(
        	-xScale*pitch/360f,
        	yScale*yaw/180f
        ), smooth * Time.deltaTime);
        _renderer.material.SetTextureOffset("_MainTex", offset + Time.time * scrollRate);

    }
}
