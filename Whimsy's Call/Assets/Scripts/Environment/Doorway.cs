using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doorway : MonoBehaviour
{
    public int areaNum = -1;
    public _doorType doorType;
    [System.Serializable]
    public enum _doorType { area,house};
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Player")
        {
            switch (doorType)
            {
                case _doorType.area:
                    if (areaNum >= 0)
                        LevelGeneration._publicObject.DoorwayToArea(areaNum);
                    break;
                case _doorType.house:
                    break;
                default:
                    break;
            }
        }
    }

    public void AreaDoorwayCreation(int _areaNum)
    {
        doorType = _doorType.area;
        areaNum = _areaNum;
    }
}
