using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public int boidCount = 200;

    [SerializeField] GameObject boidPrefab;

    // boid info
    public float boidMaxSpeed = 3.0f;
    public float detectionRadius = 7f;
    public bool showRadius = false;

    // rules
    public bool separation = false;
    public float separationWeight = 4.5f;
    public float separationDistance = 3f;

    public bool cohesion = false;
    public float cohesionWeight = 15f;

    public bool alignment = false;
    public float alignmentWeight = 3f;

    public bool mouseClick = false;
    public float mouseClickWeight = 18f;
    public bool mouseAttraction = true;

    public bool border = false;
    public float borderWeight = 25f;
    public float borderSize = 1f;

    public bool wind = false;
    public float windWeight = 1f;
    public int windDirection = 90;

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
    }

    void SpawnBoid() {
        GameObject newBoid = Instantiate(boidPrefab);
        boids.Add(newBoid);
    }

    void RemoveBoid() {
        Destroy(boids[boids.Count - 1]);
        boids.RemoveAt(boids.Count - 1);
    }

    // ui
    public void boidCountSlider(float sliderValue) { boidCount = (int)sliderValue; }

    public void boidMaxSpeedSlider(float sliderValue) { boidMaxSpeed = sliderValue; }
    public void detectionRadiusSlider(float sliderValue) { detectionRadius = sliderValue; }
    public void showRadiusToggle(bool toggleValue) { showRadius = toggleValue; }
    public void separationToggle(bool toggleValue) { separation = toggleValue; }
    public void separationWeightSlider(float sliderValue) { separationWeight = sliderValue; }
    public void separationDistanceSlider(float sliderValue) { separationDistance = sliderValue; }

    public void cohesionToggle(bool toggleValue) { cohesion = toggleValue; }
    public void cohesionWeightSlider(float sliderValue) { cohesionWeight = sliderValue; }

    public void alignmentToggle(bool toggleValue) { alignment = toggleValue; }
    public void alignmentWeightSlider(float sliderValue) { alignmentWeight = sliderValue; }

    public void mouseClickToggle(bool toggleValue) { mouseClick = toggleValue; }
    public void mouseClickWeightSlider(float sliderValue) { mouseClickWeight = sliderValue; }
    public void mouseAttractionToggle(bool toggleValue) { mouseAttraction = toggleValue; }

    public void borderToggle(bool toggleValue) { border = toggleValue; }
    public void borderWeightSlider(float sliderValue) { borderWeight = sliderValue; }
    public void borderSizeSlider(float sliderValue) { borderSize = sliderValue; }

    public void windToggle(bool toggleValue) { wind = toggleValue; }
    public void windWeightSlider(float sliderValue) { windWeight = sliderValue; }
    public void windDirectionSlider(float sliderValue) { windDirection = (int)sliderValue; }
}
