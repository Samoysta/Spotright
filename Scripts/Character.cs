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
	[Export] float wallSpeed;
	[Export] CpuParticles2D rightWallEffect;
	[Export] CpuParticles2D leftWallEffect;
	public Queue<Effect> jumpEfs = new ();
	int dir;
	Vector2 firstScale;
	bool isGrounded;
	bool isRightWalled;
	bool isLeftWalled;
	int leftWallAmount;
	int rightWallAmount;

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
		//WallEffects
		rightWallEffect.Emitting = Velocity.Y > 0 && isRightWalled;
		leftWallEffect.Emitting = Velocity.Y > 0 && isLeftWalled;
		//Wall Checks
		if (rightWallAmount > 0)
		{
			isRightWalled = true;
		}
		else
		{
			isRightWalled = false;
		}

		if (leftWallAmount > 0)
		{
			isLeftWalled = true;
		}
		else
		{
			isLeftWalled = false;
		}
		//WallSpeed Sürtünme
		if (Velocity.Y > 0)
		{
			if (isRightWalled || isLeftWalled)
			{
				if (velocity.Y > wallSpeed)
				{
					velocity.Y = wallSpeed;
					if (isRightWalled)
					{
						rightWallEffect.Emitting = true;
					}
					else
					{
						leftWallEffect.Emitting = true;
					}
				}
			}
		}
		
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
		//Jump
		if (Input.IsActionJustPressed("W"))
		{
			if (IsOnFloor())
			{
				SpawnJumpEffect();
				velocity.Y = -JumpVelocity;
				anim.Play("Jump");
				anim.Seek(0);		
			}
			else if(isRightWalled || isLeftWalled)
			{
				SpawnJumpEffect();	
				if (isLeftWalled)
				{
					velocity = new Vector2(Speed *2,-JumpVelocity);
				}
				else if (isRightWalled)
				{
					velocity = new Vector2(-Speed *2,-JumpVelocity);
				}
				anim.Play("Jump");
				anim.Seek(0);	
			}
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

	void RightWallEntered(Node2D body)
	{
		if (body.IsInGroup("Ground"))
		{
			rightWallAmount ++;
		}
	}
	void LeftWallEntered(Node2D body)
	{
		if (body.IsInGroup("Ground"))
		{
			leftWallAmount ++;
		}
	}

	void RightWallExited(Node2D body)
	{
		if (body.IsInGroup("Ground"))
		{
			rightWallAmount --;
		}
	}

	void LeftWallExited(Node2D body)
	{
		if (body.IsInGroup("Ground"))
		{
			leftWallAmount --;
		}
	}
}
