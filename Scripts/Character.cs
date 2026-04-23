using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Formats.Tar;

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
	[Export] public AnimationPlayer anim;
	[Export] CpuParticles2D runEffect;
	[Export] PackedScene jumpEf;
	[Export] PackedScene dashEf;
	[Export] float wallSpeed;
	[Export] CpuParticles2D rightWallEffect;
	[Export] CpuParticles2D leftWallEffect;
	[Export] float dashSpeed;
	[Export] public float dashCoolDown;
	[Export] PackedScene dashHaloEf;
	public float dashCD;
	public bool isDashing;
	[Export] float dashDur;
	float dashD;
	bool canDash;
	bool canJump = true;
	float dashHaloCD;
	[Export] AnimationPlayer dieAnim;
	Vector2 spawnPos;
	public Vector2 velocity;
	public Queue<Effect> jumpEfs = new ();
	public Queue<Effect> dashEfs = new ();
	public Queue<Effect> dashHaloEfs = new ();
	float dir;
	public bool isJumping;
	Vector2 firstScale;
	bool isGrounded;
	bool isRightWalled;
	bool isLeftWalled;
	int leftWallAmount;
	int rightWallAmount;
	int lastDir;


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
		for (int i = 0; i < 3; i++)
		{
			Effect def = (Effect)dashEf.Instantiate();
			GetTree().CurrentScene.CallDeferred("add_child", def);
			def.Scale = new Vector2(1,1);
			dashEfs.Enqueue(def);
		}
		for (int i = 0; i < 12; i++)
		{
			Effect def = (Effect)dashHaloEf.Instantiate();
			GetTree().CurrentScene.CallDeferred("add_child", def);
			def.Scale = new Vector2(1,1);
			dashHaloEfs.Enqueue(def);
		}
		lastDir = -1;
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
				canDash = true;
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
			if (velocity.Y < 0)
			{
				ct = 0;
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
			canDash = true;
			//CoyotoTime
			ct = coyotoTime;
		}
		//Sıkışma
		if (IsOnFloor() && IsOnCeiling())
		{
			KillSelf();
		}
		if (isRightWalled && isLeftWalled)
		{
			KillSelf();
		}
		//Dash
		if (Input.IsActionJustPressed("C") && dashCD <= 0 && canDash)
		{
			dashD = dashDur;
			dashCD = dashCoolDown;
			isDashing = true;
			ct = 0;
			isZjustPressed = false;
			isJumping = false;
			anim.Play("Dash");
			SpawnDashEffect();
		}
		if (dashD > 0)
		{
			dashD -= (float)delta;
		}
		if (dashCD > 0)
		{
			dashCD -= (float)delta;
		}
		if (isDashing)
		{
			velocity.X = dashSpeed * lastDir;
			velocity.Y = 0;
			canDash = false;
			if (dashHaloCD > 0)
			{
				dashHaloCD -= (float)delta;
			}
			else
			{
				SpawnDashHaloEffect();
				dashHaloCD = dashDur / 5;
			}
			AttemptCorrectionX(3);
		}
		else if (!isDashing)
		{
			dashHaloCD = 0;
			//sprite yönü
			if (lastDir > 0)
			{
				characterSprite.Scale = new Vector2(-firstScale.X,firstScale.Y);
			}
			else if(lastDir < 0)
			{
				characterSprite.Scale = firstScale;
			}
			//Run
			if (Input.IsActionPressed("Right"))
			{
				if (Velocity.X <= Speed)
				{
					dir = 1;
				}
				lastDir = 1;
			}
			else if (Input.IsActionPressed("Left"))
			{
				if (Velocity.X >= -Speed)
				{
					dir = -1;
				}
				lastDir = -1;
			}
			else
			{
				dir = 0;
			}
			//Jump
			if (isZjustPressed && canJump)
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
					canDash = true;
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
					canDash = true;

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
		}
		if(dashD <= 0)
		{
			if (isDashing)
			{
				if (Input.IsActionPressed("Right"))
				{
					if (velocity.X > 0)
					{
						velocity.X = Speed;	
					}
					else
					{
						velocity = Vector2.Zero;	
					}
				}
				else if (Input.IsActionPressed("Left"))
				{
					if (velocity.X < 0)
					{
						velocity.X = -Speed;	
					}
					else
					{
						velocity = Vector2.Zero;	
					}
				}
				else
				{
					velocity = Vector2.Zero;	
				}
				isDashing = false;
			}
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
		GlobalPosition = GlobalPosition.Round();
		AttemptCorrection(6);
		canJump = true;
	}

	public void AddForce(Vector2 vel)
	{
		canJump = false;
		isDashing = false;
		isJumping = false;
		velocity = vel;
		Velocity = vel;
		isZjustPressed = false;
		canDash = true;
		if (vel.Y < -JumpVelocity)
		{
			anim.CallDeferred("play", "Jump");
			anim.CallDeferred("seek", 0);
		}
	}

	public void AttemptCorrection(int amount)
	{
		float delta = (float)GetPhysicsProcessDeltaTime();

		// sadece yukarı giderken (tavana çarpma)
		if (Velocity.Y < 0 &&
			TestMove(GlobalTransform, new Vector2(0, Velocity.Y * delta)))
		{
			for (int i = 1; i <= amount * 2; i++)
			{
				for (int j = -1; j <= 1; j += 2)
				{
					Vector2 offset = new Vector2(i * j / 2f, 0);

					if (!TestMove(GlobalTransform.Translated(offset),new Vector2(0, Velocity.Y * delta)))
					{
						GlobalPosition += offset;

						// yatay hız ters yöndeyse sıfırla
						if (Velocity.X * j < 0)
							Velocity = new Vector2(0, Velocity.Y);

						return;
					}
				}
			}
		}
	}

	public void AttemptCorrectionX(int amount)
	{
		float delta = (float)GetPhysicsProcessDeltaTime();
		if (TestMove(GlobalTransform, new Vector2(velocity.X * delta, 0)))
		{
			for (int i = 1; i <= amount * 2; i++)
			{
				for (int j = -1; j <= 1; j += 2)
				{
					if (velocity.Y * j <= 0)
					{
						Vector2 offset = new Vector2(0 , i * j / 2f);

						if (!TestMove(GlobalTransform.Translated(offset), new Vector2(velocity.X * delta, 0)))
						{
							GlobalPosition += offset;
							return;
						}		
					}
				}
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

	void SpawnDashEffect()
	{
		Effect ef = dashEfs.Dequeue();
		ef.GlobalPosition = GlobalPosition;
		if (lastDir > 0)
		{
			ef.Scale = new Vector2(-1,1);
		}
		else
		{
			ef.Scale = new Vector2(1,1);
		}
		ef.setOn();
		dashEfs.Enqueue(ef);
	}
	void SpawnDashHaloEffect()
	{
		Effect ef = dashHaloEfs.Dequeue();
		ef.GlobalPosition = GlobalPosition;
		if (lastDir > 0)
		{
			ef.Scale = new Vector2(-1,1);
		}
		else
		{
			ef.Scale = new Vector2(1,1);
		}
		ef.setOn();
		dashHaloEfs.Enqueue(ef);
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
			isDashing = false;
			dashD = 0;
			dieAnim.Play("Birth");
			SetProcess(true);
			SetPhysicsProcess(true);
			camera.GlobalPosition = GlobalPosition;
		}
	}
}
