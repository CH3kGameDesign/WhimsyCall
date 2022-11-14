using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Navigation;

[RequireComponent(typeof(SphereCollider))]
public class DetectionSphere : MonoBehaviour
{
    public Navigation navController;
    // Start is called before the first frame update
    void Start()
    {

    }

    void OnTriggerStay(Collider other)
    {
        if (navController != null)
            navController.triggerEnter(other);
    }
}
