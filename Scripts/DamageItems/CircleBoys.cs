using Godot;
using System;

public partial class CircleBoys : Area2D
{
	[Export] float speed;
	RandomNumberGenerator rnd = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		rnd.Randomize();
		int a = rnd.RandiRange(0,1);
		if (a == 1)
		{
			speed *= -1;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GlobalRotationDegrees += speed * (float)delta;
	}

	void BodyEntered2D(Node2D body)
	{
		if (body is Character)
		{
			body.Call("KillSelf");
		}
	}
}
