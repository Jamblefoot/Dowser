//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour
{
    bool tagsAdded = false;

    [SerializeField] public bool resetTerrain = true;

    [SerializeField] float resetHeight = 0f;

    [SerializeField] Vector2 randomHeightRange = new Vector2(0, 0.1f);

    [SerializeField] Texture2D heightmapImage;
    [SerializeField] Vector3 heightmapScale = new Vector3(1, 1, 1);

    // --- PERLIN NOISE ---
    [SerializeField] public float perlinXScale = 0.01f;
    [SerializeField] public float perlinYScale = 0.01f;
    [SerializeField] public int perlinOffsetX = 0;
    [SerializeField] public int perlinOffsetY = 0;
    [SerializeField] public int perlinOctaves = 3;
    [SerializeField] public float perlinPersistence = 8;
    [SerializeField] public float perlinHeightScale = 0.09f;

    // --- MULTIPLE PERLIN ---
    [System.Serializable]
    public class PerlinParameters
    {
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistence = 8;
        public float mPerlinHeightScale = 0.09f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public bool remove = false;
    }
    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters()
    };

    // --- VORONOI ---
    [SerializeField] public int vPeakCount = 0;
    [SerializeField] public float vFalloff = 0.2f;
    [SerializeField] public float vDropoff = 0.7f;
    [SerializeField] public float vMinHeight = 0.01f;
    [SerializeField] public float vMaxHeight = 1f;
    public enum VoronoiType { Linear = 0, Power = 1, Combined = 2, SinPow = 3 }
    [SerializeField] VoronoiType voronoiType = VoronoiType.Linear;

    // --- CRATER ---
    [SerializeField] int craterCount = 1;
    [SerializeField] float craterFalloff = 0.1f;
    [SerializeField] float craterDropoff = 1.15f;
    [SerializeField] float craterDepthMin = 0.001f;
    [SerializeField] float craterDepthMax = 0.1f;

    // --- MIDPOINT DISPLACEMENT ---
    [SerializeField] public float mpdHeightMin = -5f;
    [SerializeField] public float mpdHeightMax = 5f;
    [SerializeField] public float mpdHeightDampPower = 2f;
    [SerializeField] public float mpdRoughness = 2f;

    // --- SMOOTH ---
    [SerializeField] public int smoothAmount = 1;

    // --- SPLATMAPS ---
    [System.Serializable]
    public class SplatHeights
    {
        public Texture2D texture = null;
        public Vector2 tileOffset = Vector2.zero;
        public Vector2 tileSize = new Vector2(1f, 1f);
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0f;
        public float maxSlope = 1.5f;
        public float noiseXScale = 0.01f;
        public float noiseYScale = 0.01f;
        public float noiseScaler = 0.1f;
        public float splatOffset = 0.01f;
        public bool remove = false;
    }
    public List<SplatHeights> splatHeights = new List<SplatHeights>()
    {
        new SplatHeights()
    };

    // --- VEGETATION ---
    [SerializeField] int maxTrees = 1000;
    [SerializeField] int treeSpacing = 5;
    [System.Serializable]
    public class Vegetation
    {
        public GameObject prefab;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0f;
        public float maxSlope = 90f;
        public float minScaleHeight = 0.5f;
        public float maxScaleHeight = 2f;
        public bool lockWidthToHeight = false;
        public float minScaleWidth = 0.5f;
        public float maxScaleWidth = 2f;
        public Color color1 = Color.white;
        public Color color2 = Color.white;
        public Color lightColor = Color.white;
        public float minRotation = 0;
        public float maxRotation = 360;
        public float density = 0.5f;
        public bool remove = false;
    }
    public List<Vegetation> vegetation = new List<Vegetation>() { new Vegetation() };

    // --- UNDO ---
    [SerializeField] public bool undo = false;
    float[,] lastHeightmap;

    //public Texture2D mainHeightmap;

    public Terrain terrain;
    public TerrainData terrainData;



    public float[,] GetHeightmap()
    {
        lastHeightmap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

        if (!resetTerrain)
            return terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        else return new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
    }

    public void PlantVegetation()
    {
        TreePrototype[] newTreePrototypes;
        newTreePrototypes = new TreePrototype[vegetation.Count];
        int tindex = 0;
        foreach (Vegetation t in vegetation)
        {
            newTreePrototypes[tindex] = new TreePrototype();
            newTreePrototypes[tindex].prefab = t.prefab;
            tindex++;
        }
        terrainData.treePrototypes = newTreePrototypes;

        float zScaler = terrainData.size.z / 500f;
        float xScaler = terrainData.size.x / 500f;
        List<TreeInstance> allVegetation = new List<TreeInstance>();
        for (int z = 0; z < terrainData.size.z; z += treeSpacing * Mathf.RoundToInt(zScaler))
        {
            for (int x = 0; x < terrainData.size.x; x += treeSpacing * Mathf.RoundToInt(xScaler))
            {
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
                {
                    if (UnityEngine.Random.Range(0f, 1f) > vegetation[tp].density) break;

                    Vector3 treePos = new Vector3((x + UnityEngine.Random.Range(-treeSpacing * xScaler, treeSpacing * xScaler)) / terrainData.size.x, 0f, (z + UnityEngine.Random.Range(-treeSpacing * zScaler, treeSpacing * zScaler)) / terrainData.size.z);
                    float thisHeight = terrainData.GetHeight(x, z) / terrainData.size.y;
                    //float thisHeight = terrainData.GetHeight(Mathf.RoundToInt(treePos.x),Mathf.RoundToInt(treePos.z)) / terrainData.size.y;
                    treePos.x = Mathf.Clamp(treePos.x, 0f, terrainData.size.x);
                    treePos.y = thisHeight;
                    treePos.z = Mathf.Clamp(treePos.z, 0f, terrainData.size.z);
                    float thisHeightStart = vegetation[tp].minHeight;
                    float thisHeightEnd = vegetation[tp].maxHeight;

                    float steepness = terrainData.GetSteepness(x / (float)terrainData.size.x, z / (float)terrainData.size.z);

                    if ((thisHeight > thisHeightStart && thisHeight < thisHeightEnd) && (steepness >= vegetation[tp].minSlope && steepness <= vegetation[tp].maxSlope))
                    {
                        TreeInstance instance = new TreeInstance();
                        instance.position = treePos;//new Vector3((x + UnityEngine.Random.Range(-treeSpacing * xScaler,treeSpacing * xScaler))/terrainData.size.x, thisHeight, (z + UnityEngine.Random.Range(-treeSpacing * zScaler, treeSpacing * zScaler))/terrainData.size.z);

                        Vector3 treeWorldPos = new Vector3(instance.position.x * terrainData.size.x, instance.position.y * terrainData.size.y, instance.position.z * terrainData.size.z) + transform.position;
                        RaycastHit hit;
                        int layerMask = 1 << terrainLayer;
                        if (Physics.Raycast(treeWorldPos + Vector3.up * terrainData.size.y, -Vector3.up, out hit, terrainData.size.y * 2f, layerMask))
                        {
                            float treeHeight = (hit.point.y - transform.position.y) / terrainData.size.y;
                            instance.position = new Vector3(instance.position.x, treeHeight, instance.position.z);


                            instance.rotation = UnityEngine.Random.Range(vegetation[tp].minRotation, vegetation[tp].maxRotation);
                            instance.prototypeIndex = tp;
                            instance.color = Color.Lerp(vegetation[tp].color1, vegetation[tp].color2, UnityEngine.Random.Range(0f, 1f));
                            instance.lightmapColor = vegetation[tp].lightColor;
                            instance.heightScale = UnityEngine.Random.Range(vegetation[tp].minScaleHeight, vegetation[tp].maxScaleHeight);
                            if (vegetation[tp].lockWidthToHeight)
                                instance.widthScale = instance.heightScale;
                            else instance.widthScale = UnityEngine.Random.Range(vegetation[tp].minScaleWidth, vegetation[tp].maxScaleWidth);
                            allVegetation.Add(instance);
                            if (allVegetation.Count >= maxTrees) goto TREESDONE;
                        }
                    }
                }
            }
        }
    TREESDONE:
        terrainData.treeInstances = allVegetation.ToArray();

        Debug.Log("VEGETATION SHOULD NOW BE PLANTED");
    }

    public void AddNewVegetation()
    {
        vegetation.Add(new Vegetation());
    }

    public void RemoveVegetation()
    {
        List<Vegetation> keptVegetation = new List<Vegetation>();
        for (int i = 0; i < vegetation.Count; i++)
        {
            if (!vegetation[i].remove)
            {
                keptVegetation.Add(vegetation[i]);
            }
        }
        if (keptVegetation.Count == 0)
        {
            keptVegetation.Add(vegetation[0]);
        }
        vegetation = keptVegetation;
    }


    /*public void SetMainHeightmap()
    {
        if(mainHeightmap == null)
            mainHeightmap = new Texture2D(terrainData.heightmapHeight, terrainData.heightmapWidth, TextureFormat.ARGB32, false);
        float[,] heightmap = terrainData.GetHeights(0,0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        for(int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for(int z = 0; z < terrainData.heightmapHeight; z++)
            {
                mainHeightmap.SetPixels

    }*/

    public void ResetTerrain()
    {
        float[,] heightmap;
        heightmap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightmap[x, z] = resetHeight;
            }
        }
        terrainData.SetHeights(0, 0, heightmap);
    }

    public void Undo()
    {
        if (lastHeightmap != null)
            terrainData.SetHeights(0, 0, lastHeightmap);
    }

    public void RandomTerrain()
    {
        //float[,] heightmap;
        //heightmap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        float[,] heightmap = GetHeightmap();//terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightmap[x, z] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0, 0, heightmap);
    }

    public void SetTextureToLoad(Texture2D texToLoad)
    {
        heightmapImage = texToLoad;
    }

    public void LoadTexture()
    {
        float[,] heightmap = GetHeightmap();
        //heightmap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];

        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                //heightmap[x,z] += heightmapImage.GetPixel(terrainData.heightmapWidth - (int)(x * heightmapScale.x), (int)(z * heightmapScale.z)).grayscale * heightmapScale.y;
                heightmap[x, z] = heightmapImage.GetPixel(terrainData.heightmapWidth - (int)(x * heightmapScale.x), (int)(z * heightmapScale.z)).grayscale * heightmapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightmap);
    }

    //THIS IS A MARK ADD
    public Texture2D CopyHeightmap()
    {
        Texture2D copy = new Texture2D(terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight, TextureFormat.ARGB32, false);
        float[,] heightmap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        for (int x = 0; x < terrain.terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrain.terrainData.heightmapHeight; z++)
            {
                copy.SetPixel(terrainData.heightmapWidth - x, z, new Color(heightmap[x, z], heightmap[x, z], heightmap[x, z], 1));
            }
        }
        copy.Apply();
        return copy;
    }

    public void Perlin()
    {
        float[,] heightmap = GetHeightmap();//terrainData.GetHeights(0,0, terrainData.heightmapWidth, terrainData.heightmapHeight);

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                //heightmap[x,y] = Mathf.PerlinNoise((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY) * perlinYScale);
                heightmap[x, y] += Utils.fBM((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY) * perlinYScale, perlinOctaves, perlinPersistence) * perlinHeightScale;
            }
        }

        terrainData.SetHeights(0, 0, heightmap);
    }

    public void MultiplePerlinTerrain()
    {
        float[,] heightmap = GetHeightmap();
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                foreach (PerlinParameters p in perlinParameters)
                {
                    heightmap[x, y] += Utils.fBM((x + p.mPerlinOffsetX) * p.mPerlinXScale, (y + p.mPerlinOffsetY) * p.mPerlinYScale, p.mPerlinOctaves, p.mPerlinPersistence) * p.mPerlinHeightScale;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightmap);
    }

    public void AddNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }
    public void RemovePerlin()
    {
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();
        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if (!perlinParameters[i].remove)
            {
                keptPerlinParameters.Add(perlinParameters[i]);
            }
        }
        if (keptPerlinParameters.Count == 0) //if don't want any
        {
            keptPerlinParameters.Add(perlinParameters[0]); //need 1 blank for display purposes
        }
        perlinParameters = keptPerlinParameters;
    }

    public void Voronoi()
    {
        float[,] heightmap = GetHeightmap();
        for (int p = 0; p < vPeakCount; p++)
        {
            Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapWidth), UnityEngine.Random.Range(vMinHeight, vMaxHeight), UnityEngine.Random.Range(0, terrainData.heightmapHeight));

            if (heightmap[(int)peak.x, (int)peak.z] < peak.y)
                heightmap[(int)peak.x, (int)peak.z] = peak.y;
            else
                continue;

            Vector2 peakLocation = new Vector2(peak.x, peak.z);
            float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapWidth, terrainData.heightmapHeight));
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    if (!(x == peak.x && y == peak.z))
                    {
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                        float h = 0f;
                        switch (voronoiType)
                        {
                            case VoronoiType.Linear:
                                h = peak.y - distanceToPeak * vFalloff;
                                break;
                            case VoronoiType.Power:
                                h = peak.y - Mathf.Pow(distanceToPeak, vDropoff) * vFalloff;
                                break;
                            case VoronoiType.Combined:
                                h = peak.y - distanceToPeak * vFalloff - Mathf.Pow(distanceToPeak, vDropoff);
                                break;
                            case VoronoiType.SinPow:
                                h = peak.y - Mathf.Pow(distanceToPeak * 3, vFalloff) - Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / vDropoff;
                                break;
                        }
                        //float h = peak.y - distanceToPeak * vFalloff - Mathf.Pow(distanceToPeak, vDropoff); //combined
                        //float h = peak.y - Mathf.Pow(distanceToPeak, vDropoff) * vFalloff; //power
                        //float h = peak.y - distanceToPeak * vFalloff; //linear
                        //float h = peak.y - Mathf.Sin(distanceToPeak * 100) * 0.1f;
                        if (heightmap[x, y] < h)
                            heightmap[x, y] = h;
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, heightmap);
    }

    public void Crater()
    {
        float[,] heightmap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        for (int c = 0; c < craterCount; c++)
        {
            Vector2 hitPoint = new Vector2(UnityEngine.Random.Range(0, terrainData.heightmapWidth), UnityEngine.Random.Range(0, terrainData.heightmapHeight));
            float depth = UnityEngine.Random.Range(craterDepthMin, craterDepthMax);
            float craterDepth = heightmap[(int)hitPoint.x, (int)hitPoint.y] - depth;
            heightmap[(int)hitPoint.x, (int)hitPoint.y] = craterDepth;
            float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapWidth, terrainData.heightmapHeight));
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    float distanceToPeak = Vector2.Distance(hitPoint, new Vector2(x, y)) / maxDistance;
                    if (distanceToPeak < depth * 0.025f)
                    {
                        float h = craterDepth + depth * craterFalloff - distanceToPeak * (craterFalloff) - Mathf.Pow(distanceToPeak, craterDropoff);
                        heightmap[x, y] = h;
                    }
                    else if (distanceToPeak < depth * craterDropoff)
                    {
                        float h = craterDepth + Mathf.Pow(distanceToPeak, craterDropoff);
                        if (heightmap[x, y] > h)
                            heightmap[x, y] = h;
                    }
                    else if (distanceToPeak < depth * craterDropoff + craterFalloff * depth)
                    {
                        float h = heightmap[x, y] + Mathf.Pow(distanceToPeak - depth * craterDropoff, craterDropoff);
                        if (heightmap[x, y] < h)
                            heightmap[x, y] = h;
                    }
                }
            }
        }
        terrainData.SetHeights(0, 0, heightmap);
    }

    public void MidPointDisplacement()
    {
        float[,] heightmap = GetHeightmap();
        int width = terrainData.heightmapWidth - 1;
        int squareSize = width;
        //float height = (float)squareSize / 2.0f * 0.01f;
        float heightMin = mpdHeightMin;
        float heightMax = mpdHeightMax;
        float roughness = mpdRoughness;
        float heightDampener = (float)Mathf.Pow(mpdHeightDampPower, -1 * mpdRoughness);

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        /*heightmap[0,0] = UnityEngine.Random.Range(0f,0.2f);
        heightmap[0, terrainData.heightmapHeight - 2] = UnityEngine.Random.Range(0f, 0.2f);
        heightmap[terrainData.heightmapWidth - 2, 0] = UnityEngine.Random.Range(0f, 0.2f);
        heightmap[terrainData.heightmapWidth - 2, terrainData.heightmapHeight - 2] = UnityEngine.Random.Range(0f, 0.2f);*/

        while (squareSize > 0)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2f);
                    midY = (int)(y + squareSize / 2f);

                    heightmap[midX, midY] = (float)((heightmap[x, y] + heightmap[cornerX, y] + heightmap[x, cornerY] + heightmap[cornerX, cornerY]) / 4.0f + UnityEngine.Random.Range(heightMin, heightMax));
                }
            }

            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2f);
                    midY = (int)(y + squareSize / 2f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    if (pmidXL <= 0 || pmidYD <= 0 || pmidXR >= width - 1 || pmidYU >= width - 1) continue;

                    //calculate the square value for the bottom side
                    heightmap[midX, y] = (float)((heightmap[midX, midY] + heightmap[x, y] + heightmap[midX, pmidYD] + heightmap[cornerX, y]) / 4f + UnityEngine.Random.Range(heightMin, heightMax));
                    //calculate the square value for the left side
                    heightmap[x, midY] = (float)((heightmap[x, cornerY] + heightmap[pmidXL, midY] + heightmap[x, y] + heightmap[midX, midY]) / 4f + UnityEngine.Random.Range(heightMin, heightMax));
                    //calculate the square value for the top side
                    heightmap[midX, cornerY] = (float)((heightmap[midX, pmidYU] + heightmap[x, cornerY] + heightmap[midX, midY] + heightmap[cornerX, cornerY]) / 4f + UnityEngine.Random.Range(heightMin, heightMax));
                    //calculate the square value for the right side
                    heightmap[cornerX, midY] = (float)((heightmap[cornerX, cornerY] + heightmap[midX, midY] + heightmap[cornerX, y] + heightmap[pmidXR, midY]) / 4f + UnityEngine.Random.Range(heightMin, heightMax));
                }
            }

            squareSize = (int)(squareSize / 2f);
            heightMin *= heightDampener;
            heightMax *= heightDampener;
        }
        terrainData.SetHeights(0, 0, heightmap);
    }

    public void Smooth()
    {
        float[,] heightmap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);//GetHeightmap();
        float smoothProgress = 0;
