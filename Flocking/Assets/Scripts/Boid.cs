using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {
    SpriteRenderer sprite;

    public Vector3 velocityCur;
    public Vector3 velocityNew;

    float xBound = 9f;
    float yBound = 5f;

    BoidManager boidManager;

    private LineRenderer line;

    public List<GameObject> neighbors = new List<GameObject>();

    void Awake() {
        // set color
        SetColor();

        // set posistion
        transform.position = new Vector3(Random.Range((float)-xBound, (float)xBound), Random.Range((float)-yBound, (float)yBound), 0);

        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        velocityCur = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

        line = GetComponent<LineRenderer>();
        line.positionCount = 100;
    }

    private void Start() {
        boidManager = Object.FindAnyObjectByType<BoidManager>();
    }

    void Update() {
        // wrap it around if it goes out of bounds
        if (transform.position.x > xBound) {
            transform.position = new Vector3(-xBound, transform.position.y, transform.position.z);
        } else if (transform.position.x < -xBound) {
            transform.position = new Vector3(xBound, transform.position.y, transform.position.z);
        } else if (transform.position.y > yBound) {
            transform.position = new Vector3(transform.position.x, -yBound, transform.position.z);
        } else if (transform.position.y < -yBound) {
            transform.position = new Vector3(transform.position.x, yBound, transform.position.z);
        }

        // show radius
        if (boidManager.showRadius) {
            line.enabled = true;
            DrawRadius();
        } else {
            line.enabled = false;
        }

        ProcessRules();

        // look at velocity pos
        Vector3 direction = (transform.position + velocityCur) - transform.position;
        transform.up = direction;

    }

    private void LateUpdate() {
        // move
        transform.position += velocityCur * Time.deltaTime;
    }

    public Vector3 getVelocityCur() {
        return velocityCur;
    }

    void SetColor() {
        float colorR = 0f;
        float colorG = 0f;
        float colorB = 0f;
        sprite = GetComponentInChildren<SpriteRenderer>();
        switch (Random.Range(0, 6)) {
            case 0:
                colorR = 1f;
                colorG = Random.Range(0, 1f);
                break;
            case 1:
                colorR = 1f;
                colorG = Random.Range(0, 1f);
                break;
            case 2:
                colorG = 1f;
                colorR = Random.Range(0, 1f);
                break;
            case 3:
                colorG = 1f;
                colorB = Random.Range(0, 1f);
                break;
            case 4:
                colorB = 1f;
                colorR = Random.Range(0, 1f);
                break;
            case 5:
                colorB = 1f;
                colorG = Random.Range(0, 1f);
                break;
        }
        sprite.color = new Color(colorR, colorG, colorB, 1f);
    }
    void DrawRadius() {
        int drawingSegments = 100;
        float angleStep = 2f * Mathf.PI / drawingSegments;

        for (int i = 0; i < drawingSegments; i++) {
            float angle = i * angleStep;
            float x = Mathf.Cos(angle) * (boidManager.detectionRadius * 0.1f);
            float y = Mathf.Sin(angle) * (boidManager.detectionRadius * 0.1f);

            line.SetPosition(i, new Vector3(x + transform.position.x, y + transform.position.y, 0));
        }
    }

    public void GetNeighbors() {
        // go through each in boidmanager's list
        // if it's in range, add it to list,
        // otherwise, dont
        neighbors.Clear();
        foreach (GameObject neighbor in boidManager.boids) {
            // check it it is near border
            // ^ im not doing this but if i had to i could
            if (neighbor == this.gameObject) continue;

            float distance = Vector3.Distance(transform.position, neighbor.transform.position);
            if (distance <= boidManager.detectionRadius * 0.1f) {
                neighbors.Add(neighbor);
            }
        }
    }
    public void ProcessRules() {
        // get neighbors
        GetNeighbors();

        Vector3 separationVelocity = Vector3.zero;
        Vector3 cohesionVelocity = Vector3.zero;
        Vector3 alignmentVelocity = Vector3.zero;
        Vector3 mouseClickVelocity = Vector3.zero;
        Vector3 borderVelocity = Vector3.zero;
        Vector3 windVelocity = Vector3.zero;

        Vector3 totalForce;

        // process rules for neighbors
        if (boidManager.separation) {
            separationVelocity = Separation();
        }
        if (boidManager.cohesion) {
            cohesionVelocity = Cohesion();
        }
        if (boidManager.alignment) {
            alignmentVelocity = Alignment();
        }
        if (boidManager.mouseClick) {
            mouseClickVelocity = MouseClick();
        }
        if (boidManager.border) {
            borderVelocity = Border();
        }
        if (boidManager.wind) {
            windVelocity = Wind();
        }

        totalForce = separationVelocity * boidManager.separationWeight
                    + cohesionVelocity * boidManager.cohesionWeight
                    + alignmentVelocity * boidManager.alignmentWeight 
                    + mouseClickVelocity * boidManager.mouseClickWeight 
                    + borderVelocity * boidManager.borderWeight 
                    + windVelocity * boidManager.windWeight;

        if (totalForce.sqrMagnitude > 0.0001f) {
            velocityCur += totalForce * Time.deltaTime;
        }

        // keeps it below max speed
        velocityCur = Vector3.ClampMagnitude(velocityCur, boidManager.boidMaxSpeed);

    }
    Vector3 Separation() {
        Vector3 separationVelocity = Vector3.zero;
        foreach (GameObject neighbor in neighbors) {
            // skip self-detection
            if (neighbor.gameObject == gameObject) {
                continue;
            }

            // process neighbor boids
            Vector3 distanceVec = transform.position - neighbor.transform.position;
            float distance = distanceVec.magnitude;

            // skip if too far away or too close
            if (distance > (boidManager.separationDistance * 0.1f) || distance < 0.0001f) {
                continue;
            }

            separationVelocity += distanceVec.normalized / distance;
        }
        //separationVelocity.Normalize();
        return separationVelocity;
    }
    Vector3 Cohesion() {
        Vector3 cohesionVelocity;
        Vector3 centerOfMass = Vector3.zero;
        int neighborCount = 0;
        foreach (GameObject neighbor in neighbors) {
            // skip self-detection
            if (neighbor.gameObject == gameObject) {
                continue;
            }

            // process neighbor boids
            centerOfMass += neighbor.transform.position;
            neighborCount++;
        }
        if (neighborCount == 0) {
            return Vector3.zero;
        }

        centerOfMass /= neighborCount;
        cohesionVelocity = centerOfMass - transform.position;
        
        // stops the drift
        if (cohesionVelocity.magnitude < 0.0001) {
            return Vector3.zero;
        }
        cohesionVelocity.Normalize();

        return cohesionVelocity;
    }
    Vector3 Alignment() {
        Vector3 alignmentVelocity = velocityCur;
        int neighborCount = 1;
        foreach (GameObject neighbor in neighbors) {
            // skip self-detection
            if (neighbor.gameObject == gameObject) {
                continue;
            }

            // process neighbor boids
            if (neighbor.TryGetComponent<Boid>(out Boid boidScript)) {
                alignmentVelocity += boidScript.velocityCur;
                neighborCount++;
            }
        }
        alignmentVelocity /= neighborCount;
        //alignmentVelocity.Normalize();

        return alignmentVelocity;
    }
    Vector3 MouseClick() {
        Vector3 mouseClickVelocity = Vector3.zero;
        if (Input.GetMouseButton(0))
        {
            Vector3 screenPos = Input.mousePosition;
            screenPos.z = 10f;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;

            mouseClickVelocity = worldPos - transform.position;
        }
        if (!boidManager.mouseAttraction) {
            mouseClickVelocity = -mouseClickVelocity;
        }

        mouseClickVelocity.Normalize();

        return mouseClickVelocity;
    }
    Vector3 Border() {
        Vector3 borderVelocity = Vector3.zero;
        if (transform.position.x >= xBound - boidManager.borderSize) {
            borderVelocity.x -= 1f;
        }
        if (transform.position.x <= -xBound + boidManager.borderSize) {
            borderVelocity.x += 1f;
        }
        if (transform.position.y >= yBound - boidManager.borderSize) {
            borderVelocity.y -= 1f;
        }
        if (transform.position.y <= -yBound + boidManager.borderSize) {
            borderVelocity.y += 1f;
        }
        borderVelocity.Normalize();
        return borderVelocity;
    }
    Vector3 Wind() {
        Vector3 windVelocity = Vector3.zero;
        float degreesRad = Mathf.Deg2Rad * (float)boidManager.windDirection;
        windVelocity = new Vector3(Mathf.Sin(degreesRad), -Mathf.Cos(degreesRad), 0f);
        return windVelocity;
    }
}
