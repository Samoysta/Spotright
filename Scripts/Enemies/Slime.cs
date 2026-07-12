using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;

public partial class Slime : CharacterBody2D
{
	[Export] int id;
	[Export] int health;
	[Export] AnimatedSprite2D sprite;
	[Export] int damage;
	[Export] Vector2 damageForce;
	[Export] float speed;
	[Export] bool reversed;
	[Export] Camera2d cam;
	RandomNumberGenerator rnd = new();
	PlayerData pd;
	int dir;
	[Export] RayCast2D ray;
	[Export] RayCast2D wallRay;
	[Export] float SpeedCoin;
	bool inProcees;
	bool running;
	float t1;
	Vector2 velocity;
	[Export] Character character;
	[Export] int coinAmount;
	[Export] CollisionShape2D col;
	//Money
	[Export] PackedScene Coin;
	Coin[] coins;
	[Export] PackedScene slimeBloodEf;
	public Queue<Effect> slimeBloodEfs = new ();
	[Export] AnimationPlayer anim;
	[Export] CpuParticles2D dieEf;
	CollisionShape2D thisCol;
	bool died;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		thisCol = GetNode<CollisionShape2D>("CollisionShape2D");
		coins = new Coin[coinAmount];
		rnd.Randomize();
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
		if (reversed)
		{
			sprite.Scale *= new Vector2(-1,1);
			dir = -1;
		}
		else
		{
			dir = 1;
		}
		for (int i = 0; i < 9; i++)
		{
			Effect ef = (Effect)slimeBloodEf.Instantiate();
			GetTree().CurrentScene.CallDeferred("add_child", ef);
			slimeBloodEfs.Enqueue(ef);
			ef.Visible = false;
		}
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
		if (!died)
		{
			col.CallDeferred("set_disabled", !character.canTakeDamage);
			velocity.Y = Velocity.Y;
			if (!IsOnFloor())
			{
				velocity += 2 * GetGravity() * (float)delta;
			}
			if (!inProcees)
			{
				running = !running;
				t1 = rnd.RandfRange(1,3);
				inProcees = true;
			}
			else
			{
				if (t1 > 0)
				{
					t1 -= (float)delta;
				}
				else
				{
					inProcees = false;
				}
				if (running)
				{
					velocity.X = dir * speed;
					if (!ray.IsColliding() || (wallRay.IsColliding() && IsOnWall()))
					{
						dir *= -1;
						sprite.Scale *= new Vector2(-1,1);
					}
				}
				else
				{
					velocity.X = 0;
				}
			}
			Velocity = velocity;
			MoveAndSlide();
		}
		
		if (died)
		{
			if (!dieEf.Emitting)
			{
				QueueFree();
			}
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

	public void TakeDamage(int dam)
	{
		anim.Play("TakeDamage");
		anim.Seek(0);
		health -= dam;	
		if (health <= 0)
		{
			if (!pd.killedEnemies.ContainsKey(id))
			{
				pd.killedEnemies.Add(id, "killed");
				SpawnCoin();	
			}
			sprite.Visible = false;
			col.CallDeferred("set_disabled",true);
			thisCol.CallDeferred("set_disabled", true);
			dieEf.Emitting = true;
			died = true;
			cam.Shake(10);
		}
	}
	public void SetDamageEf(Vector2 dir)
	{
		Effect ef = slimeBloodEfs.Dequeue();
		ef.GlobalPosition = GlobalPosition;
		CpuParticles2D ef2 = ef.GetNode<CpuParticles2D>("bloodEf");
		ef2.Direction = dir;
		ef.setOn();
		slimeBloodEfs.Enqueue(ef);
	}

	public void SpawnCoin()
	{
		for (int i = 0; i < coinAmount; i++)
		{
			float degree = rnd.RandfRange(75,105);
			Coin co = coins[i];
			co.GlobalPosition = GlobalPosition;
			co.Visible = true;
			co.ProcessMode = ProcessModeEnum.Always;
			co.character = character;
			co.AddForce(new Vector2(SpeedCoin,0).Rotated(degree));
		}
	}
}
