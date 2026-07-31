using Godot;
using System;

public partial class Lever : StaticBody2D
{
	bool isLocked;
	PlayerData pd;
	[Export] Camera2d cam;
	[Export] int Id;
	[Export] AnimationPlayer anim;
	[Export] CollisionShape2D col;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
		if (pd.LockedDoors.ContainsKey(Id))
		{
			anim.Play("SetRot");
			col.CallDeferred("set_disabled",true);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void TakeDamage(float damage)
	{
		if (!isLocked)
		{
			col.CallDeferred("set_disabled", true);
			isLocked = true;
			pd.LockedDoors.Add(Id,"opened");
			anim.Play("Turn");
			cam.Shake(15);
		}
	}
}
