using Godot;
using System;

public partial class Wepaon1Bullet : Node2D
{
	[Export] ShapeCast2D ray;
	[Export] Line2D line;
	[Export] float speed;
	Character character;
	float velocity;
	int damage;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		velocity = speed * (float)delta;
		MoveLocalX(velocity);
		ray.TargetPosition = new Vector2(velocity * 2,0);
		line.SetPointPosition(1,new Vector2(velocity * 2,0));
		if (ray.IsColliding())
		{
			Node2D body = (Node2D)ray.GetCollider(0);
			if (!(body is Character))
			{
				if (body.HasMethod("TakeDamage"))
				{
					body.Call("TakeDamage",damage);
				}
				Node2D gunLim = null;
				if (body.GetParent().GetParent() is Node2D)
				{
					gunLim = (Node2D)body.GetParent().GetParent();
				}
				if (gunLim is GunLimits)
				{
					gunLim.Call("AnimPlay","Hitted");
				}
				setOff();
				spawnEffect();
			}
		}
	}
	public void Init(Character cha)
	{
		character = cha;
		damage = character.weaponDamage;
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
		ef.GlobalPosition = ray.GetCollisionPoint(0);
		ef.GlobalRotation = GlobalRotation;
		ef.setOn();
		character.hitEffects.Enqueue(ef);
	}
}
