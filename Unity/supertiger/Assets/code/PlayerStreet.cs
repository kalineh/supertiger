using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStreet
    : MonoBehaviour
{
    public Collider2D FloorCollider;
    private Collider2D collider;

    private Vector2 acc;
    private Vector2 vel;
    private Rigidbody2D body;

    public enum BodyState
    {
        Idle,
        Step,
        Run,
        Reverse,
    }

    private BodyState bodyState;
    private float bodyFacing;

    public void Start()
    {
        body = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }

    public void Update()
    {
        UpdateInput();
        UpdateBodyState();
        UpdateBodyStateVisual();
    }

    public void UpdateBodyState()
    {
        var vx = vel.x;
        var vy = vel.y;
        var ax = acc.x;
        var ay = acc.y;
        var vxa = Mathf.Abs(vx);
        var vya = Mathf.Abs(vy);
        var axa = Mathf.Abs(ax);
        var aya = Mathf.Abs(ay);
        var vxs = Mathf.Sign(vx);
        var vys = Mathf.Sign(vy);
        var axs = Mathf.Sign(ax);
        var ays = Mathf.Sign(ay);

        if (vxa > 0.10f)
        {
            bodyState = BodyState.Run;
            bodyFacing = vxs;
            return;
        }

        if (vxa > 0.05f)
        {
            bodyState = BodyState.Step;
            bodyFacing = vxs;
            return;
        }

        bodyState = BodyState.Idle;
        bodyFacing = vxs;
    }

    public void UpdateBodyStateVisual()
    {
        var body = transform.Find("Visual/Body").GetComponent<Renderer>().material;
        var feet = transform.Find("Visual/Feet").GetComponent<Renderer>().material;

        switch (bodyState)
        {
            case BodyState.Idle: body.color = Color.white; break;
            case BodyState.Step: body.color = Color.Lerp(Color.white, Color.green, 0.25f); break;
            case BodyState.Run: body.color = Color.Lerp(Color.white, Color.green, 1.0f); break;
            case BodyState.Reverse: body.color = Color.Lerp(Color.white, Color.green, 0.5f); break;
        }

        var landed = Physics2D.IsTouching(collider, FloorCollider);

        feet.color = landed ? Color.blue : Color.white;
    }

    public void FixedUpdate()
    {
        UpdateMovement();
    }

    public void UpdateInput()
    {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");

        var punch = Input.GetButtonDown("Fire1");
        var kick = Input.GetButtonDown("Fire2");
        var jump = Input.GetButtonDown("Fire3");

        var dt = Time.deltaTime;

        acc += Vector2.right * x * dt;
        acc += Vector2.up * y * dt;
    }

    public void UpdateMovement()
    {
        var landed = Physics2D.IsTouching(collider, FloorCollider);

        var maxSpeedX = 0.3f;
        var frictionAirX = 0.20f;
        var frictionFloorX = 0.70f;
        var controlAirX = 15.0f;
        var controlFloorX = 15.0f;

        var controlX = landed ? controlFloorX : controlAirX;
        var frictionX = landed ? frictionFloorX : frictionAirX;

        vel.x += acc.x * controlX;
        vel.x = Mathf.Clamp(vel.x, -maxSpeedX, maxSpeedX);
        vel.x *= frictionX;

        //Debug.LogFormat("Vel.x: {0}, acc: {0}", vel.x, acc.x);

        acc.x = 0.0f;
        acc.y = 0.0f;

        if (Mathf.Abs(vel.x) <= 0.01f) vel.x = 0.0f;
        if (Mathf.Abs(vel.y) <= 0.01f) vel.y = 0.0f;
        if (Mathf.Abs(acc.x) <= 0.01f) acc.x = 0.0f;
        if (Mathf.Abs(acc.y) <= 0.01f) acc.y = 0.0f;

        body.position += vel;
        body.velocity = Vector2.zero;
    }
}
