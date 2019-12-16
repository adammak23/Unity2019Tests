using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTool : MonoBehaviour
{
    public enum TerrainModificationAction
    {
        Raise,
        Lower,
        Flatten,
    }

    public float range;

    public int areaOfEffectSize;

    [Range(0.001f, 0.1f)]
    public float effectIncrement;

    public float sampledHeight;

    public TerrainModificationAction modificationAction;

    public LayerMask modificationLayerMask;

    private Terrain targetTerrain;

    private TerrainData targetTerrainData;

    private float[,] terrainHeightMap;

    private int terrainHeightMapWidth;
    private int terrainHeightMapHeight;

    private void Update()
    {
        if (Camera.main)
        {
            if (Input.GetButton("Primary Action"))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, range, modificationLayerMask))
                {
                    if (GetTerrainAtObject(hit.transform.gameObject))
                    {
                        targetTerrain = GetTerrainAtObject(hit.transform.gameObject);
                        SetEditValues(targetTerrain);
                    }

                    switch (modificationAction)
                    {
                        case TerrainModificationAction.Raise:
                            RaiseTerrain(targetTerrain, hit.point, effectIncrement);
                            break;

                        case TerrainModificationAction.Lower:
                            LowerTerrain(targetTerrain, hit.point, effectIncrement);
                            break;

                        case TerrainModificationAction.Flatten:
                            FlattenTerrain(targetTerrain, hit.point, sampledHeight);
                            break;
                    }
                }
            }

            if (Input.GetButton("Secondary Action"))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, range, modificationLayerMask))
                {
                    if (GetTerrainAtObject(hit.transform.gameObject))
                    {
                        targetTerrain = GetTerrainAtObject(hit.transform.gameObject);
                        SetEditValues(targetTerrain);
                    }

                    switch (modificationAction)
                    {
                        case TerrainModificationAction.Flatten:
                            sampledHeight = SampleHeight(targetTerrain, hit.point);
                            break;
                    }
                }
            }
        }
    }

    public void SetEditValues(Terrain terrain)
    {
        targetTerrainData = GetCurrentTerrainData();
        terrainHeightMap = GetCurrentTerrainHeightMap();
        terrainHeightMapWidth = GetCurrentTerrainWidth();
        terrainHeightMapHeight = GetCurrentTerrainHeight();
    }

    public void RaiseTerrain(Terrain terrain, Vector3 location, float effectIncrement)
    {
        print(2);
        int offset = areaOfEffectSize / 2;

        Vector3 tempCoord = (location - terrain.GetPosition());
        Vector3 coord;

        coord = new Vector3
            (
            (tempCoord.x / GetTerrainSize().x),
            (tempCoord.y / GetTerrainSize().y),
            (tempCoord.z / GetTerrainSize().z)
            );

        Vector3 locationInTerrain = new Vector3(coord.x * terrainHeightMapWidth, 0, coord.z * terrainHeightMapHeight);

        int terX = (int)locationInTerrain.x - offset;

        int terZ = (int)locationInTerrain.z - offset;

        float[,] heights = targetTerrainData.GetHeights(terX, terZ, areaOfEffectSize, areaOfEffectSize);

        for (int xx = 0; xx < areaOfEffectSize; xx++)
        {
            for (int yy = 0; yy < areaOfEffectSize; yy++)
            {
                heights[xx, yy] += (effectIncrement * Time.smoothDeltaTime);
            }
        }

        targetTerrainData.SetHeights(terX, terZ, heights);
    }

    public void LowerTerrain(Terrain terrain, Vector3 location, float effectIncrement)
    {
        int offset = areaOfEffectSize / 2;

        Vector3 tempCoord = (location - terrain.GetPosition());
        Vector3 coord;

        coord = new Vector3
            (
            (tempCoord.x / GetTerrainSize().x),
            (tempCoord.y / GetTerrainSize().y),
            (tempCoord.z / GetTerrainSize().z)
            );

        Vector3 locationInTerrain = new Vector3(coord.x * terrainHeightMapWidth, 0, coord.z * terrainHeightMapHeight);

        int terX = (int)locationInTerrain.x - offset;

        int terZ = (int)locationInTerrain.z - offset;

        float[,] heights = targetTerrainData.GetHeights(terX, terZ, areaOfEffectSize, areaOfEffectSize);

        for (int xx = 0; xx < areaOfEffectSize; xx++)
        {
            for (int yy = 0; yy < areaOfEffectSize; yy++)
            {
                heights[xx, yy] -= (effectIncrement * Time.smoothDeltaTime);
            }
        }

        targetTerrainData.SetHeights(terX, terZ, heights);
    }

    public void FlattenTerrain(Terrain terrain, Vector3 location, float sampledHeight)
    {
        int offset = areaOfEffectSize / 2;

        Vector3 tempCoord = (location - terrain.GetPosition());
        Vector3 coord;

        coord = new Vector3
            (
            (tempCoord.x / GetTerrainSize().x),
            (tempCoord.y / GetTerrainSize().y),
            (tempCoord.z / GetTerrainSize().z)
            );

        Vector3 locationInTerrain = new Vector3(coord.x * terrainHeightMapWidth, 0, coord.z * terrainHeightMapHeight);

        int terX = (int)locationInTerrain.x - offset;

        int terZ = (int)locationInTerrain.z - offset;

        float[,] heights = targetTerrainData.GetHeights(terX, terZ, areaOfEffectSize, areaOfEffectSize);

        for (int xx = 0; xx < areaOfEffectSize; xx++)
        {
            for (int yy = 0; yy < areaOfEffectSize; yy++)
            {
                if (heights[xx, yy] != sampledHeight)
                {
                    heights[xx, yy] = sampledHeight;
                }
            }
        }

        targetTerrainData.SetHeights(terX, terZ, heights);
    }

    public float SampleHeight(Terrain terrain, Vector3 location)
    {
        Vector3 tempCoord = (location - terrain.GetPosition());
        Vector3 coord;

        coord = new Vector3
            (
            (tempCoord.x / GetTerrainSize().x),
            (tempCoord.y / GetTerrainSize().y),
            (tempCoord.z / GetTerrainSize().z)
            );

        Vector3 locationInTerrain = new Vector3(coord.x * terrainHeightMapWidth, 0, coord.z * terrainHeightMapHeight);

        int terX = (int)locationInTerrain.x;

        int terZ = (int)locationInTerrain.z;

        return Mathf.LerpUnclamped(0f, 1f, (terrain.terrainData.GetHeight(terX, terZ) / terrain.terrainData.size.y));
    }

    public Terrain GetTerrainAtObject(GameObject gameObject)
    {
        if (gameObject.GetComponent<Terrain>())
        {
            return gameObject.GetComponent<Terrain>();
        }

        return default(Terrain);
    }

    public Terrain GetCurrentTerrain()
    {
        if (targetTerrain)
        {
            return targetTerrain;
        }

        return default(Terrain);
    }

    public TerrainData GetCurrentTerrainData()
    {
        if (targetTerrain)
        {
            return targetTerrain.terrainData;
        }

        return default(TerrainData);
    }

    public Vector3 GetTerrainSize()
    {
        if (targetTerrain)
        {
            return targetTerrain.terrainData.size;
        }

        return Vector3.zero;
    }

    public float[,] GetCurrentTerrainHeightMap()
    {
        if (targetTerrain)
        {
            return targetTerrain.terrainData.GetHeights(0, 0, targetTerrain.terrainData.heightmapWidth, targetTerrain.terrainData.heightmapHeight);
        }

        return default(float[,]);
    }

    public int GetCurrentTerrainWidth()
    {
        if (targetTerrain)
        {
            return targetTerrain.terrainData.heightmapWidth;
        }

        return 0;
    }

    public int GetCurrentTerrainHeight()
    {
        if (targetTerrain)
        {
            return targetTerrain.terrainData.heightmapHeight;
        }

        return 0;
    }
}
