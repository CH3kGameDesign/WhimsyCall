using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using UnityEngine.XR.WSA;
using Whimsy;
using Whimsy.Creatures;
using Whimsy.Environment;

public class LevelGeneration : MonoBehaviour
{
    public bool debug_Flat = false;


    public GenerationInformation genInfo;

    public static LevelGeneration _publicObject;

    [HideInInspector]
    public List<Area> worldAreas = new List<Area>();

    public _MapColors mapColors = new _MapColors();

    public UnityEngine.UI.RawImage minimap;

    public int[,] worldChunks;

    public int activeArea = 0;

    public int area_LoadNextFrame = -1;

    Texture2D map;
    //Primarily For Testing
    public GameObject playerObject;

    private Transform doorwayHolder;

    void Start()
    {
        ValueAssignment();
        CreateHolderObjects();
        
        //Intended Only For New Save
        #region Generate New World
        GenerateWorld();

        DoorwayToArea(0);
        #endregion
    }

    void ValueAssignment()
    {
        _publicObject = this;
    }

    void Update()
    {
        if (area_LoadNextFrame >= 0)
            ApplyArea(area_LoadNextFrame);
        //DONT FORGET DEBUG CONTROLS
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.R))
                ApplyArea(activeArea + 1);
            if (Input.GetKeyDown(KeyCode.T))
                ApplyArea(activeArea - 1);
        }
    }

    void OnLoadArea()
    {
        

    }

    //Creates All The Objects To Hold Terrain Assets At Run-Time
    void CreateHolderObjects()
    {
        if (doorwayHolder == null)
            doorwayHolder = new GameObject().transform;
        doorwayHolder.name = "DoorwayHolder Object";

        //New Version Cuts Objects Needed by 15 times But Doesn't Allow Caves
        Transform holderZ;
        Transform holderX;
        Transform holderObject;
        for (int z = 0; z < genInfo.maxAreaSize.z; z++)
        {
            holderZ = new GameObject().transform;
            holderZ.parent = transform;
            holderZ.name = "Z (" + z + ")";
            holderZ.localPosition = new Vector3(0, 0, z * genInfo.tileSize.z);
            for (int x = 0; x < genInfo.maxAreaSize.x; x++)
            {
                holderX = new GameObject().transform;
                holderX.parent = holderZ;
                holderX.name = "X (" + x + ")";
                holderX.localPosition = new Vector3(x * genInfo.tileSize.x, 0, 0);
                //Holder Object
                {
                    holderObject = new GameObject().transform;

                    //Set up Holder Object
                    HolderObject HO = holderObject.gameObject.AddComponent<HolderObject>();
                    HO.LevelGen = this;
                    HO.gridPos = new Vector2Int(x, z);

                    holderObject.parent = holderX;
                    holderObject.name = "Holder Object ( " + x + ", " + z + ")";
                    holderObject.localPosition = new Vector3(0, genInfo.maxAreaSize.y * genInfo.tileSize.y, 0);
                }

                #region oldSystem
                /*
                for (int y = 0; y < genInfo.maxAreaSize.y; y++)
                {
                    holderObject = new GameObject().transform;
                    holderObject.parent = holderX;
                    holderObject.name = "Object ( " + x + ", " + y + ", " + z + ")";
                    holderObject.localPosition = new Vector3(0, y * genInfo.tileSize.y, 0);
                }
                */
                #endregion
            }
        }
        Debug.Log("Successfully Created " + genInfo.maxAreaSize.x * genInfo.maxAreaSize.z + " Holder Objects");
    }

    //Applies An Area's Terrain to the Holder Objects ---UNFINISHED---
    void ApplyArea(int area)
    {
        Debug.Log("Load Area: " + area);
        area_LoadNextFrame = -1;
        int prevArea = activeArea;
        activeArea = area;

        if (worldAreas[area].accurateInfo == null)
            worldAreas[area].accurateInfo = GenerateAreaAccurate(area);

        for (int x = 0; x < genInfo.maxAreaSize.x; x++)
        {
            for (int z = 0; z < genInfo.maxAreaSize.z; z++)
            {
                HolderObject holderObject = GetHolderObject(x, z);
                if (x < worldAreas[activeArea].looseInfo.tileSize.x && z < worldAreas[activeArea].looseInfo.tileSize.y)
                {
                    if (worldAreas[activeArea].accurateInfo.tileIDs[x, 1, z] > 0)
                    {
                        holderObject.gameObject.SetActive(true);
                        holderObject.Load(
                            genInfo.tilePropList.Tiles[worldAreas[activeArea].accurateInfo.tileIDs[x, 0, z]],
                            worldAreas[activeArea].accurateInfo.tileIDs[x, 1, z]);
                    }
                    else
                    {
                        if (worldAreas[activeArea].accurateInfo.tileIDs[x, 1, z] <= -100)
                        {
                            holderObject.gameObject.SetActive(true);
                            holderObject.Doorway((-worldAreas[activeArea].accurateInfo.tileIDs[x, 1, z]) - 100);
                        }
                        else
                        {
                            if (GetNeighbours(new Vector2Int(x, z)).hasNeighbours)
                            {
                                holderObject.gameObject.SetActive(true);
                                holderObject.InvisibleWall();
                            }
                            else
                                holderObject.gameObject.SetActive(false);
                        }
                    }
                }
                else
                    holderObject.gameObject.SetActive(false);
            }
        }

        //ApplyDoorways(area);
        map = GenerateMapTexture_Large(prevArea);

        RoomLoadPlayerPosition(prevArea);
    }

    public void RoomLoadPlayerPosition (int prevArea)
    {
        Vector3 tarPlayerPos = new Vector3(12, 10, 12);
        Vector3 newPlayerPos = Vector3.zero;
        if (prevArea != activeArea)
        {
            int tarChunk = -prevArea - 100;
            //New Version reliant on Blocks
            for (int x = 0; x < worldAreas[activeArea].looseInfo.tileSize.x; x++)
            {
                for (int z = 0; z < worldAreas[activeArea].looseInfo.tileSize.y; z++)
                {
                    if (worldAreas[activeArea].accurateInfo.tileIDs[x, 1, z] == tarChunk)
                    {
                        newPlayerPos = new Vector3(x * 2, 10, z * 2);
                        if (x < 5)
                            newPlayerPos += new Vector3(7,0,2);

                        if (x > worldAreas[activeArea].looseInfo.tileSize.x - 5)
                            newPlayerPos += new Vector3(-5, 0, 2);

                        if (z < 5)
                            newPlayerPos += new Vector3(3, 0, 5);

                        if (z > worldAreas[activeArea].looseInfo.tileSize.y - 5)
                            newPlayerPos += new Vector3(3, 0, -6);

                        break;
                    }
                }
                if (newPlayerPos != Vector3.zero)
                {
                    tarPlayerPos = newPlayerPos;
                    break;
                }
            }

            ///Previous Version Reliant On Chunks
            /*
            Vector2Int chunkOffset = worldAreas[activeArea].looseInfo.position;
            for (int x = 0; x <= worldAreas[activeArea].looseInfo.chunkSize.x; x++)
            {
                if (worldChunks[chunkOffset.x + x, chunkOffset.y] == tarChunk)
                    tarPlayerPos = new Vector3(x * 10 + 5, 10, 12);


                if (worldChunks[chunkOffset.x + x, chunkOffset.y + worldAreas[activeArea].looseInfo.chunkSize.y + 1] == tarChunk)
                    tarPlayerPos = new Vector3(x * 10 + 5, 10, worldAreas[activeArea].looseInfo.chunkSize.y * 10 - 2);
            }
            for (int z = 0; z <= worldAreas[activeArea].looseInfo.chunkSize.y; z++)
            {
                if (worldChunks[chunkOffset.x, chunkOffset.y + z] == tarChunk)
                    tarPlayerPos = new Vector3(12, 10, z * 10 + 5);

                if (worldChunks[chunkOffset.x + worldAreas[activeArea].looseInfo.chunkSize.x + 1, chunkOffset.y + z] == tarChunk)
                    tarPlayerPos = new Vector3(worldAreas[activeArea].looseInfo.chunkSize.x * 10 - 2, 10, z * 10 + 5);
            }
            */
        }

        playerObject.SetActive(true);
        playerObject.transform.GetChild(0).position = tarPlayerPos;
    }

    public void DoorwayToArea (int area)
    {
        area_LoadNextFrame = area;
    }

    public void ApplyDoorways(int area)
    {
        StaticScripts.DeleteChildren(doorwayHolder);

        for (int x = worldAreas[area].looseInfo.position.x; x <= worldAreas[area].looseInfo.position.x + worldAreas[area].looseInfo.chunkSize.x; x++)
        {
            for (int z = worldAreas[area].looseInfo.position.y; z <= worldAreas[area].looseInfo.position.y + worldAreas[area].looseInfo.chunkSize.y; z++)
            {
                if (worldChunks[x,z] < -1)
                {
                    GameObject GO = Instantiate(genInfo.doorWay, doorwayHolder);
                    GO.transform.position = new Vector3((x - worldAreas[area].looseInfo.position.x) * 10 + 5, 0, (z - worldAreas[area].looseInfo.position.y) * 10 + 5);
                    GO.GetComponent<Doorway>().AreaDoorwayCreation(-(worldChunks[x, z] + 100));
                    GO.name = "Doorway: " + activeArea + " > " + (-(worldChunks[x, z] + 100));
                }
            }
        }
    }

    HolderObject GetHolderObject (Vector2Int index)
    {
        if (index.x >= genInfo.maxAreaSize.x || index.y >= genInfo.maxAreaSize.z)
            return null;

        HolderObject temp = transform.GetChild(index.y).GetChild(index.x).GetChild(0).GetComponent<HolderObject>();
        return temp;
    }
    HolderObject GetHolderObject(int x, int z)
    {
        if (x >= genInfo.maxAreaSize.x || z >= genInfo.maxAreaSize.z)
            return null;

        HolderObject temp = transform.GetChild(z).GetChild(x).GetChild(0).GetComponent<HolderObject>();
        return temp;
    }

    //Gets Height Difference From Neighbours
    public Neighbours GetNeighbours(Vector2Int gridPos)
    {
        Neighbours temp = new Neighbours();
        int tempValue = worldAreas[activeArea].accurateInfo.tileIDs[gridPos.x,1,gridPos.y];
        int tempNeighbours;
        temp.hasNeighbours = false;
        if (gridPos.x > 0)
        {
            tempNeighbours = worldAreas[activeArea].accurateInfo.tileIDs[gridPos.x - 1, 1, gridPos.y];
            temp.left = tempNeighbours - tempValue;
            if (tempNeighbours > 0)
                temp.hasNeighbours = true;

        }
        else
            temp.left = -tempValue - 1;

        if (gridPos.x < (worldAreas[activeArea].looseInfo.tileSize.x) - 1)
        {
            tempNeighbours = worldAreas[activeArea].accurateInfo.tileIDs[gridPos.x + 1, 1, gridPos.y];
            temp.right = tempNeighbours - tempValue;

            if (tempNeighbours > 0)
                temp.hasNeighbours = true;
        }
        else
            temp.right = -tempValue - 1;

        if (gridPos.y > 0)
        {
            tempNeighbours = worldAreas[activeArea].accurateInfo.tileIDs[gridPos.x, 1, gridPos.y - 1];
            temp.back = tempNeighbours - tempValue;

            if (tempNeighbours > 0)
                temp.hasNeighbours = true;
        }
        else
            temp.back = -tempValue - 1;

        if (gridPos.y < (worldAreas[activeArea].looseInfo.tileSize.y) - 1)
        {
            tempNeighbours = worldAreas[activeArea].accurateInfo.tileIDs[gridPos.x, 1, gridPos.y + 1];
            temp.forward = tempNeighbours - tempValue;

            if (tempNeighbours > 0)
                temp.hasNeighbours = true;
        }
        else
            temp.forward = -tempValue - 1;

        return temp;
    }

    #region Generation
    //Generates The Loose Info Of A World
    void GenerateWorld()
    {
        if (genInfo.randomlyGenerated)
            genInfo.seed = Random.Range(0, 100000000);

        worldChunks = new int[genInfo.worldChunkSize.x, genInfo.worldChunkSize.y];
        for (int i = 0; i < genInfo.worldChunkSize.x; i++)
            for (int j = 0; j < genInfo.worldChunkSize.y; j++)
                worldChunks[i, j] = -1;

        int areaNum = GenerateAreaLoose(0,10);
        
        while (areaNum < genInfo.areaAmount)
            areaNum = GenerateAreaLoose(areaNum);

        for (int i = 0; i < 3; i++)
            ShrinkWorld();

        map = GenerateMapTexture_Large();
    }

    //Generates An Area at a random distance from the centre of the map  //UNFINISHED
    int GenerateAreaLoose(int areaNum)
    {
        //STILL NEED TO Generate Biome Num
        int biomeNum = 0;

        Vector2Int tempMapSize = Vector2Int.RoundToInt(new Vector2(
            Random.Range(genInfo.biomeList.Biomes[biomeNum].sizeRequirements.x,
            genInfo.biomeList.Biomes[biomeNum].sizeRequirements.y) + 2,
            Random.Range(genInfo.biomeList.Biomes[biomeNum].sizeRequirements.x,
            genInfo.biomeList.Biomes[biomeNum].sizeRequirements.y) + 2));

        Vector2Int mapPosition = new Vector2Int(genInfo.worldChunkSize.x / 2, genInfo.worldChunkSize.y / 2);

        Vector2 dir = Vector2.ClampMagnitude(new Vector2(
            Random.Range((-genInfo.worldChunkSize.x), (genInfo.worldChunkSize.x)),
            Random.Range((-genInfo.worldChunkSize.y), (genInfo.worldChunkSize.y))), 1);

        int range = Mathf.Min(
            genInfo.worldChunkSize.x / 2 - tempMapSize.x, 
            genInfo.worldChunkSize.y / 2 - tempMapSize.y);

        mapPosition += Vector2Int.FloorToInt(dir * Random.Range(1, range));

        //Vector2Int mapPosition = Vector2Int.RoundToInt(new Vector2(Random.Range(0, 499 - 30), Random.Range(0, 499 - 30)));

        bool createArea = true;

        for (int x = mapPosition.x - 1; x < mapPosition.x + tempMapSize.x + 1; x++)
            for (int y = mapPosition.y - 1; y < mapPosition.y + tempMapSize.y + 1; y++)
                if (worldChunks[x, y] != -1)
                    createArea = false;

        if (createArea)
        {
            for (int x = mapPosition.x; x < mapPosition.x + tempMapSize.x; x++)
                for (int y = mapPosition.y; y < mapPosition.y + tempMapSize.y; y++)
                    worldChunks[x, y] = areaNum;

            Area temp = new Area();
            temp.accurateInfo = null;
            temp.looseInfo = new AreaLooseInformation();

            temp.looseInfo.areaNum = areaNum;
            temp.looseInfo.name = "Area Number: " + areaNum;
            temp.looseInfo.chunkSize = tempMapSize;
            temp.looseInfo.tileSize = tempMapSize * 5;
            temp.looseInfo.position = mapPosition;

            temp.looseInfo.biome = genInfo.biomeList.Biomes[biomeNum];

            worldAreas.Add(temp);
            areaNum++;
        }

        return areaNum;
    }

    //Generates An Area 'range' distance away from the centre of the map //UNFINISHED
    int GenerateAreaLoose(int areaNum, int range)
    {
        //STILL NEED TO Generate Biome Num
        int biomeNum = 0;

        Vector2Int tempMapSize = Vector2Int.RoundToInt(new Vector2(
            Random.Range(genInfo.biomeList.Biomes[biomeNum].sizeRequirements.x, 
            genInfo.biomeList.Biomes[biomeNum].sizeRequirements.y) + 2, 
            Random.Range(genInfo.biomeList.Biomes[biomeNum].sizeRequirements.x,
            genInfo.biomeList.Biomes[biomeNum].sizeRequirements.y) + 2));

        Vector2Int mapPosition = new Vector2Int(genInfo.worldChunkSize.x/2, genInfo.worldChunkSize.y / 2);
        Vector2 dir = Vector2.ClampMagnitude(new Vector2(
            Random.Range((-genInfo.worldChunkSize.x), (genInfo.worldChunkSize.x)), 
            Random.Range((-genInfo.worldChunkSize.y), (genInfo.worldChunkSize.y))), 1);

        mapPosition += Vector2Int.FloorToInt(dir * Random.Range(1, range));

        //Vector2Int mapPosition = Vector2Int.RoundToInt(new Vector2(Random.Range(0, 499 - 30), Random.Range(0, 499 - 30)));

        bool createArea = true;

        for (int x = mapPosition.x - 1; x < mapPosition.x + tempMapSize.x + 1; x++)
            for (int y = mapPosition.y - 1; y < mapPosition.y + tempMapSize.y + 1; y++)
                if (worldChunks[x, y] != -1)
                    createArea = false;

        if (createArea)
        {
            for (int x = mapPosition.x; x < mapPosition.x + tempMapSize.x; x++)
                for (int y = mapPosition.y; y < mapPosition.y + tempMapSize.y; y++)
                    worldChunks[x, y] = areaNum;

            Area temp = new Area();
            temp.accurateInfo = null;
            temp.looseInfo = new AreaLooseInformation();

            temp.looseInfo.areaNum = areaNum;
            temp.looseInfo.name = "Area Number: " + areaNum;
            temp.looseInfo.chunkSize = tempMapSize;
            temp.looseInfo.tileSize = tempMapSize * 5;
            temp.looseInfo.position = mapPosition;

            worldAreas.Add(temp);
            areaNum++;
        }

        return areaNum;
    }

    //Compresses The Gap Between Each Area To Allow For Natural Corridors
    void ShrinkWorld()
    {
        bool[] areaChecked = new bool[genInfo.areaAmount];
        int numAreaChecked = 0;
        Vector2Int center = new Vector2Int(genInfo.worldChunkSize.x/2, genInfo.worldChunkSize.y/2);
        Vector2Int offset = new Vector2Int(0, 0);
        Vector2Int loc = center;
        int dir = 0;
        Vector4 bound = new Vector4(1, 1, -1, -1);
        int id = worldChunks[center.x, center.y];
        if (id >= 0)
        {
            areaChecked[id] = true;
            numAreaChecked += 1;
        }

        while (numAreaChecked < genInfo.areaAmount)
        {
            switch (dir)
            {
                case 0:
                    offset += new Vector2Int(1, 0);
                    if (offset.x >= bound.x)
                        dir += 1;
                    break;
                case 1:
                    offset += new Vector2Int(0, 1);
                    if (offset.y >= bound.y)
                        dir += 1;
                    break;
                case 2:
                    offset += new Vector2Int(-1, 0);
                    if (offset.x <= bound.z)
                        dir += 1;
                    break;
                case 3:
                    offset += new Vector2Int(0, -1);
                    if (offset.y <= bound.w)
                    {
                        dir = 0;
                        bound += new Vector4(1, 1, -1, -1);
                    }
                    break;
                default:
                    dir = 0;
                    break;
            }
            loc = center + offset;
            if (loc.x < 0 || loc.y < 0 || loc.x >= genInfo.worldChunkSize.x || loc.y >= genInfo.worldChunkSize.y)
                break;
            id = worldChunks[loc.x, loc.y];
            if (id >= 0)
            {
                if (!areaChecked[id])
                {
                    areaChecked[id] = true;
                    numAreaChecked++;

                    Vector2Int size = worldAreas[id].looseInfo.chunkSize;
                    Vector2Int pos = worldAreas[id].looseInfo.position;
                    if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
                    {
                        if (offset.x > 0)
                            MoveArea(id, 0);
                        if (offset.x < 0)
                            MoveArea(id, 2);
                        if (offset.y > 0)
                            MoveArea(id, 1);
                        if (offset.y < 0)
                            MoveArea(id, 3);
                    }
                    else
                    {
                        if (offset.y > 0)
                            MoveArea(id, 1);
                        if (offset.y < 0)
                            MoveArea(id, 3);
                        if (offset.x > 0)
                            MoveArea(id, 0);
                        if (offset.x < 0)
                            MoveArea(id, 2);
                    }

                    for (int x = pos.x; x < pos.x + size.x; x++)
                        for (int y = pos.y; y < pos.y + size.y; y++)
                            worldChunks[x, y] = -1;

                    size = worldAreas[id].looseInfo.chunkSize;
                    pos = worldAreas[id].looseInfo.position;

                    for (int x = pos.x; x < pos.x + size.x; x++)
                        for (int y = pos.y; y < pos.y + size.y; y++)
                            worldChunks[x, y] = id;
                }
            }
        }
    }

    //Moves An Area's Location (Used In ShrinkWorld())
    void MoveArea(int id, int dir)
    {
        bool move = true;
        Vector2Int size = worldAreas[id].looseInfo.chunkSize;
        Vector2Int pos = worldAreas[id].looseInfo.position;
        for (int countTemp = 0; countTemp < 300; countTemp++)
        {
            switch (dir)
            {
                case 0:
                    if (pos.x <= genInfo.worldChunkSize.x / 2)
                        move = false;
                    else
                        for (int y = -1; y < size.y + 1; y++)
                        {
                            if (worldChunks[pos.x - 1, pos.y + y] != -1 || worldChunks[pos.x - 2, pos.y + y] != -1)
                                move = false;

                        }
                    if (move)
                        pos += new Vector2Int(-1, 0);
                    break;
                case 1:
                    if (pos.y <= genInfo.worldChunkSize.y / 2)
                        move = false;
                    else
                        for (int x = -1; x < size.x + 1; x++)
                        {
                            if (worldChunks[pos.x + x, pos.y - 1] != -1 || worldChunks[pos.x + x, pos.y - 2] != -1)
                            {
                                move = false;
                            }
                        }
                    if (move)
                        pos += new Vector2Int(0, -1);
                    break;
                case 2:
                    if (pos.x + size.x >= genInfo.worldChunkSize.x / 2)
                        move = false;
                    else
                        for (int y = -1; y < size.y + 1; y++)
                        {
                            if (worldChunks[pos.x + size.x + 1, pos.y + y] != -1 || worldChunks[pos.x + size.x, pos.y + y] != -1)
                                move = false;

                        }
                    if (move)
                        pos += new Vector2Int(1, 0);
                    break;
                case 3:
                    if (pos.y + size.y >= genInfo.worldChunkSize.y / 2)
                        move = false;
                    else
                        for (int x = -1; x < size.x + 1; x++)
                        {
                            if (worldChunks[pos.x + x, pos.y + size.y + 1] != -1 || worldChunks[pos.x + x, pos.y + size.y] != -1)
                                move = false;

                        }
                    if (move)
                        pos += new Vector2Int(0, 1);
                    break;
                default:
                    move = false;
                    break;
            }
            worldAreas[id].looseInfo.position = pos;
            if (move != true)
                break;
        }
    }

    //Generates Terrain + Objects Data For An Area And Saves It To Its Object //UNFINISHED
    AreaAccurateInformation GenerateAreaAccurate(int areaNum)
    {
        AreaAccurateInformation temp = new AreaAccurateInformation();
        GenerateCorridors(areaNum);

        //Change To Generate Water Level
        temp.waterLevel = 3;

        temp.tileIDs = new int[worldAreas[areaNum].looseInfo.tileSize.x, 2, worldAreas[areaNum].looseInfo.tileSize.y];

        int[,] height = GenerateAreaHeightMap(areaNum);
        int[,] palletes = GenerateAreaPalleteMap(areaNum, temp);

        for (int x = 0; x < worldAreas[areaNum].looseInfo.tileSize.x; x++)
        {
            for (int z = 0; z < worldAreas[areaNum].looseInfo.tileSize.y; z++)
            {
                temp.tileIDs[x, 0, z] = palletes[x, z];
                temp.tileIDs[x, 1, z] = height[x, z];
            }
        }
        

        temp = GenerateProps(areaNum, temp);
        temp = GenerateCreatures(areaNum, temp);

        return temp;
    }

    //Generates An Area's Height Map to control the height of each tile, also creates 5 cell border. //UNFINISHED
    int[,] GenerateAreaHeightMap(int areaNum)
    {
        int[,] tempV2I = new int[worldAreas[areaNum].looseInfo.tileSize.x, worldAreas[areaNum].looseInfo.tileSize.y];
        Debug.Log("Tile Size: " + worldAreas[areaNum].looseInfo.tileSize);
        for (int x = 0; x < worldAreas[areaNum].looseInfo.tileSize.x; x++)
        {
            for (int z = 0; z < worldAreas[areaNum].looseInfo.tileSize.y; z++)
            {
                if (x < 5 || z < 5 || x >= worldAreas[areaNum].looseInfo.tileSize.x - 5 || z >= worldAreas[areaNum].looseInfo.tileSize.y - 5)
                {
                    tempV2I[x, z] = 0;
                }
                else
                {
                    if (debug_Flat)
                        tempV2I[x, z] = 3;
                    else
                        tempV2I[x, z] = GenerateHeightMapPoint(new Vector2Int(x, z));
                }
            }
        }

        //Temp for Research Purposes
        #region Research Purposes
        int tempCorridorNum = 0;
        int tempChunkValue = 0;
        for (int x = 5; x < worldAreas[areaNum].looseInfo.tileSize.x; x+=5)
        {
            tempChunkValue = worldChunks[Mathf.FloorToInt(x / 5) + worldAreas[activeArea].looseInfo.position.x, worldAreas[activeArea].looseInfo.position.y];
            //Debug.Log(areaNum.ToString() + "'s TempChunkValue DBounds = " + tempChunkValue);

            if (tempChunkValue < -1)
            {
                tempV2I[x + 1, 4] = GenerateHeightMapPoint(new Vector2Int(x + 1, 4));
                tempV2I[x + 2, 4] = GenerateHeightMapPoint(new Vector2Int(x + 2, 4));
                tempV2I[x + 3, 4] = GenerateHeightMapPoint(new Vector2Int(x + 3, 4));

                tempV2I[x + 1, 3] = GenerateHeightMapPoint(new Vector2Int(x + 1, 3));
                tempV2I[x + 2, 3] = GenerateHeightMapPoint(new Vector2Int(x + 2, 3));
                tempV2I[x + 3, 3] = GenerateHeightMapPoint(new Vector2Int(x + 3, 3));

                tempV2I[x + 1, 2] = tempChunkValue;
                tempV2I[x + 2, 2] = tempChunkValue;
                tempV2I[x + 3, 2] = tempChunkValue;
                tempCorridorNum++;
            }
            tempChunkValue = worldChunks[Mathf.FloorToInt(x / 5) + worldAreas[activeArea].looseInfo.position.x, worldAreas[activeArea].looseInfo.position.y + worldAreas[activeArea].looseInfo.chunkSize.y];
            //Debug.Log(areaNum.ToString() + "'s TempChunkValue UBounds = " + tempChunkValue);
            if (tempChunkValue < -1)
            {
                tempV2I[x + 1, worldAreas[areaNum].looseInfo.tileSize.y - 5] = GenerateHeightMapPoint(new Vector2Int(x + 1, worldAreas[areaNum].looseInfo.tileSize.y - 5));
                tempV2I[x + 2, worldAreas[areaNum].looseInfo.tileSize.y - 5] = GenerateHeightMapPoint(new Vector2Int(x + 2, worldAreas[areaNum].looseInfo.tileSize.y - 5));
                tempV2I[x + 3, worldAreas[areaNum].looseInfo.tileSize.y - 5] = GenerateHeightMapPoint(new Vector2Int(x + 3, worldAreas[areaNum].looseInfo.tileSize.y - 5));

                tempV2I[x + 1, worldAreas[areaNum].looseInfo.tileSize.y - 4] = GenerateHeightMapPoint(new Vector2Int(x + 1, worldAreas[areaNum].looseInfo.tileSize.y - 4));
                tempV2I[x + 2, worldAreas[areaNum].looseInfo.tileSize.y - 4] = GenerateHeightMapPoint(new Vector2Int(x + 2, worldAreas[areaNum].looseInfo.tileSize.y - 4));
                tempV2I[x + 3, worldAreas[areaNum].looseInfo.tileSize.y - 4] = GenerateHeightMapPoint(new Vector2Int(x + 3, worldAreas[areaNum].looseInfo.tileSize.y - 4));

                tempV2I[x + 1, worldAreas[areaNum].looseInfo.tileSize.y - 3] = tempChunkValue;
                tempV2I[x + 2, worldAreas[areaNum].looseInfo.tileSize.y - 3] = tempChunkValue;
                tempV2I[x + 3, worldAreas[areaNum].looseInfo.tileSize.y - 3] = tempChunkValue;
                tempCorridorNum++;
            }
        }
        for (int z = 5; z < worldAreas[areaNum].looseInfo.tileSize.y; z += 5)
        {
            tempChunkValue = worldChunks[worldAreas[activeArea].looseInfo.position.x, Mathf.FloorToInt(z / 5) + worldAreas[activeArea].looseInfo.position.y];
            //Debug.Log(areaNum.ToString() + "'s TempChunkValue LBounds = " + tempChunkValue);
            if (tempChunkValue < -1)
            {
                tempV2I[4, z + 1] = GenerateHeightMapPoint(new Vector2Int(4, z + 1));
                tempV2I[4, z + 2] = GenerateHeightMapPoint(new Vector2Int(4, z + 2));
                tempV2I[4, z + 3] = GenerateHeightMapPoint(new Vector2Int(4, z + 3));

                tempV2I[3, z + 1] = GenerateHeightMapPoint(new Vector2Int(3, z + 1));
                tempV2I[3, z + 2] = GenerateHeightMapPoint(new Vector2Int(3, z + 2));
                tempV2I[3, z + 3] = GenerateHeightMapPoint(new Vector2Int(3, z + 3));

                tempV2I[2, z + 1] = tempChunkValue;
                tempV2I[2, z + 2] = tempChunkValue;
                tempV2I[2, z + 3] = tempChunkValue;
                tempCorridorNum++;
            }
            tempChunkValue = worldChunks[worldAreas[activeArea].looseInfo.position.x + worldAreas[activeArea].looseInfo.chunkSize.x, Mathf.FloorToInt(z / 5) + worldAreas[activeArea].looseInfo.position.y];
            //Debug.Log(areaNum.ToString() + "'s TempChunkValue RBounds = " + tempChunkValue);
            if (tempChunkValue < -1)
            {
                tempV2I[worldAreas[areaNum].looseInfo.tileSize.x - 5, z + 1] = GenerateHeightMapPoint(new Vector2Int(worldAreas[areaNum].looseInfo.tileSize.x - 5, z + 1));
                tempV2I[worldAreas[areaNum].looseInfo.tileSize.x - 5, z + 2] = GenerateHeightMapPoint(new Vector2Int(worldAreas[areaNum].looseInfo.tileSize.x - 5, z + 2));
                tempV2I[worldAreas[areaNum].looseInfo.tileSize.x - 5, z + 3] = GenerateHeightMapPoint(new Vector2Int(worldAreas[areaNum].looseInfo.tileSize.x - 5, z + 3));

                tempV2I[worldAreas[areaNum].looseInfo.tileSize.x - 4, z + 1] = GenerateHeightMapPoint(new Vector2Int(worldAreas[areaNum].looseInfo.tileSize.x - 4, z + 1));
                tempV2I[worldAreas[areaNum].looseInfo.tileSize.x - 4, z + 2] = GenerateHeightMapPoint(new Vector2Int(worldAreas[areaNum].looseInfo.tileSize.x - 4, z + 2));
                tempV2I[worldAreas[areaNum].looseInfo.tileSize.x - 4, z + 3] = GenerateHeightMapPoint(new Vector2Int(worldAreas[areaNum].looseInfo.tileSize.x - 4, z + 3));

                tempV2I[worldAreas[areaNum].looseInfo.tileSize.x - 3, z + 1] = tempChunkValue;
                tempV2I[worldAreas[areaNum].looseInfo.tileSize.x - 3, z + 2] = tempChunkValue;
                tempV2I[worldAreas[areaNum].looseInfo.tileSize.x - 3, z + 3] = tempChunkValue;
                tempCorridorNum++;
            }
        }
        Debug.Log("Created: " + tempCorridorNum + " Corridors");
        #endregion
        return tempV2I;
    }

    //Generates the Height for a single point ---UNFINISHED---
    int GenerateHeightMapPoint (Vector2Int pos)
    {
        float tempF = Mathf.PerlinNoise(
                (pos.x + worldAreas[activeArea].looseInfo.position.x) * 0.12345234f,
                (pos.y + worldAreas[activeArea].looseInfo.position.y) * 0.123213f
                ) * (genInfo.maxAreaSize.y / 4);


       int temp = Mathf.RoundToInt(tempF);


        return temp;
    }

    //Generates An Area's Pallete Map to control the type of each tile. ---UNFINISHED---
    int[,] GenerateAreaPalleteMap(int areaNum, AreaAccurateInformation area)
    {
        int[,] tempV2I = new int[worldAreas[areaNum].looseInfo.tileSize.x, worldAreas[areaNum].looseInfo.tileSize.y];
        for (int x = 0; x < worldAreas[areaNum].looseInfo.tileSize.x; x++)
        {
            for (int z = 0; z < worldAreas[areaNum].looseInfo.tileSize.y; z++)
            {
                tempV2I[x, z] = 0;
            }
        }
        return tempV2I;
    }

    //Checks Adjacent Tiles For Other Areas
    void GenerateCorridors(int areaNum)
    {
        Vector2Int lesserBounds = worldAreas[areaNum].looseInfo.position;
        Vector2Int size = worldAreas[areaNum].looseInfo.chunkSize;

        int xTemp;
        int zTemp;

        int debug_CorridorNum = 0;

        List<int> areaHistory = new List<int>();

        //Lower Bounds
        zTemp = lesserBounds.y;
        for (int x = 1; x < size.x - 1; x++)
        {
            xTemp = lesserBounds.x + x;
            for (int z = 1; z <= genInfo.maxCorridorLength; z++)
            {

                if (xTemp >= 0 && xTemp < genInfo.worldChunkSize.x &&
                    zTemp - z >= 0 && zTemp - z < genInfo.worldChunkSize.y)
                {
                    if (worldChunks[xTemp, zTemp - z] >= 0 && worldChunks[xTemp, zTemp - z] != areaNum && !areaHistory.Contains(worldChunks[xTemp, zTemp - z]))
                    {
                        if (worldChunks[xTemp + 1, zTemp - z] == worldChunks[xTemp, zTemp - z] &&
                            worldChunks[xTemp - 1, zTemp - z] == worldChunks[xTemp, zTemp - z])
                        {
                            areaHistory.Add(worldChunks[xTemp, zTemp - z]);

                            worldChunks[xTemp, zTemp] = -worldChunks[xTemp, zTemp - z] - 100;
                            worldChunks[xTemp, zTemp - z] = -areaNum - 100;
                            debug_CorridorNum++;
                            break;
                        }
                    }
                }
            }
        }
        
        //Upper Bounds
        zTemp = lesserBounds.y + size.y;
        for (int x = 1; x < size.x - 1; x++)
        {
            xTemp = lesserBounds.x + x;
            for (int z = 1; z <= genInfo.maxCorridorLength; z++)
            {
                if (xTemp >= 0 && xTemp < genInfo.worldChunkSize.x &&
                    zTemp + z >= 0 && zTemp + z < genInfo.worldChunkSize.y)
                {
                    if (worldChunks[xTemp, zTemp + z] >= 0 && worldChunks[xTemp, zTemp + z] != areaNum && !areaHistory.Contains(worldChunks[xTemp, zTemp + z]))
                    {
                        if (worldChunks[xTemp + 1, zTemp + z] == worldChunks[xTemp, zTemp + z] &&
                            worldChunks[xTemp - 1, zTemp + z] == worldChunks[xTemp, zTemp + z])
                        {
                            areaHistory.Add(worldChunks[xTemp, zTemp + z]);

                            worldChunks[xTemp, zTemp] = -worldChunks[xTemp, zTemp + z] - 100;
                            worldChunks[xTemp, zTemp + z] = -areaNum - 100;
                            debug_CorridorNum++;
                            break;
                        }
                    }
                }
            }
        }
        
        //Lefter Bounds
        xTemp = lesserBounds.x;
        for (int z = 1; z < size.y - 1; z++)
        {
            zTemp = lesserBounds.y + z;
            for (int x = 1; x <= genInfo.maxCorridorLength; x++)
            {
                if (zTemp >= 0 && zTemp < genInfo.worldChunkSize.y &&
                    xTemp - x >= 0 && xTemp - x < genInfo.worldChunkSize.x)
                {
                    if (worldChunks[xTemp - x, zTemp] >= 0 && worldChunks[xTemp - x, zTemp] != areaNum && !areaHistory.Contains(worldChunks[xTemp - x, zTemp]))
                    {
                        if (worldChunks[xTemp - x, zTemp + 1] == worldChunks[xTemp - x, zTemp] &&
                            worldChunks[xTemp - x, zTemp - 1] == worldChunks[xTemp - x, zTemp])
                        {
                            areaHistory.Add(worldChunks[xTemp - x, zTemp]);

                            worldChunks[xTemp, zTemp] = -worldChunks[xTemp - x, zTemp] - 100;
                            worldChunks[xTemp - x, zTemp] = -areaNum - 100;
                            debug_CorridorNum++;
                            break;
                        }
                    }
                }
            }
        }
        
        //Righter Bounds
        xTemp = lesserBounds.x + size.x;
        for (int z = 1; z < size.y - 1; z++)
        {
            zTemp = lesserBounds.y + z;
            for (int x = 1; x <= genInfo.maxCorridorLength; x++)
            {
                if (zTemp >= 0 && zTemp < genInfo.worldChunkSize.y &&
                    xTemp + x >= 0 && xTemp + x < genInfo.worldChunkSize.x)
                {
                    if (worldChunks[xTemp + x, zTemp] >= 0 && worldChunks[xTemp + x, zTemp] != areaNum && !areaHistory.Contains(worldChunks[xTemp + x, zTemp]))
                    {
                        if (worldChunks[xTemp + x, zTemp + 1] == worldChunks[xTemp + x, zTemp] &&
                            worldChunks[xTemp + x, zTemp - 1] == worldChunks[xTemp + x, zTemp])
                        {
                            areaHistory.Add(worldChunks[xTemp + x, zTemp]);

                            worldChunks[xTemp, zTemp] = -worldChunks[xTemp + x, zTemp] - 100;
                            worldChunks[xTemp + x, zTemp] = -areaNum - 100;
                            debug_CorridorNum++;
                            break;
                        }
                    }
                }
            }
        }
        Debug.Log("Planned: " + debug_CorridorNum + " Corridors");
    }

    //Generates An Area's Initial Props. ---UNFINISHED---
    AreaAccurateInformation GenerateProps(int areaNum, AreaAccurateInformation area)
    {
        return area;
    }

    //Generates An Area's Creatures ---UNFINISHED---
    AreaAccurateInformation GenerateCreatures(int areaNum, AreaAccurateInformation area)
    {
        return area;
    }

    #endregion

    #region ViewRange

    public void ViewFinder(Vector2Int pos, int range)
    {
        for (int x = 0; x < worldAreas[activeArea].looseInfo.tileSize.x; x++)
        {
            for (int z = 0; z < worldAreas[activeArea].looseInfo.tileSize.y; z++)
            {
                bool show = false;
                if (Vector2Int.Distance(new Vector2Int(x, z), pos) <= range)
                    show = true;

                HolderObject temp = GetHolderObject(x, z);
                if (temp != null)
                    temp.HideShow(show);
            }
        }
    }


    public static Vector2Int GetGridPos(Vector3 pos)
    {
        Vector2Int temp = Vector2Int.RoundToInt(new Vector2((pos.x -1) /  _publicObject.genInfo.tileSize.x, pos.z / _publicObject.genInfo.tileSize.z));

        return temp;
    }

    public static void _ViewFinder(Vector2Int pos, int range)
    {
        _publicObject.ViewFinder(pos, range);
    }

    #endregion

    #region MapTextures
    //Generates A Map Texture Of The World With Varying Colors Based On Player's Interaction With Areas
    Texture2D GenerateMapTexture_Large()
    {
        return GenerateMapTexture_Large(-1);
    }

    Texture2D GenerateMapTexture_Large(int prevArea)
    {
        Texture2D temp = new Texture2D(genInfo.worldChunkSize.x,genInfo.worldChunkSize.y);
        for (int x = 0; x < genInfo.worldChunkSize.x; x++)
        {
            for (int y = 0; y < genInfo.worldChunkSize.y; y++)
            {
                if (worldChunks[x, y] >= 0)
                {
                    if (worldChunks[x,y] == activeArea)
                        temp.SetPixel(x, y, mapColors.currentArea);
                    else if (worldAreas[worldChunks[x,y]].accurateInfo == null)
                        temp.SetPixel(x, y, mapColors.possibleArea);
                    else if (worldChunks[x, y] == prevArea)
                        temp.SetPixel(x, y, mapColors.prevArea);
                    else
                        temp.SetPixel(x, y, mapColors.visitedArea);
                }
                else
                    temp.SetPixel(x, y, mapColors.noArea);
            }
        }
        temp.Apply();

        if (minimap != null)
            minimap.texture = temp;

        return temp;
    }

    //Generates A Map Texture Of An Area ---UNFINISHED---
    Texture2D GenerateMapTexture_Area(Area area)
    {
        Texture2D temp = new Texture2D(area.looseInfo.tileSize.x, area.looseInfo.tileSize.y);
        return temp;
    }
    #endregion

    #region Classes

    #region Areas
    public class Area
    {
        public AreaAccurateInformation accurateInfo;
        public AreaLooseInformation looseInfo;
    }

    public class AreaAccurateInformation
    {
        //Y 0 = Object Type, Y 1 = Object Height
        public int[,,] tileIDs;
        public int waterLevel;
        public List<activeProp> props;
        public List<activeCreature> creature;
    }

    public class AreaLooseInformation
    {
        public string name;
        public int areaNum;
        public Biome biome;
        public Vector2Int position;
        public Vector2Int chunkSize;
        public Vector2Int tileSize;
    }
    #endregion

    #region Objects
    public class activeProp
    {
        public Prop prop;
        public Vector3 position;
        public Quaternion rotation;
    }
    public class activeCreature
    {
        public CreatureObject prop;
        public Vector3 position;
        public Quaternion rotation;
    }
    #endregion

    #region Helpers
    public class Neighbours
    {
        public int left;
        public int right;
        public int back;
        public int forward;
        public bool hasNeighbours = false;
    }
    #endregion

    #region Map
    [System.Serializable]
    public class _MapColors
    {
        public Color currentArea = new Color(0.4f, 1, 0.4f, 1);
        public Color prevArea = new Color(0.25f, 0.7f, 0.25f, 1);
        public Color visitedArea = new Color(0.2f, 0.5f, 0.2f, 1);
        public Color possibleArea = new Color(0.1f, 0.25f, 0.1f, 1);
        [Space(10)]
        public Color currentCorridor = new Color(0.7f, 0.5f, 0.25f, 1);
        public Color visitedCorridor = new Color(0.5f, 0.35f, 0.175f, 1);
        public Color possibleCorridor = new Color(0.25f, 0.175f, 0.09f,1);
        [Space(10)]
        public Color noArea = new Color(0, 0, 0, 0.5f);


    }
    #endregion

    #endregion
}
