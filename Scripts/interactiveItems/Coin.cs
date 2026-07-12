using Godot;
using System;
using System.Collections;

public partial class Coin : CharacterBody2D
{
	Vector2 velocity;
	int jumpAmount = 5;
	PlayerData pd;
	public Character character;
	[Export] AnimationPlayer anim;
	bool taked;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (jumpAmount <= 0 && !taked)
		{
			if (GlobalPosition.DistanceTo(character.GlobalPosition) < 14f)
			{
				pd.coin++;
				taked = true;
				anim.Play("Queue");
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
				else
				{
					velocity = Vector2.Zero;
				}

			}
			if (IsOnWall())
			{
				velocity.X *= -1;
			}
		}
		if (!IsOnFloor())
		{
			velocity += GetGravity() * 2 * (float)delta;
		}
		Velocity = velocity;
		MoveAndSlide();
	}

	public void AddForce(Vector2 vel)
	{
		velocity = vel;
	}

	public void AnimFinished(string animName)
	{
		if (animName == "Queue")
		{
			QueueFree();
		}
	}
}
