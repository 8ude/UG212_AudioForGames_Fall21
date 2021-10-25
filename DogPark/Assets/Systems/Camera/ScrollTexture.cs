using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollTexture : MonoBehaviour
{
	public Vector2 scrollRate;

    private Renderer _renderer;
    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 offset = Time.time * scrollRate;
        _renderer.material.SetTextureOffset("_MainTex", offset);
    }
}
