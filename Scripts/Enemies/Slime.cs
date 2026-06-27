using Godot;
using System;
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
	RandomNumberGenerator rnd = new();
	PlayerData pd;
	int dir;
	[Export] RayCast2D ray;
	[Export] RayCast2D wallRay;
	bool inProcees;
	bool running;
	float t1;
	Vector2 velocity;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		rnd.Randomize();
		pd = GetNode<PlayerData>("/root/PlayerData");
		if (pd.killedEnemies.ContainsKey(id))
		{
			if (pd.killedEnemies[id] == "killed")
			{
				QueueFree();
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
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
		velocity.Y = Velocity.Y;
		if (!IsOnFloor())
		{
			velocity += 2 * GetGravity() * (float)delta;
		}
		if (!inProcees)
		{
			running = !running;
			t1 = 2;
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
		health -= dam;	
		if (health <= 0)
		{
			if (!pd.killedEnemies.ContainsKey(id))
			{
				pd.killedEnemies.Add(id, "killed");	
			}
			QueueFree();
		}
	}
}
