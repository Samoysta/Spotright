using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class Character : CharacterBody2D
{
	[Export] Camera2d camera;
	[Export] float Speed;
	[Export] float JumpVelocity;
	[Export] float jumpTime;
	float jumpT;
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
	Vector2 spawnPos;
	Vector2 velocity;
	public Queue<Effect> jumpEfs = new ();
	int dir;
	bool isJumping;
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
		velocity = Velocity;
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
				runEffect.CallDeferred("set_emitting",true);
			}
			else
			{
				runEffect.CallDeferred("set_emitting", false);
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
				anim.Play("Jump");
				anim.Seek(0);
				jumpT = jumpTime;
				isJumping = true;		
			}
			else if(isRightWalled || isLeftWalled)
			{
				SpawnJumpEffect();	
				if (isLeftWalled)
				{
					velocity.X = Speed;
				}
				else if (isRightWalled)
				{
					velocity.X = -Speed;
				}
				anim.Play("Jump");
				anim.Seek(0);	
				isJumping = true;
				jumpT = jumpTime;
			}
		}
		if (isJumping)
		{
			velocity.Y = -JumpVelocity;
			jumpT -= (float)delta;
			if (Input.IsActionJustReleased("W"))
			{
				isJumping = false;
			}
			if (jumpT <= 0)
			{
				isJumping = false;
			}
			if (IsOnCeiling())
			{
				isJumping = false;
			}
		}
		//movement apply
		if (IsOnFloor())
		{
			velocity.X = Mathf.MoveToward(velocity.X, dir * Speed, accel * (float)delta); 	
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, dir * Speed, accel / 1.2f * (float)delta); 
		}
		Velocity = velocity;
		MoveAndSlide();
		//Çarpışma kontrol
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);
			Node2D col = (Node2D)collision.GetCollider();
			if (col.IsInGroup("DamageTile"))
			{
				GlobalPosition = spawnPos;
                velocity = Vector2.Zero;
				Velocity = Vector2.Zero;
				isJumping = false;
				camera.Call("Shake", 20f);
			}
		}
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

	public void SetSpawnPos(Vector2 pos)
	{
		spawnPos = pos;
	}
}
