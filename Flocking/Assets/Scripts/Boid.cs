using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class Boid : MonoBehaviour {
    SpriteRenderer sprite;

    public Vector3 velocityNorm;

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
        velocityNorm = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

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

            ProcessRules(boidManager.detectionRadius, 1 << 0);

        // look at velocity pos
        Vector3 direction = (transform.position + velocityNorm) - transform.position;
        transform.up = direction;

        // move
        transform.position += velocityNorm * boidManager.boidSpeed * Time.deltaTime;

    }

    public Vector3 getVelocityNorm() {
        return velocityNorm;
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

    public void GetNeighbors() {
        // go through each in boidmanager's list
        // if it's in range, add it to list,
        // otherwise, dont
        neighbors.Clear();
        foreach (GameObject neighbor in boidManager.boids) {
            // check it it is near border
            // if (transform.position.x >= xBound - boidManager.)
            if (neighbor == this.gameObject) continue;

            float distance = Vector3.Distance(transform.position, neighbor.transform.position);
            if (distance <= boidManager.detectionRadius * 0.1f) {
                neighbors.Add(neighbor);
            }
        }
    }
    public void ProcessRules(float detectionRadius, LayerMask unitLayer) {
        // get neighbors
        GetNeighbors();

        Vector3 separationVelocity = Vector3.zero;
        Vector3 cohesionVelocity = Vector3.zero;
        Vector3 alignmentVelocity = Vector3.zero;

        Vector3 newVelocity;

        // process rules for neighbors
        if (boidManager.separation) {
            // here: returns a velocity
            separationVelocity = Separation();
        }
        if (boidManager.cohesion) {
            cohesionVelocity = Cohesion();
        }
        if (boidManager.alignment) {
            alignmentVelocity = Alignment();
        }

        newVelocity = alignmentVelocity * boidManager.alignmentWeight 
                    + cohesionVelocity * boidManager.cohesionWeight 
                    + separationVelocity * boidManager.separationWeight;
        newVelocity.Normalize();

        if (newVelocity == Vector3.zero) {
            return;
        }
        velocityNorm = newVelocity;

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
            float distanceMagSqr = distanceVec.sqrMagnitude;

            // skip if too far away
            if (distanceMagSqr > (boidManager.separationDistance * 0.1f) * (boidManager.separationDistance * 0.1f) || distanceMagSqr < 0.0001f) {
                continue;
            }

            float dist = Mathf.Sqrt(distanceMagSqr);

            Vector3 pushDirection = distanceVec / dist;
            separationVelocity += pushDirection / dist;
        }
        separationVelocity.Normalize();
        return separationVelocity;
    }
    Vector3 Cohesion() {
        Vector3 cohesionVelocity;
        Vector3 totalPos = Vector3.zero;
        int neighborCount = 0;
        foreach (GameObject neighbor in neighbors) {
            // skip self-detection
            if (neighbor.gameObject == gameObject) {
                continue;
            }

            // process neighbor boids
            totalPos += neighbor.transform.position;
            neighborCount++;
        }
        if (neighborCount == 0) {
            return Vector3.zero;
        }

        Vector3 averagePosition = totalPos / neighborCount;
        cohesionVelocity = averagePosition - transform.position;
        cohesionVelocity.Normalize();

        return cohesionVelocity;
    }
    Vector3 Alignment() {
        Vector3 alignmentVelocity = velocityNorm;
        int neighborCount = 1;
        foreach (GameObject neighbor in neighbors) {
            // skip self-detection
            if (neighbor.gameObject == gameObject) {
                continue;
            }

            // process neighbor boids
            if (neighbor.TryGetComponent<Boid>(out Boid boidScript)) {
                alignmentVelocity += boidScript.velocityNorm;
                neighborCount++;
            }
        }
        alignmentVelocity /= neighborCount;
        alignmentVelocity.Normalize();

        return alignmentVelocity;
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
}
