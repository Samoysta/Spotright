using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Formats.Tar;

public partial class Character : CharacterBody2D
{
	PlayerData pd;
	[Export] Node2D[] doors;
	[Export] int [] doorIDs;
	[Export] Camera2d camera;
	[Export] public float Speed;
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
	[Export] public AnimatedSprite2D characterSprite;
	[Export] Node2D foot;
	[Export] PackedScene jumpEf;
	[Export] PackedScene dashEf;
	[Export] float wallSpeed;
	[Export] CpuParticles2D rightWallEffect;
	[Export] CpuParticles2D leftWallEffect;
	[Export] float dashSpeed;
	[Export] public float dashCoolDown;
	[Export] PackedScene dashHaloEf;
	[Export] PackedScene hitEf;
	[Export] PackedScene balHaloEf;
	[Export] AnimatedSprite2D baloonBoomEf;
	[Export] CpuParticles2D baloonBoomEfCpu;
	public float dashCD;
	public bool isDashing;
	public bool canDie = true;
	[Export] float dashDur;
	float dashD;
	bool canDash;
	bool canJump = true;
	float dashHaloCD;
	[Export] AnimationPlayer dieAnim;
	[Export] public AnimationPlayer anim;
	Vector2 spawnPos;
	public Vector2 velocity;
	public Queue<Effect> jumpEfs = new ();
	public Queue<Effect> dashEfs = new ();
	public Queue<Effect> dashHaloEfs = new ();
	public Queue<Effect> hitEffects = new ();
	public Queue<Effect> baloonHaloEfs = new ();
	public Queue<Wepaon1Bullet> bul1s = new();
	float dir;
	public bool isJumping;
	Vector2 firstScale;
	bool isGrounded;
	bool isRightWalled;
	bool isLeftWalled;
	int leftWallAmount;
	int rightWallAmount;
	int lastDir;
	bool canAnim;
	public bool selected;
	public bool cantInput;
	CollisionShape2D col;
	//die Effecct
	Vector2 center;
	Vector2 start;
	Vector2 end;
	float t1;
	Tween t;
	float dieTimer = -11;
	float birthTimer = -11;
	float haloTimer;
	bool restartAnim;

    public override void _Ready()
    {
		pd = GetNode<PlayerData>("/root/PlayerData");
		col = GetNode<CollisionShape2D>("CollisionShape2D");
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
		for (int i = 0; i < 6; i++)
		{
			Effect ef = (Effect)hitEf.Instantiate();
			GetTree().CurrentScene.CallDeferred("add_child", ef);
			ef.Scale = new Vector2(1,1);
			hitEffects.Enqueue(ef);
		}
		for (int i = 0; i < 12; i++)
		{
			Effect def = (Effect)dashHaloEf.Instantiate();
			GetTree().CurrentScene.CallDeferred("add_child", def);
			def.Scale = new Vector2(1,1);
			dashHaloEfs.Enqueue(def);
		}
		for (int i = 0; i < 12; i++)
		{
			Effect ef = (Effect)balHaloEf.Instantiate();
			GetTree().CurrentScene.CallDeferred("add_child", ef);
			ef.Scale = new Vector2(1,1);
			baloonHaloEfs.Enqueue(ef);
		}
		lastDir = pd.lastDir;
		if (lastDir < 0)
		{
			characterSprite.Scale = new Vector2(-firstScale.X,firstScale.Y);
		}
		else if(lastDir > 0)
		{
			characterSprite.Scale = firstScale;
		}	
		if (pd.doorID != 0)
		{
			int index = Array.IndexOf(doorIDs, pd.doorID);
			Node2D spawnps = doors[index].GetNode<Node2D>("spawnPos");
			GlobalPosition = spawnps.GlobalPosition;
			camera.GlobalPosition = GlobalPosition;
			doors[index].Call("Starting");
			cantInput = true;
			characterSprite.Play("Idle");
			isGrounded = true;
		}
    }
    public override void _Process(double delta)
    {
        
    }

