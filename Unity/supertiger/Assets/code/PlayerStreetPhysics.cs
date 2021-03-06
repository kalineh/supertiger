﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStreetPhysics
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

    public enum BodyState
    {
        Idle,
        Land,
        Walk,
        PunchCharge,
        Punch,
        JumpPushing,
        JumpRising,
        JumpFalling,
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

    public bool IsJumpingAny()
    {
        return
            bodyState == BodyState.JumpPushing ||
            bodyState == BodyState.JumpRising ||
            bodyState == BodyState.JumpFalling;
    }

    public bool IsTouchingFloor()
    {
        return Physics2D.IsTouching(selfCollider, FloorTrigger);
    }

    /*
    public IEnumerator DoUpdateBodyState()
    {
        yield break;
        var frameCycle = new WaitForFixedUpdate();

        while (true)
        {
            var vx = vel.x;
            var vy = vel.y;
            var vxa = Mathf.Abs(vx);
            var vya = Mathf.Abs(vy);
            var vxs = Mathf.Sign(vx);
            var vys = Mathf.Sign(vy);

            if (IsJumpingAny())
            {
                var landed = IsTouchingFloor();
                if (landed)
                {
                    bodyState = BodyState.Idle;
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                bodyFacing = vxs;
                yield return null;
                continue;
            }

            if (vxa > 0.10f)
            {
                //bodyState = BodyState.Run;
                bodyFacing = vxs;

                yield return null;
                continue;
            }

            if (vxa > 0.05f)
            {
                //bodyState = BodyState.Step;
                bodyFacing = vxs;

                yield return null;
                continue;
            }

            bodyState = BodyState.Idle;
            bodyFacing = vxs;
        }
    }
    */

        /*
    public IEnumerator DoUpdateBodyStateVisual()
    {
        var body = transform.Find("Visual/Body").GetComponent<Renderer>().material;
        var feet = transform.Find("Visual/Feet").GetComponent<Renderer>().material;

        while (true)
        {
            switch (bodyState)
            {
                case BodyState.Idle: body.color = Color.white; break;
                //case BodyState.Step: body.color = Color.Lerp(Color.white, Color.green, 0.25f); break;
                //case BodyState.Run: body.color = Color.Lerp(Color.white, Color.green, 1.0f); break;
                //case BodyState.Reverse: body.color = Color.Lerp(Color.white, Color.green, 0.5f); break;
                //case BodyState.Jump: body.color = Color.Lerp(Color.white, Color.red, 0.5f); break;
            }

            var landed = Physics2D.IsTouching(selfCollider, FloorCollider);

            feet.color = landed ? Color.blue : Color.white;

            yield return null;
        }
    }
    */

    public void FixedUpdate()
    {
    }

    public IEnumerator DoUpdateInput()
    {
        while (true)
        {
            var inputX = Input.GetAxis("Horizontal");
            var inputY = Input.GetAxis("Vertical");

            var inputUp = inputY > 0.0f;
            var inputDown = inputY < 0.0f;

            if (Input.GetButton("Fire1"))
                attackFrames++;
            else
                attackFrames = 0;

            if (Input.GetButton("Jump"))
                jumpFrames++;
            else
                jumpFrames = 0;

            var landed = IsTouchingFloor();
            var dt = Time.deltaTime;

            switch (bodyState)
            {
                case BodyState.Idle:
                    {
                        if (jumpFrames == 1)
                        {
                            bodyState = BodyState.JumpPushing;
                            forceY = 0.20f;
                            break;
                        }

                        if (attackFrames == 1)
                        {
                            bodyState = BodyState.PunchCharge;
                            forceX = 0.0f;
                            forceY = 0.0f;
                            jumpFrames = 0;
                            break;
                        }

                        forceX += inputX * 0.02f;
                        break;
                    }
                case BodyState.Land:
                    {
                        forceX *= 0.2f;
                        forceY = 0.0f;
                        bodyState = BodyState.Idle;
                        break;
                    }

                case BodyState.Walk:
                    {
                        bodyState = BodyState.Idle;
                        break;
                    }
                case BodyState.PunchCharge:
                    {
                        bodyState = BodyState.Idle;
                        break;
                    }
                case BodyState.JumpPushing:
                    {
                        forceX += inputX * 0.01f;
                        forceY += 0.0003f;
                        forceY -= (float)jumpFrames * 0.00002f;
                        forceY = Mathf.Max(forceY, 0.0f);

                        if (jumpFrames == 0 || forceY <= 0.0f)
                        {
                            bodyState = BodyState.JumpRising;
                            forceY = 0.0f;
                            break;
                        }

                        break;
                    }
                case BodyState.JumpRising:
                    {
                        forceX += inputX * 0.01f;
                        forceY -= 0.001f;
                        forceY = Mathf.Max(forceY, 0.0f);

                        if (forceY <= 0.0f)
                        {
                            bodyState = BodyState.JumpFalling;
                            break;
                        }

                        break;
                    }
                case BodyState.JumpFalling:
                    {
                        forceX += inputX * 0.01f;
                        forceY -= 0.008f;

                        if (landed)
                        {
                            bodyState = BodyState.Land;
                            break;
                        }

                        break;
                    }
            }

            vel += new Vector2(forceX, forceY);

            body.position += vel;

            landed = IsTouchingFloor();

            forceX = Mathf.Clamp(forceX, -0.03f, 0.03f);
            forceY = Mathf.Clamp(forceY, -0.25f, 0.25f);
            vel.x = Mathf.Clamp(vel.x, -0.03f, 0.03f);
            vel.y = Mathf.Clamp(vel.y, -0.05f, 0.05f);

            if (landed)
                vel.x *= 0.8f;
            else
                vel.x *= 0.98f;

            vel.y *= 0.8f;

            if (landed)
                forceX *= 0.40f;
            else
                forceX *= 0.80f;

            forceY *= 0.92f;

            if (Mathf.Abs(forceX) < 0.001f) forceX = 0.0f;
            if (Mathf.Abs(forceY) < 0.001f) forceY = 0.0f;
            if (Mathf.Abs(vel.x) < 0.001f) vel.x = 0.0f;
            if (Mathf.Abs(vel.y) < 0.001f) vel.y = 0.0f;

            EjectFromFloor();

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
