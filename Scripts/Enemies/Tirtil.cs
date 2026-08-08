using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public partial class Tirtil : CharacterBody2D
{
	[Export] int id;
	[Export] int health;
	[Export] int damage;
	[Export] float speed;
	[Export] RayCast2D ray1;
	[Export] Vector2 damageForce;
	[Export] int coinAmount;
	RandomNumberGenerator rnd = new();
	[Export] Character character;
	[Export] float SpeedCoin;
	[Export] PackedScene Coin;
	[Export] Camera2d cam;
	Coin[] coins;
	PlayerData pd;
	[Export] float Timer1;
	float timer1;
	float timer2;
	bool running;
	Vector2 velocity;
	[Export] CollisionShape2D col;
	bool beganing;
	[Export] PackedScene bloodEf;
	public Queue<Effect> bloodEfs = new();
	[Export] float dieTimer;
	float dTimer;
	[Export] CpuParticles2D dieEf;
	[Export] CollisionShape2D mainCol;
	[Export] AnimatedSprite2D eyes;
	[Export] AnimationPlayer anim;
	[Export] CpuParticles2D sleepyEf;
	[Export] CpuParticles2D awakeEf;
	[Export] AnimationPlayer anim2;
	float timer3;
	Tween t;
	[Export] CpuParticles2D dustEf;
	AnimatedSprite2D hitEf;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		hitEf = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sleepyEf.Emitting = true;
		rnd.Randomize();
		coins = new Coin[coinAmount];
		pd = GetNode<PlayerData>("/root/PlayerData");
		if (pd.killedEnemies.ContainsKey(id))
		{
			if (pd.killedEnemies[id] == "killed")
			{
				QueueFree();
			}
		}
		else
		{
			for (int i = 0; i < coinAmount; i++)
			{
				Coin co = (Coin)Coin.Instantiate();
				co.GlobalPosition = GlobalPosition;
				GetTree().CurrentScene.CallDeferred("add_child",co);
				co.Visible = false;
				co.ProcessMode = ProcessModeEnum.Disabled;
				coins[i] = co;
			}	
		}

		for (int i = 0; i < 12; i++)
		{
			Effect ef = (Effect)bloodEf.Instantiate();
			ef.Scale = new Vector2(1,1);
			GetTree().CurrentScene.CallDeferred("add_child", ef);
			ef.Visible = false;
			bloodEfs.Enqueue(ef);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		dustEf.Emitting = Mathf.Abs(velocity.X) >= speed;
		if (health > 0)
		{
			ray1.LookAt(character.GlobalPosition);
			col.CallDeferred("set_disabled", !character.canTakeDamage);
			if (running)
			{
				if (Velocity.X == 0)
				{
					if (velocity.X > 0)
					{
						hitEf.GlobalPosition = GlobalPosition + new Vector2(32,-32);
					}
					else
					{
						hitEf.GlobalPosition = GlobalPosition + new Vector2(-32,-32);
					}
					velocity.X *= -1;
					cam.Shake(10);
					hitEf.Play("default");
					hitEf.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("default");
				}
			}
			if (timer3 > 0)
			{
				timer3 -= (float)delta;
			}
			if (timer1 > 0)
			{
				timer1 -= (float)delta;
				if (timer1 <= 0)
				{
					running = false;
					eyes.Frame = 0;
					sleepyEf.Emitting = true;
					timer3 = 1f;
					t?.Kill();
					t = CreateTween();
					t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
					t.TweenProperty(this, "velocity", Vector2.Zero, 0.5f);
				}
			}
			if (timer2 > 0)
			{
				timer2 -= (float)delta;
				if (timer2 <= 0)
				{
					running = true;
					beganing = false;
					timer1 = Timer1;
					eyes.Frame = 2;
					velocity.X = Mathf.Sign(character.GlobalPosition.X - GlobalPosition.X) * speed;
				}
			}
			if (ray1.IsColliding())
			{
				Node2D body = (Node2D)ray1.GetCollider();
				if (body is Character)
				{
					if (!running && !beganing && timer3 <= 0)
					{
						timer2 = 0.5f;
						beganing = true;
						awakeEf.Emitting = true;
						anim2.Play("WakeUp");
						anim2.Seek(0);
						sleepyEf.Emitting = false;
						eyes.Frame = 1;
						velocity.X = 0;
						t?.Kill();
					}
				}
			}
		}	
		if (dTimer > 0)
		{
			dTimer -= (float)delta;
			if (dTimer <= 0)
			{
				QueueFree();
			}
		}
		Velocity = velocity;
		MoveAndSlide();
		
	}

	public void TakeDamage(int damage)
	{
		pd.energyAmount++;
		health -= damage;
		anim.Play("Flash");
		anim.Seek(0);
		if (!running && !beganing && timer3 <= 0)
		{
			timer2 = 0.5f;
			beganing = true;
			awakeEf.Emitting = true;
			anim2.Play("WakeUp");
			anim2.Seek(0);
			sleepyEf.Emitting = false;
			eyes.Frame = 1;
			velocity.X = 0;
			t?.Kill();
		}
		if (health <= 0)
		{
			SpawnCoin();
			col.CallDeferred("set_disabled",true);
			mainCol.CallDeferred("set_disabled", true);
			mainCol.Visible = false;
			dTimer = dieTimer;
			dieEf.Emitting = true;
			velocity.X = 0;
			cam.Shake(20);
			if (!pd.killedEnemies.ContainsKey(id))
			{
				pd.killedEnemies.Add(id,"killed");	
			}
			sleepyEf.Emitting = false;			
		}
	}
	public void SetDamageEf(Vector2 dir)
	{
		Effect ef = bloodEfs.Dequeue();
		ef.GlobalPosition = GlobalPosition;
		ef.LookAt(GlobalPosition + dir);
		ef.setOn();
		bloodEfs.Enqueue(ef);
	}

	public void SpawnCoin()
	{
		for (int i = 0; i < coinAmount; i++)
		{
			float degree = rnd.RandfRange(-75,-105);
			Coin co = coins[i];
			co.GlobalPosition = GlobalPosition;
			co.Visible = true;
			co.ProcessMode = ProcessModeEnum.Always;
			co.character = character;
			co.AddForce(new Vector2(SpeedCoin,0).Rotated(degree));
		}
	}

	public void BodyEntered2D(Node2D body)
	{
		if (body is Character)
		{
			int dir = Mathf.Sign(body.GlobalPosition.X - GlobalPosition.X);
			body.Call("TakeDamage", damage, new Vector2(dir * damageForce.X,-damageForce.Y));
		}
	}
}
