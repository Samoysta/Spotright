using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class Character : CharacterBody2D
{
	[Export] Camera2d camera;
	[Export] float Speed;
	[Export] float JumpVelocity;
	[Export] float coyotoTime;
	float ct;
	[Export] float jumpTime;
	float jumpT;
	[Export] float inputBufTimer;
	float ibt;
	bool isZjustPressed;
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
	[Export] AnimationPlayer dieAnim;
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
		for (int i = 0; i < 12; i++)
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
		//JumpBufferInput
		if (Input.IsActionJustPressed("Z"))
		{
			ibt = inputBufTimer;
			isZjustPressed = true;
		}
		if (Input.IsActionJustReleased("Z"))
		{
			ibt = 0;
			isZjustPressed = false;
		}
		if (ibt > 0)
		{
			ibt -= (float)delta;
		}
		else
		{
			isZjustPressed = false;
		}
		
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
			if (ct > 0)
			{
				ct -= (float)delta;
			}
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
			//CoyotoTime
			ct = coyotoTime;
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
		if (Input.IsActionPressed("Right"))
		{
			if (Velocity.X < Speed)
			{
				dir = 1;
			}
		}
		else if (Input.IsActionPressed("Left"))
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
		if (isZjustPressed)
		{
			if (ct > 0)
			{
				SpawnJumpEffect();
				anim.Play("Jump");
				anim.Seek(0);
				jumpT = jumpTime;
				isJumping = true;	
				isZjustPressed = false;	
				ct = 0;
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
				isZjustPressed = false;
				ct = 0;
			}
		}
		if (isJumping)
		{
			velocity.Y = -JumpVelocity;
			jumpT -= (float)delta;
			if (Input.IsActionJustReleased("Z"))
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
			velocity.X = Mathf.MoveToward(velocity.X, dir * Speed, accel / 1.1f * (float)delta); 
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
				KillSelf();
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
	public void KillSelf()
	{
		camera.Call("Shake", 20f);
		dieAnim.Play("Die");
		SetProcess(false);
		SetPhysicsProcess(false);
	}
	void DieFinished(string animName)
	{
		if (animName == "Die")
		{
			GlobalPosition = spawnPos;
            velocity = Vector2.Zero;
			Velocity = Vector2.Zero;
			isJumping = false;
			isZjustPressed = false;
			dieAnim.Play("Birth");
			SetProcess(true);
			SetPhysicsProcess(true);
			camera.GlobalPosition = GlobalPosition;
		}
	}
}
