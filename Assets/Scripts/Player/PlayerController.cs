using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState {
 Idle,
 Run,
 Jump,
 Attack,
 Interact,
 Stagger
}

public class PlayerController : PhysicsObject {

    public float jumpTakeOffSpeed = 7;
    public PlayerState currentState;
    
    private Animator _animator;

    private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int RunVelocity = Animator.StringToHash("runVelocity");
    private static readonly int Attack = Animator.StringToHash("attack");

    // Use this for initialization
    void Awake ()
    {
        currentState = PlayerState.Idle;
        _animator = GetComponent<Animator> ();
    }

    protected override void ComputeVelocity()
    {
        CalculateInputs();
        SetAnimatorParametres();
        TargetVelocity = Move * maxSpeed;
    }

    private void CalculateInputs()
    {

        if (currentState != PlayerState.Stagger)
        {
            Move.x = Input.GetAxis("Horizontal");

            if (Input.GetButtonDown("Jump") && Grounded)
            {
                Velocity.y = jumpTakeOffSpeed;
            }
            else if (Input.GetButtonUp("Jump"))
            {
                if (Velocity.y > 0) Velocity.y = Velocity.y * 0.5f;
            }

            if (Input.GetButtonDown("Attack") && currentState != PlayerState.Attack)
            {
                StartCoroutine(AttackCo());
            }
        }
    }

    private void SetAnimatorParametres()
    {
        _animator.SetBool(IsGrounded, Grounded);
        _animator.SetFloat(RunVelocity, Mathf.Abs(Velocity.x) / maxSpeed);
    }

    private IEnumerator AttackCo()
    {
        var previousState = currentState;
        _animator.SetTrigger(Attack);
        currentState = PlayerState.Attack;
        yield return new WaitForSeconds(0.8f);
        currentState = previousState;
    }
    
    public void Knock(Rigidbody2D myRigidBody, float knockTime)
    {
        StartCoroutine(KnockCo(myRigidBody, knockTime));
    }

    private IEnumerator KnockCo(Rigidbody2D myRigidBody, float knockTime)
    {
        if (myRigidBody != null && currentState == PlayerState.Stagger)
        {
            yield return new WaitForSeconds(knockTime);
            myRigidBody.velocity = Vector2.zero;
            currentState = PlayerState.Idle;
        }
    }
}