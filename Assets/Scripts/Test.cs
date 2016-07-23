using UnityEngine;
using System.Collections.Generic;
using System;

public class Test : MonoBehaviour
{
    float timer = 0.0f;
    public float timeStep = 0.1f;

    // Use this for initialization
    void Start () {
        
    }

    // Update is called once per frame
    void FixedUpdate () {
        timer += Time.fixedDeltaTime;

        while (timer >= timeStep)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f) * Mathf.Sin(timer);
            timer -= timeStep;
        }
    }
}
