using Godot;
using System;

public partial class LockedDoor : StaticBody2D
{
	PlayerData pd;
	[Export] int Id;
	[Export] AnimationPlayer anim;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
		if (pd.LockedDoors.ContainsKey(Id))
		{
			anim.Play("Set");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
