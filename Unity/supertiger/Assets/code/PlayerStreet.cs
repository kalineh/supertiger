using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStreet
    : MonoBehaviour
{
    public Collider2D FloorTrigger;
    public Collider2D FloorCollider;

    private Collider2D selfCollider;

    private Vector2 vel;
    private Rigidbody2D body;

    private float forceX;
    private float forceY;

    private int attackFrames;
    private int jumpFrames;
    private int fallFrames;
    private int attackCooldown;
    private int jumpCooldown;

    public enum BodyState
    {
        Idle,
        Land,
        Walk,
        PunchCharge,
        Punch,
        Jump,
        Fall,
    }

    private BodyState bodyState;
    private float bodyFacing;

    public void Update()
    {
    }

    public void OnEnable()
    {
        body = GetComponent<Rigidbody2D>();
        selfCollider = GetComponent<Collider2D>();

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

            if (attackCooldown > 0)
            {
                attack = false;
                attackCooldown--;
            }

            if (jumpCooldown > 0)
            {
                jump = false;
                jumpCooldown--;
            }

            switch (bodyState)
            {
                case BodyState.Idle:
                    {
                        if (moveX != 0.0f)
                        {
                            bodyState = BodyState.Walk;
                            body.position += Vector2.right * moveX;
                            break;
                        }

                        if (jump)
                        {
                            bodyState = BodyState.Jump;
                            jumpFrames = 0;
                        }

                        if (attack)
                        {
                            bodyState = BodyState.Punch;
                            attackFrames = 0;
                        }

                        break;
                    }
                case BodyState.Land:
                    {
                        bodyState = BodyState.Idle;
                        break;
                    }
                case BodyState.Walk:
                    {
                        body.position += Vector2.right * moveX;

                        if (moveX == 0.0f)
                        {
                            bodyState = BodyState.Idle;
                        }

                        if (jump)
                        {
                            bodyState = BodyState.Jump;
                            jumpFrames = 0;
                        }

                        if (attack)
                        {
                            bodyState = BodyState.Punch;
                            attackFrames = 0;
                        }

                        break;
                    }
                case BodyState.PunchCharge:
                    {
                        if (!attack)
                        {
                            bodyState = BodyState.Idle;
                            attackCooldown = 12;
                            break;
                        }

                        break;
                    }
                case BodyState.Punch:
                    {
                        var attackFramesMin = 4;
                        var attackFramesCharge = 12;

                        attackFrames++;

                        if (!attack && attackFrames > attackFramesMin)
                        {
                            bodyState = BodyState.Idle;
                            attackCooldown = 4;
                            break;
                        }

                        if (attackFrames > attackFramesCharge)
                        {
                            bodyState = BodyState.PunchCharge;
                            break;
                        }

                        break;
                    }
                case BodyState.Jump:
                    {
                        var jumpFramesMin = 12;
                        var jumpFramesMax = 40;

                        var jumpForceInv = (float)jumpFrames / (float)jumpFramesMax;

                        jumpY *= 1.0f - jumpForceInv * 1.0f;

                        body.position += Vector2.right * moveX;
                        body.position += Vector2.up * jumpY;

                        jumpFrames++;

                        if (!jump && jumpFrames > jumpFramesMin)
                        {
                            bodyState = BodyState.Fall;
                            jumpCooldown = 10;
                            fallFrames = 0;
                            break;
                        }

                        if (jumpFrames > jumpFramesMax)
                        {
                            bodyState = BodyState.Fall;
                            jumpCooldown = 10;
                            fallFrames = 0;
                            break;
                        }

                        break;
                    }
                case BodyState.Fall:
                    {
                        fallFrames++;

                        fallY *= (float)fallFrames * 0.05f;
                        body.position += Vector2.right * moveX;
                        body.position -= Vector2.up * fallY;

                        if (IsTouchingFloor())
                        {
                            bodyState = BodyState.Land;
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
}
