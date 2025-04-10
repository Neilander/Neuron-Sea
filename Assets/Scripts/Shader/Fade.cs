using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    Material material;

    int fadePropertyID;

    float fadeValue;

    void Start(){
        //Get material reference
        material = GetComponent<SpriteRenderer>().material;

        //Convert property name to id (improves performance).
        //You can see property names by hovering over them in the material inspector.
        fadePropertyID = Shader.PropertyToID("_FullDistortionFade");

        //Set fade value to zero at start.
        fadeValue = 1;
    }

    void Update(){
        //Update while fade value is less than 1.
        if (fadeValue >0) {
            //Increase fade value over time.
            fadeValue -= Time.deltaTime;
            if (fadeValue <0) fadeValue = 1;

            //Update value in material.
            material.SetFloat(fadePropertyID, fadeValue);
        }
    }
}
