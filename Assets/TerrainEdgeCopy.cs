using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class TerrainEdgeCopy : MonoBehaviour
{
    public Terrain targetTerrain;
    public enum Edge {Top, Bottom, Left, Right};
    public Edge thisEdge = Edge.Bottom;
    public Edge targetEdge = Edge.Top;

    Terrain thisTerrain;
    TerrainData thisData;
    TerrainData targetData;

    public bool activeMatch = false;
    public bool match = false;

    public float easeRange = 50;

    void OnValidate()
    {
        if(!match && !activeMatch) return;

        match = false;

        if(thisTerrain == null) 
            thisTerrain = GetComponent<Terrain>();
        if(thisTerrain == null || targetTerrain == null) return;

        if(targetData != targetTerrain.terrainData)
        {
            targetData = targetTerrain.terrainData;
            thisData = thisTerrain.terrainData;
        }

        int res = thisData.heightmapResolution;
        float[,] thisMap = thisData.GetHeights(0, 0, res, res);
        float[,] targetMap = targetData.GetHeights(0, 0, res, res);
        //float[] thisSide = new float[thisData.heightmapResolution];

        for(int x = 0; x < res; x++)
        {
            for(int z = 0; z < res; z++)
            {
                float targetHeight = 0;//thisMap[x, z];
                if (targetEdge == Edge.Left)
                    targetHeight = targetMap[x, 0];
                else if (targetEdge == Edge.Right)
                    targetHeight = targetMap[x, res - 1];
                else if (targetEdge == Edge.Bottom)
                    targetHeight = targetMap[0, z];
                else if(targetEdge == Edge.Top)
                    targetHeight = targetMap[res - 1, z];

                if(thisEdge == Edge.Left)
                {
                    if(z > easeRange) continue;

                    thisMap[x, z] = Mathf.Lerp(thisMap[x, z], targetHeight, (easeRange - z) / easeRange);
                }
                else if(thisEdge == Edge.Right)
                {
                    if(z < res - 1 - easeRange) continue;

                    thisMap[x, z] = Mathf.Lerp(thisMap[x,z], targetHeight, (easeRange - (res - 1 - z))/ easeRange);
                }
                else if(thisEdge == Edge.Bottom)
                {
                    if(x > easeRange) continue;

                    thisMap[x, z] = Mathf.Lerp(thisMap[x, z], targetHeight, (easeRange - x) / easeRange);
                }
                else if(thisEdge == Edge.Top)
                {
                    if(x < res - 1 - easeRange) continue;

                    thisMap[x, z] = Mathf.Lerp(thisMap[x, z], targetHeight, (easeRange - (res - 1 - x))/ easeRange);

                    //Debug.Log("easeRange lerp value is "+ ((easeRange - (res - 1 - x)) / easeRange).ToString());
                }
            }
        }

        thisData.SetHeights(0,0,thisMap);

        //Debug.Log("Terrain should be matching!");
        
    }
}
