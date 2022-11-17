using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeOverLifetime : MonoBehaviour
{
    public Vector3 startSize = Vector3.one;
    public Vector3 endSize = Vector3.zero;
    public AnimationCurve lifetimeCurve;
    [Space (10)]
    public float lifetime = 1;
    private float timer = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(startSize,endSize, lifetimeCurve.Evaluate(timer / lifetime));
        timer += Time.deltaTime;

        if (timer > lifetime)
            Destroy(gameObject);
    }
}
