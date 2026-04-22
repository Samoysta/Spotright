using Godot;
using System;
using System.Linq;

public partial class FallebleStone : CharacterBody2D
{
	[Export] Camera2d camera;
	[Export] RayCast2D[] rayCasts;
	[Export] AnimatedSprite2D[] smokeEfects;
	[Export] CpuParticles2D hitEffect;
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
				if (rayCasts[i].IsColliding() && !falling)
				{
					Node2D body = (Node2D)rayCasts[i].GetCollider();
					if (body is Character)
					{
						falling = true;
						GD.Print("nig");
						break;
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
					for (int i = 0; i < smokeEfects.Count(); i++)
					{
						smokeEfects[i].Play("Start");
					}
					hitEffect.Emitting = true;
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
}
