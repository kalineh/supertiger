﻿using System.Collections;
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

    private Animator animator;

    public enum BodyState
    {
        Idle,
        Land,
        Walk,
        PunchCharge,
        PunchLight,
        PunchHeavy,
        Jump,
        Fall,
    }

    private BodyState bodyState;
    private int bodyStateFrames;

    public void Update()
    {
    }

    public void ChangeState(BodyState state)
    {
        bodyState = state;
        bodyStateFrames = 0;
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
                        if (moveX != 0.0f)
                        {
                            ChangeState(BodyState.Walk);
                            animator.Play("Walk");
                            body.position += Vector2.right * moveX;
                            break;
                        }

                        if (jump)
                        {
                            ChangeState(BodyState.Jump);
                            animator.Play("Jump");
                        }

                        if (attack)
                        {
                            ChangeState(BodyState.PunchCharge);
                            animator.Play("PunchCharge");
                        }

                        break;
                    }
                case BodyState.Land:
                    {
                        if (bodyStateFrames > 4)
                        {
                            ChangeState(BodyState.Idle);
                            animator.Play("Idle");
                            break;
                        }
                        break;
                    }
                case BodyState.Walk:
                    {
                        body.position += Vector2.right * moveX;

                        if (moveX == 0.0f)
                        {
                            ChangeState(BodyState.Idle);
                            animator.Play("Idle");
                        }

                        if (jump)
                        {
                            ChangeState(BodyState.Jump);
                            animator.Play("Jump");
                        }

                        if (attack)
                        {
                            ChangeState(BodyState.PunchCharge);
                            animator.Play("PunchCharge");
                        }

                        break;
                    }
                case BodyState.PunchCharge:
                    {
                        var attackFramesMin = 4;
                        var attackFramesHeavy = 12;

                        if (!attack && bodyStateFrames > attackFramesHeavy)
                        {
                            ChangeState(BodyState.PunchHeavy);
                            animator.Play("PunchHeavy");
                            break;
                        }

                        if (!attack && bodyStateFrames > attackFramesMin)
                        {
                            ChangeState(BodyState.PunchLight);
                            animator.Play("PunchLight");
                            break;
                        }

                        break;
                    }
                case BodyState.PunchLight:
                    {
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

                        body.position += Vector2.right * moveX;
                        body.position += Vector2.up * jumpY;

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
                        body.position += Vector2.right * moveX;
                        body.position -= Vector2.up * fallY;

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
}
