using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClayxelCache : MonoBehaviour
{
    public TextMesh textObj;

    float deltaTime = 0.0f;
    int fps = 0;
    int solids = 0;

    // Start is called before the first frame update
    void Start()
    {
        if(!Clayxels.ClayContainer.isCacheEnabled()){
            Debug.Log("To run this example you need to set CLAYXELS_CACHEON to 1 inside claySDF.compute");

            return;
        }

        Clayxels.ClayContainer container = this.gameObject.GetComponent<Clayxels.ClayContainer>();
        container.init();
        container.clearCachedClay();
    }

    void Update(){
        this.deltaTime += (Time.deltaTime - this.deltaTime) * 0.1f;
        this.fps = (int)Mathf.Ceil(1.0f / this.deltaTime);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!Clayxels.ClayContainer.isCacheEnabled()){
            this.textObj.text = "To run this example you need to set CLAYXELS_CACHEON to 1 inside claySDF.compute";

            return;
        }

        this.textObj.text = "FPS " + this.fps + " \nSolids: " + this.solids;

        this.transform.localEulerAngles = this.transform.localEulerAngles + new Vector3(0.0f, 1.0f, 0.0f);

        Transform childTrn = this.transform.GetChild(0);

        Clayxels.ClayObject clayObj = childTrn.gameObject.GetComponent<Clayxels.ClayObject>();

        clayObj.primitiveType = UnityEngine.Random.Range(0, 6);

        clayObj.blend = UnityEngine.Random.Range(-1.0f, 1.0f);

        Color col = clayObj.color;
        clayObj.color = col + new Color(
            UnityEngine.Random.Range(-0.1f, 0.1f),
            UnityEngine.Random.Range(-0.1f, 0.1f),
            UnityEngine.Random.Range(-0.1f, 0.1f), 
            1.0f);

        if(clayObj.color.r < 0.0f){
            clayObj.color.r = 0.0f;
        }
        else if(clayObj.color.r > 1.0f){
            clayObj.color.r = 1.0f;
        }

        if(clayObj.color.g < 0.0f){
            clayObj.color.g = 0.0f;
        }
        else if(clayObj.color.g > 1.0f){
            clayObj.color.g = 1.0f;
        }

        if(clayObj.color.b < 0.0f){
            clayObj.color.b = 0.0f;
        }
        else if(clayObj.color.b > 1.0f){
            clayObj.color.b = 1.0f;
        }

        childTrn.localEulerAngles = childTrn.localEulerAngles + new Vector3(
            UnityEngine.Random.Range(-10.0f, 10.0f),
            UnityEngine.Random.Range(-10.0f, 10.0f),
            UnityEngine.Random.Range(-10.0f, 10.0f));

        Vector3 newPos = childTrn.localPosition + new Vector3(
            UnityEngine.Random.Range(-0.5f, 0.5f),
            UnityEngine.Random.Range(-0.5f, 0.5f),
            UnityEngine.Random.Range(-0.5f, 0.5f));
        
        if(newPos.x > 1.0f){
            newPos.x = 1.0f;
        }
        else if(newPos.x < -1.0f){
            newPos.x = -1.0f;
        }

        if(newPos.y > 1.0f){
            newPos.y = 1.0f;
        }
        else if(newPos.y < -1.0f){
            newPos.y = -1.0f;
        }

        if(newPos.z > 1.0f){
            newPos.z = 1.0f;
        }
        else if(newPos.z < -1.0f){
            newPos.z = -1.0f;
        }

        childTrn.localPosition = newPos;

        this.solids += 1;

        Clayxels.ClayContainer container = this.gameObject.GetComponent<Clayxels.ClayContainer>();
        container.computeClay();
        container.cacheClay();
    }
}
