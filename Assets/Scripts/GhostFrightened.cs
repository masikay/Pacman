using System.Collections;
using UnityEngine;

public class GhostFrightened : GhostBehaviour
{
    public SpriteRenderer body;
    public SpriteRenderer eyes;
    public SpriteRenderer blue;
    public SpriteRenderer white;

    public Node inside;
    public Node outside;

    public bool eaten { get; private set; }

    public override void Start()
    {

    }

    public override void Update()
    {
        if (this.eaten && this.transform.position.x == this.ghost.home.outside.position.x && this.transform.position.y == this.ghost.home.outside.position.y)
        {
            this.ghost.home.Enable(this.duration);
            this.ghost.movement.speedMultiplier = 1.0f;
            Physics2D.IgnoreCollision(this.ghost.target.GetComponent<Collider2D>(), this.GetComponent<Collider2D>(), false);
        }
    }

    public override void Enable(float duration)
    {
        base.Enable(duration);

        this.body.enabled = false;
        this.eyes.enabled = false;
        this.blue.enabled = true;
        this.white.enabled = false;

        Invoke(nameof(Flash), duration * 0.5f);

    }

    public override void Disable()
    {
        base.Disable();

        this.body.enabled = true;
        this.eyes.enabled = true;
        this.blue.enabled = false;
        this.white.enabled = false;
    }

    private void Flash()
    {
        if (!this.eaten)
        {
            this.blue.enabled = false;
            this.white.enabled = true;
            this.white.GetComponent<AnimatedSprite>().Restart();
        }
    }

    private void Eaten()
    {
        this.eaten = true;

        //Vector3 position = this.ghost.home.inside.position;
        //position.z = this.ghost.transform.position.z;
        //this.ghost.transform.position = position;
        //this.ghost.home.Enable(this.duration);
        
        this.ghost.movement.speedMultiplier = 2.0f;
        Physics2D.IgnoreCollision(this.ghost.target.GetComponent<Collider2D>(), this.GetComponent<Collider2D>());
        
        this.body.enabled = false;
        this.eyes.enabled = true;
        this.blue.enabled = false;
        this.white.enabled = false;
    }

    private void OnEnable()
    {
        this.ghost.movement.speedMultiplier = 0.5f;
        this.eaten = false;
    }

    private void OnDisable()
    {
        this.ghost.movement.speedMultiplier = 1.0f;
        this.eaten = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            if (this.enabled)
            {
                Eaten();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && this.enabled)
        {
            if (!this.eaten)
            {
                Vector2 direction = Vector2.zero;
                float maxDistance = float.MinValue;

                foreach (Vector2 availableDirection in node.availableDirections)
                {
                    Vector3 newPosition = this.transform.position + new Vector3(availableDirection.x, availableDirection.y, 0.0f);
                    float distance = (this.ghost.target.position - newPosition).sqrMagnitude;

                    if (distance > maxDistance)
                    {
                        direction = availableDirection;
                        maxDistance = distance;
                    }
                }

                this.ghost.movement.SetDirection(direction);
            }
            else
            {
                if ((node.transform.position.x >= -1.5f && node.transform.position.x <= -1.5f) && node.transform.position.y == 2.5f)
                {
                    StartCoroutine(EnterTransition());
                }
                
                Vector2 direction = Vector2.zero;
                float minDistance = float.MaxValue;

                foreach (Vector2 availableDirection in node.availableDirections)
                {
                    Vector3 newPosition = this.transform.position + new Vector3(availableDirection.x, availableDirection.y, 0.0f);
                    float distance = (this.ghost.home.outside.position - newPosition).sqrMagnitude;

                    if (distance < minDistance)
                    {
                        direction = availableDirection;
                        minDistance = distance;
                    }
                }

                this.ghost.movement.SetDirection(direction);

            }
        }
    }

    private IEnumerator EnterTransition()
    {
        this.ghost.movement.SetDirection(Vector3.down, true);
        this.ghost.movement.rigidbody.isKinematic = true;
        this.ghost.movement.enabled = false;

        Vector3 position = this.transform.position;

        float duration = 0.25f;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            Vector3 newPosition = Vector3.Lerp(position, this.ghost.home.outside.position, elapsed / duration);
            newPosition.z = position.z;
            this.ghost.transform.position = newPosition;
            elapsed += Time.deltaTime;

            yield return null;
        }

        elapsed = 0.0f;

        while (elapsed < duration)
        {
            Vector3 newPosition = Vector3.Lerp(this.ghost.home.outside.position, this.ghost.home.inside.position, elapsed / duration);
            newPosition.z = position.z;
            this.ghost.transform.position = newPosition;
            elapsed += Time.deltaTime;

            yield return null;
        }

        this.ghost.movement.SetDirection(new Vector2(Random.value < 0.5f ? -1.0f : 1.0f, 0.0f), true);
        this.ghost.movement.rigidbody.isKinematic = false;
        this.ghost.movement.enabled = true;
        this.ghost.frightened.Disable();
        this.ghost.home.Enable(this.duration);
        Physics2D.IgnoreCollision(this.ghost.target.GetComponent<Collider2D>(), this.GetComponent<Collider2D>(), false);

    }
}
