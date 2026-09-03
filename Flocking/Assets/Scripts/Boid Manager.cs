using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public int boidCount = 20;

    [SerializeField] GameObject boidPrefab;

    // boid info
    public float boidSpeed = 1.0f;
    public float detectionRadius = 10f;
    public bool showRadius = false;

    // rules
    public bool separation = false;
    public bool cohesion = false;
    public bool alignment = false;

    public float separationWeight = 4.75f;
    public float cohesionWeight = 4.25f;
    public float alignmentWeight = 2.9f;

    public float separationDistance = 5f;

    public List<GameObject> boids = new List<GameObject>();
    void Start()
    {

    }

    void Update()
    {
        // make sure boid is always 1 or more
        if (boidCount < 1) { boidCount = 1; }

        // spawn the boids if needed
        int currentBoidCount = boids.Count;
        for (int i = currentBoidCount; i < boidCount; i++) {
            SpawnBoid();
        }

        // delete boids if needed
        for (int i = currentBoidCount; i > boidCount; i--) {
            RemoveBoid();
        }

        //int realBoidCount = boid.GetBoidCount();
        //if (realBoidCount < boidCount) {
        //    for (int i = realBoidCount; i < boidCount; i++) {
        //        Instantiate(boidPrefab, Vector3.zero, Quaternion.identity);
        //    }
        //    realBoidCount = boidCount;
        //}
    }

    void SpawnBoid() {
        GameObject newBoid = Instantiate(boidPrefab);
        boids.Add(newBoid);
    }

    void RemoveBoid() {
        Destroy(boids[boids.Count - 1]);
        boids.RemoveAt(boids.Count - 1);
    }
}
