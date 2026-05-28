using Godot;
using System;
using System.Collections;

public partial class Coin : CharacterBody2D
{
	Vector2 velocity;
	int jumpAmount = 5;
	PlayerData pd;
	public Character character;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (jumpAmount <= 0)
		{
			velocity = Vector2.Zero;
			if (GlobalPosition.DistanceTo(character.GlobalPosition) < 14f)
			{
				pd.coin++;
				QueueFree();
			}
		}
		else
		{
			if (IsOnFloor())
			{
				jumpAmount--;
				if (jumpAmount > 0)
				{
					velocity.X *= 0.5f;
					velocity.Y *= -0.5f;
				}
			}
			else
			{
				velocity += GetGravity() * 2 * (float)delta;
			}
			if (IsOnWall())
			{
				velocity.X *= -1;
			}
		}
		Velocity = velocity;
		MoveAndSlide();
	}

	public void AddForce(Vector2 vel)
	{
		velocity = vel;
	}
}
