using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Color[] colors;
    Vector3[] normals;

    public int xSize = 20;
    public int zSize = 20;
    public float scale = 0.5f;

    public Gradient gradient;

    private float minTerrainHeight;
    private float maxTerrainHeight;

    public List<Vector3> spawnPoints = new List<Vector3>();
    public GameObject grassPrefab, TreePrefab, Tree2Prefab, towerPrefab, batteryPrefab;

    public int grassMaxHeight;

    public int maxSpawnedPrefabs;
    private int spawnedPrefabs;
    private int towers = 0;
    public int maxTowers;

    private int batteries = 0;
    public int maxBatteries;
    public GameObject gameEndingPrefab;

    void Start()
    {
        spawnedPrefabs = 0;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        createShape();
        updateMesh();

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        spawnPrefabs();

        // Calculate the index of the vertex at (65, y, 65)
        int vertCountX = Mathf.FloorToInt(xSize / scale);
        int vertCountZ = Mathf.FloorToInt(zSize / scale);
        int xIndex = Mathf.FloorToInt(65 / scale);
        int zIndex = Mathf.FloorToInt(65 / scale);
        int vertexIndex = zIndex * (vertCountX + 1) + xIndex;

        // Get the y value of the vertex at (65, y, 65)
        float yValue = vertices[vertexIndex].y + 1.5f;

        // Set the endSpawn position
        Vector3 endSpawn = new Vector3(65, yValue, 65);
        Instantiate(gameEndingPrefab, endSpawn, Quaternion.identity);
    } 

    void createShape()
    {
        int vertCountX = Mathf.FloorToInt(xSize / scale);
        int vertCountZ = Mathf.FloorToInt(zSize / scale);

        vertices = new Vector3[(vertCountX + 1) * (vertCountZ + 1)];

        System.Random random = new System.Random();
        float baseRandX = (float)random.NextDouble() * 100f;
        float baseRandZ = (float)random.NextDouble() * 100f;

        for (int i = 0, z = 0; z <= vertCountZ; z++)
        {
            for (int x = 0; x <= vertCountX; x++)
            {
                float offsetX = (float)random.NextDouble() * 0.1f;
                float offsetZ = (float)random.NextDouble() * 0.1f;

                float y = Mathf.PerlinNoise((x * 0.3f * scale) + baseRandX + offsetX, (z * 0.3f * scale) + baseRandZ + offsetZ) * 1.5f;
                y += Mathf.PerlinNoise((x * 0.1f * scale) + baseRandX + offsetX, (z * 0.1f * scale) + baseRandZ + offsetZ) * 3.5f;
                y -= Mathf.PerlinNoise((x * 0.05f * scale) + baseRandX + offsetX, (z * 0.05f * scale) + baseRandZ + offsetZ) * 2.25f;
                y *= 2.6f;

                if (y > maxTerrainHeight)
                {
                    maxTerrainHeight = y;
                }
                if (y < minTerrainHeight)
                {
                    minTerrainHeight = y;
                }

                vertices[i] = new Vector3(x * scale, y, z * scale);

                i++;
            }
        }

        triangles = new int[vertCountX * vertCountZ * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < vertCountZ; z++)
        {
            for (int x = 0; x < vertCountX; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + vertCountX + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + vertCountX + 1;
                triangles[tris + 5] = vert + vertCountX + 2;

                vert++;
                tris += 6;
            }
            vert += 1;
        }

        colors = new Color[vertices.Length];

        for (int i = 0, z = 0; z <= vertCountZ; z++)
        {
            for (int x = 0; x <= vertCountX; x++)
            {
                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y);
                colors[i] = gradient.Evaluate(height);
                i++;
            }
        }
    }

    void updateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        normals = mesh.normals;

        List<Vector3> vertexes = vertices.ToList();
        for (int i = 0; i < vertexes.Count; i++)
        {
            if (vertexes[i].y >= 2 && vertexes[i].y < grassMaxHeight)
            {
                spawnPoints.Add(vertexes[i]);
            }
        }
    }

    void spawnPrefabs()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            Vector3 position = spawnPoints[i];
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normals[i]);

            Transform[] children = grassPrefab.GetComponentsInChildren<Transform>(true);

            List<Transform> childList = new List<Transform>();
            foreach (Transform child in children)
            {
                if (child != grassPrefab.transform)
                {
                    childList.Add(child);
                }
            }

            int randomIndex = Random.Range(0, childList.Count);
            Transform selectedChild = childList[randomIndex];

            selectedChild.gameObject.SetActive(true);
            Vector3 pos = position;
            pos.y -= .05f;
            Instantiate(selectedChild.gameObject, pos, rotation);

            if (spawnedPrefabs < maxSpawnedPrefabs)
            {
                if (spawnChance(TreePrefab, 2f, position))
                {
                    spawnedPrefabs++;
                }
                if (spawnChance(Tree2Prefab, 2f, position))
                {
                    spawnedPrefabs++;
                }
                if (towers <= maxTowers)
                {
                    if (spawnChance(towerPrefab, .00000005f, position))
                    {
                        towers++;
                        spawnedPrefabs++;
                    }
                }
                if (batteries <= maxBatteries)
                {
                    if (spawnChance(batteryPrefab, 2f, position))
                    {
                        batteries++;
                        spawnedPrefabs++;
                    }
                }

            }
        }
    }

    bool spawnChance(GameObject prefab, float chance, Vector3 spawnPoint)
    {
        float rand = Random.Range(0, 1800);
        if (rand <= chance)
        {
            if (prefab == batteryPrefab)
            {
                spawnPoint.y += .1f;
            } else 
            {
                spawnPoint.y -= 1f;
            }
            Instantiate(prefab, spawnPoint, Quaternion.identity);
            return true;
        }
        return false;
    }
}
