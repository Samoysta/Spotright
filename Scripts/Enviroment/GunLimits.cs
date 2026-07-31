using Godot;
using System;

public partial class GunLimits : Node2D
{
	[Export] AnimationPlayer anim;
	PlayerData pd;
	bool locked;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
}
