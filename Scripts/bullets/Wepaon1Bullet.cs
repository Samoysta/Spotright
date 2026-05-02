using Godot;
using System;

public partial class Wepaon1Bullet : Node2D
{
	[Export] RayCast2D ray;
	[Export] Line2D line;
	[Export] float speed;
	Character character;
	float velocity;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		velocity = speed * (float)delta;
		MoveLocalX(velocity);
		ray.TargetPosition = new Vector2(velocity,0);
		line.SetPointPosition(1,new Vector2(velocity,0));
		if (ray.IsColliding())
		{
			Node2D body = (Node2D)ray.GetCollider();
			if (!(body is Character))
			{
				if (body.HasMethod("TakeDamage"))
				{
					body.Call("TakeDamage");
				}
				setOff();
				spawnEffect();
			}
		}
	}
	public void Init(Character cha)
	{
		character = cha;
	}

	public void setOff()
	{
		Visible = false;
		ray.Enabled = false;
		character.bul1s.Enqueue(this);
		SetProcess(false);
		SetPhysicsProcess(false);
	}

	public void setOn()
	{
		ray.TargetPosition = new Vector2(velocity,0);
		ray.Enabled = true;
		Visible = true;
		SetProcess(true);
		SetPhysicsProcess(true);
	}

	public void spawnEffect()
	{
		Effect ef = character.hitEffects.Dequeue();
		ef.GlobalPosition = ray.GetCollisionPoint();
		ef.GlobalRotation = GlobalRotation;
		ef.setOn();
		character.hitEffects.Enqueue(ef);
	}
}