	public override void _PhysicsProcess(double delta)
	{
		velocity = Velocity;
		//restartAnim;
		if (birthTimer >= 0)
		{
			birthTimer -= (float)delta;
		}
		else if (birthTimer < 0 && birthTimer > -10)
		{
			cantInput = false;
			restartAnim = false;
			canDie = true;
			col.CallDeferred("set_disabled",false);
			velocity = Vector2.Zero;
			Velocity = Vector2.Zero;
			anim.CallDeferred("play","RESET");
			canDie = true;
			isJumping = false;
			baloonBoomEf.GlobalPosition = GlobalPosition;
			baloonBoomEf.Play("Start");
			baloonBoomEfCpu.Emitting = true;
			birthTimer = -11;
		}
		if (dieTimer >= 0)
		{
			dieTimer -= (float)delta;
		}
		else if (dieTimer < 0 && dieTimer > -10)
		{
			t?.Kill();
			t = CreateTween();
			t.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
			t.TweenProperty(this, nameof(t1), 1, 0.7f);
			t1 = Mathf.Clamp(t1,0,1);
			dieTimer = -11;
		}
		if (cantInput)
		{
			if (restartAnim)
			{
				ReStartAnim();	
			}
		}
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
		rightWallEffect.CallDeferred("set_emitting",Velocity.Y > 0 && isRightWalled);
		leftWallEffect.CallDeferred("set_emitting",Velocity.Y > 0 && isLeftWalled);
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
		//DüşmeAnim
		if (velocity.Y > -JumpVelocity / 4 && !IsOnFloor() && !cantInput)
		{
			if (characterSprite.Animation != "Fall" && !isDashing)
			{
				if (!((isRightWalled || isLeftWalled) && velocity.Y >= wallSpeed))
				{
					characterSprite.Play("Fall");	
				}
			}
		}
		//WallSpeed Sürtünme
		if (velocity.Y > -JumpVelocity)
		{
			if (isRightWalled || isLeftWalled)
			{
				if (velocity.Y > wallSpeed)
				{
					velocity.Y = wallSpeed;
					characterSprite.Play("Climb");
					if (isRightWalled)
					{
						characterSprite.Scale = firstScale;
						rightWallEffect.Emitting = true;
					}
					else
					{
						characterSprite.Scale = new Vector2(-firstScale.X,firstScale.Y);
						leftWallEffect.Emitting = true;
					}
				}
				canDash = true;
			}
		}
		// Add the gravity.
		if (!IsOnFloor() && !cantInput)
		{
			if (velocity.Y <= JumpVelocity * 1.5f && !cantInput)
			{
				if (isJumping)
				{
					velocity += Gravity * GetGravity() * (float)delta;
				}
				else
				{
					velocity += 1.2f * Gravity * GetGravity() * (float)delta;
				}		
			}
			if (isGrounded)
			{
				isGrounded = false;
			}
			if (ct > 0)
			{
				ct -= (float)delta;
			}
			if (velocity.Y < 0)
			{
				ct = 0;
			}
			if (anim.CurrentAnimation == "Fall")
			{
				anim.Play("RESET");
			}
		}
		if (IsOnFloor() && !cantInput)
		{
			if (!isGrounded)
			{
				SpawnJumpEffect();
				if (!isDashing)
				{
					characterSprite.Play("Falled");	
					anim.Play("Fall");
					anim.Seek(0);
				}
				characterSprite.Frame = 0;
				isGrounded = true;
				if (Mathf.Abs(velocity.X - 0) < dashSpeed * 2)
				{
					velocity.X = Mathf.Clamp(velocity.X,-Speed,Speed);
				}
			}
			//RunEffect
			if (Mathf.Abs(Velocity.X - 0) > 10)
			{
				if (!isDashing && canAnim)
				{
					characterSprite.Play("Run");	
				}
			}
			else
			{
				if (!isDashing && canAnim)
				{
					if (characterSprite.Animation == "Falled")
					{			
						if (!characterSprite.IsPlaying())
						{
							characterSprite.Play("Idle");	
						}
					}
					else
					{
						characterSprite.Play("Idle");
					}	
				}
			}
			canDash = true;
			//CoyotoTime
			ct = coyotoTime;
		}
		//Sıkışma
		if (canDie)
		{
			if (IsOnFloor() && IsOnCeiling())
			{
				KillSelf();
			}
			if (isRightWalled && isLeftWalled)
			{
				KillSelf();
			}	
		}
		
		//Dash
		if (Input.IsActionJustPressed("C") && dashCD <= 0 && canDash && !cantInput)
		{
			anim.Play("Dash");
			dashD = dashDur;
			dashCD = dashCoolDown;
			isDashing = true;
			ct = 0;
			isZjustPressed = false;
			isJumping = false;
			characterSprite.Play("Dash");
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
		if (isDashing && !cantInput)
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
		}
		else if (!isDashing && !cantInput)
		{
			dashHaloCD = 0;
			//sprite yönü
			if (!((isRightWalled || isLeftWalled) && velocity.Y >= wallSpeed))
			{
				if (lastDir < 0)
				{
					characterSprite.Scale = new Vector2(-firstScale.X,firstScale.Y);
				}
				else if(lastDir > 0)
				{
					characterSprite.Scale = firstScale;
				}	
			}
			//Run
			if (Input.IsActionPressed("Right"))
			{
				if (velocity.X <= Speed * 1.3f)
				{
					dir = 1;
				}
				else
				{
					if (!IsOnFloor())
					{
						dir = 1.3f;
					}
				}
				lastDir = 1;
			}
			else if (Input.IsActionPressed("Left"))
			{
				if (velocity.X >= -Speed * 1.3f)
				{
					dir = -1;
				}
				else
				{
					if (!IsOnFloor())
					{
						dir = -1.3f;
					}
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
					characterSprite.Play("Jump");
					anim.Play("Jump");
					anim.Seek(0);
					characterSprite.Frame = 0;
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
					characterSprite.Play("Jump");
					anim.Play("Jump");
					anim.Seek(0);
					characterSprite.Frame = 0;
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
		AttemptCorrectionX(3);
		canJump = true;
		canAnim = true;
		pd.lastDir = lastDir;
	}

	public void AddForce(Vector2 vel)
	{
		if (!cantInput)
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
				characterSprite.CallDeferred("play", "Jump");
				characterSprite.SetDeferred("frame", 0);
				SetDeferred("canAnim", false);
			}	
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
		if (TestMove(GlobalTransform, new Vector2(velocity.X * delta, 0)) && !IsOnFloor() && velocity.Y >= 0)
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
		canDie = false;
		cantInput = true;
		restartAnim = true;
		col.Disabled = true;
		velocity = Vector2.Zero;
		Velocity = Vector2.Zero;
		dieAnim.Play("Die");
		t1 = 0;
		center = (GlobalPosition + spawnPos) / 2;
		center.Y += GlobalPosition.DistanceTo(spawnPos) * 0.9f;
		start = GlobalPosition - center;
		end = spawnPos - center;
		anim.CallDeferred("seek", 0);
		anim.CallDeferred("play","Died");
		dieTimer = 0.3f;
		haloTimer = 0;
	}
	void AnimFinished(string animName)
	{
		if (animName == "Die")
		{
			isZjustPressed = false;
			isDashing = false;
			dashD = 0;
		}
	}

	void ReStartAnim()
	{
		Vector2 result = start.Slerp(end,t1);
		GlobalPosition = center + result;
		//Effect Halo
		if (haloTimer > 0)
		{
			haloTimer -= (float)GetPhysicsProcessDeltaTime();
		}
		else
		{
			Effect ef = baloonHaloEfs.Dequeue();
			ef.GlobalPosition = characterSprite.GlobalPosition;
			ef.setOn();
			baloonHaloEfs.Enqueue(ef);	
			haloTimer = 0.05f;
		}
		if (GlobalPosition.DistanceTo(spawnPos) < 1)
		{
			if (birthTimer < -10)
			{
				birthTimer = 0.3f;	
			}
		}
	}
}
