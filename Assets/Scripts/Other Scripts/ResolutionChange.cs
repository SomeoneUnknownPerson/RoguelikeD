using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionChange : MonoBehaviour
{
	
    // Start is called before the first frame update
    void Start()
    {
        Camera.main.aspect = 1080f / 1920f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
