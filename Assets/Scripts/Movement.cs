using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public new Rigidbody2D rigidbody { get; private set; }

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }
}
