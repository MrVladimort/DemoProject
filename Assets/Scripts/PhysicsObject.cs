using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PhysicsObject : MonoBehaviour
{
    [FormerlySerializedAs("MinGroundNormalY")] public float minGroundNormalY = .65f;
    [FormerlySerializedAs("GravityModifier")] public float gravityModifier = 1f;
    [FormerlySerializedAs("WallTrigger")] public Transform wallTrigger;
    [FormerlySerializedAs("MaxSpeed")] public float maxSpeed = 7;
    [FormerlySerializedAs("Range")] public float range = 0.7f;

    protected Vector2 Velocity;
    protected Vector2 TargetVelocity;
    protected Vector2 GroundNormal;
    protected Vector2 Move;
    protected bool Grounded;
    
    protected Rigidbody2D Rb2D;
    protected Vector3 Direction = new Vector3(-1, 0, 0);

    private ContactFilter2D _contactFilter;
    private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private readonly List<RaycastHit2D> _hitBufferList = new List<RaycastHit2D>(16);
    private const float MinMoveDistance = 0.001f;
    private const float ShellRadius = 0.01f;

    private bool _isFacingRight;

    protected virtual void Start()
    {
        _isFacingRight = true;

        _contactFilter.useTriggers = false;
        _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        _contactFilter.useLayerMask = true;
        
        Rb2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        TargetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {
        Move = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (CheckWallTrigger()) TargetVelocity = Vector2.zero;
        
        Velocity += gravityModifier * Time.deltaTime * Physics2D.gravity;
        Velocity.x = TargetVelocity.x;

        Grounded = false;

        Vector2 deltaPosition = Velocity * Time.deltaTime;

        Vector2 moveAlongGround = new Vector2(GroundNormal.y, -GroundNormal.x);

        Vector2 move = moveAlongGround * deltaPosition.x;

        Flip(move);

        Movement(move, false);

        move = Vector2.up * deltaPosition.y;

        Movement(move, true);
    }

    void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        if (distance > MinMoveDistance)
        {
            int count = Rb2D.Cast(move, _contactFilter, _hitBuffer, distance + ShellRadius);
            _hitBufferList.Clear();
            for (int i = 0; i < count; i++)
            {
                _hitBufferList.Add(_hitBuffer[i]);
            }

            for (int i = 0; i < _hitBufferList.Count; i++)
            {
                Vector2 currentNormal = _hitBufferList[i].normal;

                if (currentNormal.y > minGroundNormalY)
                {
                    Grounded = true;
                    if (yMovement)
                    {
                        GroundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot(Velocity, currentNormal);
                if (projection < 0)
                {
                    Velocity = Velocity - projection * currentNormal;
                }

                float modifiedDistance = _hitBufferList[i].distance - ShellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }

        Rb2D.position = Rb2D.position + move.normalized * distance;
    }

    private void Flip(Vector2 move)
    {
        if (move.x < -0.01f && !_isFacingRight) FlipGameObject();
        else if (move.x > 0.01f && _isFacingRight) FlipGameObject();
    }

    private void FlipGameObject()
    {
        _isFacingRight = !_isFacingRight;
        Vector3 mirrorScale = gameObject.transform.localScale;
        mirrorScale.x *= -1;
        gameObject.transform.localScale = mirrorScale;
    }

    protected bool CheckWallTrigger()
    {
        Debug.DrawRay(wallTrigger.position, new Vector3(transform.localScale.x, 0, 0) * range);
        if (IsNotSameDirection(TargetVelocity.x, transform.localScale.x)) return false;
        
        RaycastHit2D wallHit =
            Physics2D.Raycast(wallTrigger.position, new Vector3(transform.localScale.x, 0, 0), range);

        if (wallHit == true)
            if (wallHit.collider.CompareTag("Ground"))
                return true;

        return false;
    }
    
    protected void ChangeDirection()
    {
        Direction *= -1;
    }

    private static bool IsNotSameDirection(float a, float b)
    {
        return !(a < 0 && b < 0 || a > 0 && b > 0);
    }
}
