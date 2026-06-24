using Godot;
using System;

public partial class Slime : CharacterBody2D
{
	[Export] int health;
	[Export] int damage;
	[Export] Vector2 damageForce;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
			QueueFree();
		}
	}
}
