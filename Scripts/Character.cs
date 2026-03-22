using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class Character : CharacterBody2D
{
	[Export] float Speed;
	[Export] float JumpVelocity;
	[Export] float Gravity;
	[Export] float accel;
	[Export] Sprite2D characterSprite;
	[Export] Node2D foot;
	[Export] AnimationPlayer anim;
	[Export] CpuParticles2D runEffect;
	[Export] PackedScene jumpEf;
	public Queue<Effect> jumpEfs = new ();
	int dir;
	Vector2 firstScale;
	bool isGrounded;

    public override void _Ready()
    {
        firstScale = characterSprite.Scale;
		for (int i = 0; i < 6; i++)
		{
			Effect jef = (Effect)jumpEf.Instantiate();
			GetTree().CurrentScene.CallDeferred("add_child", jef);
			jef.Scale = new Vector2(1,1);
			jumpEfs.Enqueue(jef);
		}
    }

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += Gravity * GetGravity() * (float)delta;
			if (isGrounded)
			{
				isGrounded = false;
			}
			runEffect.Emitting = false;
		}
		if (IsOnFloor())
		{
			if (!isGrounded)
			{
				SpawnJumpEffect();
				anim.Play("Fall");
				anim.Seek(0);
				isGrounded = true;
			}
			//RunEffect
			if (Mathf.Abs(Velocity.X - 0) > 10)
			{
				runEffect.Emitting = true;
			}
			else
			{
				runEffect.Emitting = false;
			}
			//Jump
			if (Input.IsActionJustPressed("W"))
			{
				SpawnJumpEffect();
				velocity.Y = -JumpVelocity;
				anim.Play("Jump");
				anim.Seek(0);
			}
		}
		//sprite yönü
		if (dir > 0)
		{
			characterSprite.Scale = new Vector2(-firstScale.X,firstScale.Y);
		}
		else if(dir < 0)
		{
			characterSprite.Scale = firstScale;
		}
		//Run
		if (Input.IsActionPressed("D"))
		{
			if (Velocity.X < Speed)
			{
				dir = 1;
			}
		}
		else if (Input.IsActionPressed("A"))
		{
			if (Velocity.X > -Speed)
			{
				dir = -1;
			}
		}
		else
		{
			dir = 0;
		}
		velocity.X = Mathf.Lerp(velocity.X, dir * Speed, accel * (float)delta); 
		Velocity = velocity;
		MoveAndSlide();
	}

	void SpawnJumpEffect()
	{
		Effect ef = jumpEfs.Dequeue();
		ef.GlobalPosition = foot.GlobalPosition;
		ef.setOn();
		jumpEfs.Enqueue(ef);
	}
}
