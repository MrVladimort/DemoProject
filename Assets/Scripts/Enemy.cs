using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Run,
    Attack,
    Stagger
}

public class Enemy : PhysicsObject
{
    public string enemyName;
    public EnemyState currentState;
    public float attack;
    public float chaseRadius;
    public float attackRadius;

    private Transform _target;

    private Animator _animator;

    private static readonly int IsGroundedAnimation = Animator.StringToHash("isGrounded");
    private static readonly int RunVelocityAnimation = Animator.StringToHash("runVelocity");
    private static readonly int AttackAnimation = Animator.StringToHash("attack");

    // Start is called before the first frame update
    private void Awake()
    {
        currentState = EnemyState.Idle;
        _target = GameObject.FindWithTag("Player").transform;
        _animator = GetComponent<Animator>();
    }

    private void CheckDistance()
    {

        if (currentState != EnemyState.Stagger && currentState != EnemyState.Attack)
        {
            if (Vector3.Distance(_target.position, transform.position) <= chaseRadius &&
                Vector3.Distance(_target.position, transform.position) >= attackRadius)
            {
                if (currentState == EnemyState.Idle || currentState == EnemyState.Run)
                {
                    Move = GetDirectionToTarget(true);
                    currentState = EnemyState.Run;
                }
            }
            else if (Vector3.Distance(_target.position, transform.position) <= attackRadius)
            {
                Attack();
            }
            else
            {
                Move = Vector3.zero;
                currentState = EnemyState.Idle;
            }
        }
    }

    private Vector3 GetDirectionToTarget(bool isHorizontal)
    {
        var heading = _target.position - transform.position;
        var distance = heading.magnitude;
        var direction = heading / distance; // This is now the normalized direction.

        if (isHorizontal) return new Vector3(direction.x, 0, 0);

        return direction;
    }

    // Update is called once per frame
    protected override void ComputeVelocity()
    {
        CheckDistance();
        SetAnimatorParameters();
        TargetVelocity = Move * maxSpeed;
    }

    private void SetAnimatorParameters()
    {
        _animator.SetBool(IsGroundedAnimation, Grounded);
        _animator.SetFloat(RunVelocityAnimation, Mathf.Abs(Velocity.x) / maxSpeed);
    }

    public void Attack()
    {
        StartCoroutine(AttackCo());
    }

    private IEnumerator AttackCo()
    {
        _animator.SetTrigger(AttackAnimation);
        currentState = EnemyState.Attack;
        yield return new WaitForSeconds(1f);
        currentState = EnemyState.Idle;
    }

    public void Knock(Rigidbody2D myRigidBody, float knockTime)
    {
        StartCoroutine(KnockCo(myRigidBody, knockTime));
    }

    private IEnumerator KnockCo(Rigidbody2D myRigidBody, float knockTime)
    {
        if (myRigidBody != null && currentState == EnemyState.Stagger)
        {
            yield return new WaitForSeconds(knockTime);
            myRigidBody.velocity = Vector2.zero;
            currentState = EnemyState.Idle;
        }
    }
}