#if UNITY_EDITOR
        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress);
#endif

        for (int s = 0; s < smoothAmount; s++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    float avgHeight = heightmap[x, y];
                    List<Vector2> neighbors = GenerateNeighbors(new Vector2(x, y), terrainData.heightmapWidth, terrainData.heightmapHeight);
                    foreach (Vector2 neigh in neighbors)
                    {
                        avgHeight += heightmap[(int)neigh.x, (int)neigh.y];
                    }

                    heightmap[x, y] = avgHeight / ((float)neighbors.Count + 1);
                }
            }
            smoothProgress++;
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress / smoothAmount);
#endif
        }
        terrainData.SetHeights(0, 0, heightmap);
#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
#endif
    }
    List<Vector2> GenerateNeighbors(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbors = new List<Vector2>();
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (!(x == 0 && y == 0))
                {
                    Vector2 nPos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1), Mathf.Clamp(pos.y + y, 0, height - 1));
                    if (!neighbors.Contains(nPos))
                        neighbors.Add(nPos);
                }
            }
        }
        return neighbors;
    }

    public void AddNewSplatHeight()
    {
        splatHeights.Add(new SplatHeights());
    }
    public void RemoveSplatHeight()
    {
        List<SplatHeights> keptSplatHeights = new List<SplatHeights>();
        for (int i = 0; i < splatHeights.Count; i++)
        {
            if (!splatHeights[i].remove)
            {
                keptSplatHeights.Add(splatHeights[i]);
            }
        }
        if (keptSplatHeights.Count == 0)
        {
            keptSplatHeights.Add(splatHeights[0]);
        }
        splatHeights = keptSplatHeights;
    }

    float GetSteepness(float[,] heightmap, int x, int y, int width, int height)
    {
        float h = heightmap[x, y];
        int nx = x + 1;
        int ny = y + 1;

        //if on the upper edge of the map find gradient by going backward
        if (nx > width - 1) nx = x - 1;
        if (ny > height - 1) ny = y - 1;

        float dx = heightmap[nx, y] - h;
        float dy = heightmap[x, ny] - h;
        Vector2 gradient = new Vector2(dx, dy);

        float steep = gradient.magnitude;

        return steep;
    }

    public void SplatMaps()
    {
#if UNITY_EDITOR
        TerrainLayer[] newSplatPrototypes;
        newSplatPrototypes = new TerrainLayer[splatHeights.Count];
        int spindex = 0;
        foreach (SplatHeights sh in splatHeights)
        {
            newSplatPrototypes[spindex] = new TerrainLayer();
            newSplatPrototypes[spindex].diffuseTexture = sh.texture;
            newSplatPrototypes[spindex].tileOffset = sh.tileOffset;
            newSplatPrototypes[spindex].tileSize = sh.tileSize;
            newSplatPrototypes[spindex].diffuseTexture.Apply(true);
            string path = "Assets/New Terrain Layer " + spindex + ".terrainLayer";
            AssetDatabase.CreateAsset(newSplatPrototypes[spindex], path);
            spindex++;
            Selection.activeObject = this.gameObject;
        }
        terrainData.terrainLayers = newSplatPrototypes;

        float[,] heightmap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float[] splat = new float[terrainData.alphamapLayers];
                for (int i = 0; i < splatHeights.Count; i++)
                {
                    float noise = Mathf.PerlinNoise(x * splatHeights[i].noiseXScale, y * splatHeights[i].noiseYScale) * splatHeights[i].noiseScaler;
                    float offset = splatHeights[i].splatOffset + noise;
                    float thisHeightStart = splatHeights[i].minHeight - offset;
                    float thisHeightStop = splatHeights[i].maxHeight + offset;
                    //float steepness = GetSteepness(heightmap, x, y, terrainData.heightmapWidth, terrainData.heightmapHeight);
                    float steepness = terrainData.GetSteepness(y / (float)terrainData.alphamapHeight, x / (float)terrainData.alphamapWidth);
                    if ((heightmap[x, y] >= thisHeightStart && heightmap[x, y] <= thisHeightStop) && (steepness >= splatHeights[i].minSlope && steepness <= splatHeights[i].maxSlope))
                    {
                        splat[i] = 1;
                    }
                }
                NormalizeVector(splat);
                for (int j = 0; j < splatHeights.Count; j++)
                {
                    splatmapData[x, y, j] = splat[j];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
#endif
    }
    void NormalizeVector(float[] v)
    {
        float total = 0;
        for (int i = 0; i < v.Length; i++)
        {
            total += v[i];
        }

        for (int i = 0; i < v.Length; i++)
        {
            v[i] /= total;
        }
    }



    void OnEnable()
    {
        Debug.Log("Initializing Terrain Data");
        terrain = this.GetComponent<Terrain>();
        //terrainData = Terrain.activeTerrain.terrainData;
        //terrainData = terrain.terrainData;
    }

    public enum TagType { Tag = 0, Layer = 1 }
    [SerializeField] int terrainLayer = -1;
    void Awake()
    {
#if UNITY_EDITOR
        SerializedObject tagManager = new SerializedObject(
                            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Terrain", TagType.Tag);
        AddTag(tagsProp, "Cloud", TagType.Tag);
        AddTag(tagsProp, "Shore", TagType.Tag);
        tagManager.ApplyModifiedProperties();

        //apply tag changes to tag database
        SerializedProperty layerProp = tagManager.FindProperty("layers");
        terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
        tagManager.ApplyModifiedProperties();
#endif

        //take this object
        this.gameObject.tag = "Terrain";
        this.gameObject.layer = terrainLayer;
    }
#if UNITY_EDITOR
    void AddTags()
    {
        SerializedObject tagManager = new SerializedObject(
                            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Terrain", TagType.Tag);
        AddTag(tagsProp, "Cloud", TagType.Tag);
        AddTag(tagsProp, "Shore", TagType.Tag);

        //apply tag changes to tag database
        tagManager.ApplyModifiedProperties();

        //take this object
        this.gameObject.tag = "Terrain";
    }
    int AddTag(SerializedProperty tagsProp, string newTag, TagType tType)
    {
        bool found = false;
        //ensure the tag doesn't already exist
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag))
            {
                found = true; return i;
            }
        }
        if (!found && tType == TagType.Tag)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
        else if (!found && tType == TagType.Layer)
        {
            for (int j = 8; j < tagsProp.arraySize; j++)
            {
                SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);
                //add layer in next empty slot
                if (newLayer.stringValue == "")
                {
                    newLayer.stringValue = newTag;
                    return j;
                }
            }
        }

        return -1;
    }
#endif

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (!tagsAdded) { AddTags(); tagsAdded = true; }
#endif


        if (terrainLayer == -1)
        {
            terrainLayer = gameObject.layer;
        }

        terrainData = terrain.terrainData;
    }


}

