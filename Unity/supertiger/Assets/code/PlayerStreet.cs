using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStreet
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
        Land,
        Walk,
        PunchCharge,
        PunchLight,
        PunchHeavy,
        ElbowDrop,
        Jump,
        Fall,
    }

    private BodyState bodyState;
    private int bodyStateFrames;
    private float facing;

    public BoxCollider2D colliderHitIdle;
    public BoxCollider2D colliderHitElbowDrop;

    public BoxCollider2D colliderAttackPunchLight;
    public BoxCollider2D colliderAttackPunchHeavy;
    public BoxCollider2D colliderAttackElbowDrop;

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
                colliderHitElbowDrop.enabled = false;
                break;

            case BodyState.ElbowDrop:
                colliderHitIdle.enabled = false;
                colliderHitElbowDrop.enabled = true;
                break;
        }
    }

    public void OnEnable()
    {
        body = GetComponent<Rigidbody2D>();
        selfCollider = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();

        StartCoroutine(DoUpdateInput());
        //StartCoroutine(DoUpdateBodyState());
        //StartCoroutine(DoUpdateBodyStateVisual());

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
        if (delta > +0.01f)
        {
            facing = Mathf.Sign(delta);
            animator.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }

        if (delta < -0.01f)
        {
            facing = Mathf.Sign(delta);
            animator.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }
    }

    public IEnumerator DoUpdateInput()
    {
        while (true)
        {
            var inputX = Input.GetAxis("Horizontal");
            var inputY = Input.GetAxis("Vertical");
            var moveX = inputX * 0.1f;
            var jumpY = 0.15f;
            var fallY = 0.1f;
            var attack = Input.GetButton("Fire1");
            var jump = Input.GetButton("Jump");

            bodyStateFrames++;

            switch (bodyState)
            {
                case BodyState.Idle:
                    {
                        MovePosition(Vector2.right * moveX);
                        MoveFacing(moveX);

                        if (jump)
                        {
                            ChangeState(BodyState.Jump);
                            break;
                        }

                        if (attack)
                        {
                            ChangeState(BodyState.PunchCharge);
                            break;
                        }

                        if (moveX != 0.0f)
                        {
                            ChangeState(BodyState.Walk);
                            break;
                        }

                        break;
                    }
                case BodyState.Land:
                    {
                        if (bodyStateFrames > 4)
                        {
                            ChangeState(BodyState.Idle);
                            break;
                        }
                        break;
                    }
                case BodyState.Walk:
                    {
                        MovePosition(Vector2.right * moveX);
                        MoveFacing(moveX);

                        if (jump)
                        {
                            ChangeState(BodyState.Jump);
                            break;
                        }

                        if (attack)
                        {
                            ChangeState(BodyState.PunchCharge);
                            break;
                        }

                        if (moveX == 0.0f)
                        {
                            ChangeState(BodyState.Idle);
                            break;
                        }

                        break;
                    }
                case BodyState.PunchCharge:
                    {
                        var attackFramesMin = 4;
                        var attackFramesHeavy = 12;

                        MoveFacing(moveX);

                        if (!attack && bodyStateFrames > attackFramesHeavy)
                        {
                            ChangeState(BodyState.PunchHeavy);
                            break;
                        }

                        if (!attack && bodyStateFrames > attackFramesMin)
                        {
                            ChangeState(BodyState.PunchLight);
                            break;
                        }

                        break;
                    }
                case BodyState.PunchLight:
                    {
                        if (bodyStateFrames == 1)
                            StartCoroutine(DoPunchLight());

                        if (bodyStateFrames > 4)
                        {
                            ChangeState(BodyState.Idle);
                            animator.Play("Idle");
                            break;
                        }

                        break;
                    }
                case BodyState.PunchHeavy:
                    {
                        if (bodyStateFrames == 1)
                            StartCoroutine(DoPunchHeavy());

                        if (bodyStateFrames > 8)
                        {
                            ChangeState(BodyState.Idle);
                            animator.Play("Idle");
                            break;
                        }

                        break;
                    }
                case BodyState.Jump:
                    {
                        var jumpFramesMin = 12;
                        var jumpFramesMax = 40;

                        var jumpForceInv = (float)bodyStateFrames / (float)jumpFramesMax;

                        jumpY *= 1.0f - jumpForceInv * 1.0f;

                        MovePosition(Vector2.right * moveX);
                        MovePosition(Vector2.up * jumpY);

                        MoveFacing(moveX);

                        if (attack && bodyStateFrames > jumpFramesMin)
                        {
                            ChangeState(BodyState.ElbowDrop);
                            animator.Play("ElbowDrop");
                            break;
                        }

                        if (!jump && bodyStateFrames > jumpFramesMin)
                        {
                            ChangeState(BodyState.Fall);
                            animator.Play("Fall");
                            break;
                        }

                        if (bodyStateFrames > jumpFramesMax)
                        {
                            ChangeState(BodyState.Fall);
                            animator.Play("Fall");
                            break;
                        }

                        break;
                    }
                case BodyState.Fall:
                    {
                        fallY *= (float)bodyStateFrames * 0.05f;

                        MovePosition(Vector2.right * moveX);
                        MovePosition(Vector2.down * fallY);

                        MoveFacing(moveX);

                        if (IsTouchingFloor())
                        {
                            ChangeState(BodyState.Land);
                            animator.Play("Land");
                            break;
                        }
                        break;
                    }
                case BodyState.ElbowDrop:
                    {
                        if (bodyStateFrames == 1)
                            StartCoroutine(DoElbowDrop());

                        fallY *= (float)bodyStateFrames * 0.05f;

                        MovePosition(Vector2.right * moveX * 0.2f);
                        MovePosition(Vector2.down * fallY);

                        if (IsTouchingFloor())
                        {
                            ChangeState(BodyState.Land);
                            animator.Play("Land");
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

    public IEnumerator DoPunchLight()
    {
        var results = new RaycastHit2D[32];
        var collisions = colliderAttackPunchLight.Cast(Vector2.right * facing, results, 0.0f, true);

        for (int i = 0; i < collisions; ++i)
        {
            var c = results[i];
            var e = c.collider.gameObject.GetComponentInParent<EnemyStreet>();

            if (e != null)
            {
                Destroy(e.gameObject);
            }
        }

        yield break;
    }

    public IEnumerator DoPunchHeavy()
    {
        yield break;
    }

    public IEnumerator DoElbowDrop()
    {
        yield break;
    }
}
