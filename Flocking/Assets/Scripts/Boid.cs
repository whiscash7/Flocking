using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Boid : MonoBehaviour
{
    SpriteRenderer sprite;

    Rigidbody2D rb;
    float speed = 4f;

    float xBound = 9f;
    float yBound = 5f;

    void Awake() {
        // set color
        sprite = GetComponentInChildren<SpriteRenderer>();
        sprite.color = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 1f);

        // set posistion
        transform.position = new Vector3(Random.Range((float)-xBound, (float)xBound), Random.Range((float)-yBound, (float)yBound), 0);
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-180, 180));

        // get rigidbody
        rb = GetComponent<Rigidbody2D>();
    }

    void Update() {
        // wrap it around if it goes out of bounds
        if (transform.position.x > xBound) {
            transform.position = new Vector3(-xBound, transform.position.y, transform.position.z);
        }
        else if (transform.position.x < -xBound) {
            transform.position = new Vector3(xBound, transform.position.y, transform.position.z);
        }
        else if (transform.position.y > yBound) {
            transform.position = new Vector3(transform.position.x, -yBound, transform.position.z);
        }
        else if (transform.position.y < -yBound) {
            transform.position = new Vector3(transform.position.x, yBound, transform.position.z);
        }
    }

    void FixedUpdate() {
        rb.linearVelocity = transform.up * speed;
    }
}
