using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Whimsy.Creatures;
using static Navigation;

[RequireComponent(typeof(Collider))]
public class DetectionSphere : MonoBehaviour
{
    public Navigation navController;
    public Attack attack;
    // Start is called before the first frame update
    void Start()
    {

    }

    void OnTriggerStay(Collider other)
    {
        if (navController != null)
            navController.triggerEnter(other);

        if (attack != null)
            attack.OnTriggerEnter(other);
    }
}
