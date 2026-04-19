using Godot;
using System;
using System.Linq;

public partial class FallebleStone : CharacterBody2D
{
	[Export] Camera2d camera;
	[Export] RayCast2D[] rayCasts;
	Vector2 vel;
	bool falling;
	bool dead;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (!dead)
		{
			vel = Velocity;
			for (int i = 0; i < rayCasts.Count(); i++)
			{
				if (rayCasts[i].IsColliding())
				{
					Node2D body = (Node2D)rayCasts[i].GetCollider();
					if (body is Character)
					{
						falling = true;
						GD.Print("nig");
					}
				}
			}
			if (falling)
			{
				if (!IsOnFloor())
				{
					vel += GetGravity() * (float)delta;
				}
				else
				{
					camera.Shake(10);
					falling = false;
					dead = true;
				}
			}
		}
		vel.Y = Mathf.Clamp(vel.Y, -1000, 2000);
		Velocity = vel;
		MoveAndSlide();	
	}

	public void BodyEntered2D(Node2D body)
	{
		if (body is Character)
		{
			body.Call("KillSelf");
		}
	}
}
