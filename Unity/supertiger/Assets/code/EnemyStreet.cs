using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStreet
    : MonoBehaviour
{
    public Collider2D FloorTrigger;
    public Collider2D FloorCollider;

    private Collider2D selfCollider;
    private Rigidbody2D body;
    private Animator animator;

    public enum BodyState
    {
        Idle,
        Walk,
        Punch,
        Damage,
        Dead,
    }

    private BodyState bodyState;
    private int bodyStateFrames;
    private float facing;
    private Transform facingTransform;
    private int health;

    public BoxCollider2D colliderHitIdle;
    public BoxCollider2D colliderAttackPunch;

    public void Update()
    {
    }

    public void ChangeState(BodyState state)
    {
        bodyState = state;
        bodyStateFrames = 0;

        animator.Play(state.ToString());

        switch (state)
        {
            default:
                colliderHitIdle.enabled = true;
                break;
        }
    }

    public void OnEnable()
    {
        body = GetComponent<Rigidbody2D>();
        selfCollider = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();
        facingTransform = transform.Find("Facing");

        health = 3;

        StartCoroutine(DoUpdateAI());

        EjectFromFloor();
    }

    public bool IsTouchingFloor()
    {
        return Physics2D.IsTouching(selfCollider, FloorTrigger);
    }

    public void FixedUpdate()
    {
    }

    public void MovePosition(Vector2 delta)
    {
        body.position += delta;
    }

    public void MoveFacing(float delta)
    {
        if (delta < -0.001f)
        {
            facing = Mathf.Sign(delta);
            facingTransform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }

        if (delta > +0.001f)
        {
            facing = Mathf.Sign(delta);
            facingTransform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }
    }

    public IEnumerator DoUpdateAI()
    {
        var framesWalk = 0;
        var inputX = 0.0f;
        var attack = false;

        while (true)
        {
            if (Random.Range(0.0f, 1.0f) < 0.01f)
            {
                framesWalk = Random.Range(120, 200);
                inputX = Mathf.Sign(Random.Range(-1.0f, 1.0f));
            }

            var playerDetect = Physics2D.BoxCast(transform.position, Vector2.one * 2.0f, 0.0f, Vector2.zero, 0.0f, LayerMask.GetMask("Player"));
            if (playerDetect)
            {
                if (Random.Range(0.0f, 1.0f) < 0.12f)
                    attack = true;
                else
                    attack = false;
            }

            var moveX = inputX * 0.01f;

            bodyStateFrames++;

            switch (bodyState)
            {
                case BodyState.Idle:
                    {
                        MovePosition(Vector2.right * moveX);
                        MoveFacing(moveX);

                        if (attack)
                        {
                            ChangeState(BodyState.Punch);
                            break;
                        }

                        if (moveX != 0.0f)
                        {
                            ChangeState(BodyState.Walk);
                            break;
                        }

                        break;
                    }
                case BodyState.Walk:
                    {
                        MovePosition(Vector2.right * moveX);
                        MoveFacing(moveX);

                        if (attack)
                        {
                            ChangeState(BodyState.Punch);
                            break;
                        }

                        if (moveX == 0.0f)
                        {
                            ChangeState(BodyState.Idle);
                            break;
                        }

                        break;
                    }
                case BodyState.Punch:
                    {
                        if (bodyStateFrames == 1)
                            StartCoroutine(DoPunch());

                        if (bodyStateFrames > 120)
                        {
                            ChangeState(BodyState.Idle);
                            break;
                        }

                        break;
                    }
                case BodyState.Damage:
                    {
                        if (bodyStateFrames > 12)
                        {
                            ChangeState(BodyState.Idle);
                            break;
                        }

                        break;
                    }
                case BodyState.Dead:
                    {
                        var fall = Vector2.up * 0.05f + Vector2.down * 0.001f * (float)bodyStateFrames;

                        if (bodyStateFrames < 8 || !IsTouchingFloor())
                            MovePosition(fall);

                        if (bodyStateFrames > 360)
                        {
                            Destroy(gameObject);
                            break;
                        }

                        break;
                    }
            }

            yield return new WaitForFixedUpdate();
        }
    }

    public void EjectFromFloor()
    {
        if (!Physics2D.IsTouching(selfCollider, FloorTrigger))
            return;

        for (int i = 0; i < 32; ++i)
        {
            if (Physics2D.IsTouching(selfCollider, FloorCollider))
                body.position += Vector2.up * 0.001f;
        }
    }

    public IEnumerator DoPunch()
    {
        var results = new RaycastHit2D[32];
        var collisions = colliderAttackPunch.Cast(Vector2.right * facing, results, 0.0f, true);

        for (int i = 0; i < collisions; ++i)
        {
            var c = results[i];
            var p = c.collider.gameObject.GetComponentInParent<PlayerStreet>();

            if (p != null)
                p.OnGetHit(gameObject, 1);
        }

        yield break;
    }

    public void OnGetHit(GameObject src, int damage)
    {
        if (bodyState == BodyState.Dead)
            return;

        health -= damage;

        if (health <= 0)
        {
            ChangeState(BodyState.Dead);
            return;
        }

        ChangeState(BodyState.Damage);
    }
}
