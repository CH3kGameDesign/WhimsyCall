using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Whimsy;

namespace Whimsy.Environment
{
    public class HolderObject : MonoBehaviour
    {
        public LevelGeneration LevelGen;

        public LevelGeneration.Neighbours neighbours;

        public GroundTile tileType;
        public GameObject prop;

        public Vector2Int gridPos;

        public int height;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        //Load New Palette and Height
        public void Load(GroundTile _tileType, int _height)
        {
            tileType = _tileType;
            height = _height;

            UpdateObject();
        }

        public void InvisibleWall()
        {
            StaticScripts.DeleteChildren(transform);

            GameObject GO = Instantiate(LevelGen.genInfo.invisibleWall, transform);
            GO.transform.localPosition = Vector3.zero;
        }

        public void Doorway(int area)
        {
            StaticScripts.DeleteChildren(transform);

            GameObject GO = Instantiate(LevelGen.genInfo.doorWay, transform);
            GO.transform.localPosition = Vector3.zero;
            GO.GetComponent<Doorway>().AreaDoorwayCreation(area);
        }

        //Update Object Based On Current Pallete and Height
        public void UpdateObject()
        {
            transform.localPosition = new Vector3(0, height, 0);
            StaticScripts.DeleteChildren(transform);

            neighbours = LevelGen.GetNeighbours(gridPos);

            int childNumber = GetObjectChildNumberFromNeighbours();

            GameObject GO = Instantiate(tileType.palette.transform.GetChild(0).GetChild(childNumber).gameObject, transform);
            GO.transform.localPosition = Vector3.zero;

            UpdateSides();
            UpdateStairs();
        }

        //Get Object Child Number for Palette Object based on Neighbours
        int GetObjectChildNumberFromNeighbours()
        {
            int temp = 0;

            if (neighbours.left < 0)
                temp += 1;
            if (neighbours.right < 0)
                temp += 2;
            if (neighbours.back < 0)
                temp += 4;
            if (neighbours.forward < 0)
                temp += 8;

            return temp;
        }

        //Get Stair Child Number for Palette Object based on Neighbours
        int GetStairChildNumberFromNeighbours()
        {
            int temp = 0;

            if (neighbours.left == 1)
                temp += 1;
            if (neighbours.right == 1)
                temp += 2;
            if (neighbours.back == 1)
                temp += 4;
            if (neighbours.forward == 1)
                temp += 8;

            return temp;
        }

        //Update The Vertical Size of the Sides Of The Object
        void UpdateSides()
        {
            transform.GetChild(0).GetChild(1).localScale = new Vector3(1, SideLength(neighbours.left, transform.GetChild(0).GetChild(1).localPosition.y), 1);
            transform.GetChild(0).GetChild(2).localScale = new Vector3(1, SideLength(neighbours.right, transform.GetChild(0).GetChild(2).localPosition.y), 1);
            transform.GetChild(0).GetChild(3).localScale = new Vector3(1, SideLength(neighbours.back, transform.GetChild(0).GetChild(3).localPosition.y), 1);
            transform.GetChild(0).GetChild(4).localScale = new Vector3(1, SideLength(neighbours.forward, transform.GetChild(0).GetChild(4).localPosition.y), 1);
        }

        //Helps offset bevelling & if Neighbour Distance is >0 don't show
        float SideLength(float distance, float height)
        {
            if (distance < 1)
                distance = ((distance + 1) / height) - 1;
            else
                distance = 0;

            return distance;
        }


        void UpdateStairs()
        {
            int childNumber = GetStairChildNumberFromNeighbours();
            GameObject GO = Instantiate(tileType.palette.transform.GetChild(1).GetChild(childNumber).gameObject, transform);
            GO.transform.localPosition = Vector3.zero;
        }

        public void HideShow(bool show)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (show)
                {
                    transform.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
    }
}