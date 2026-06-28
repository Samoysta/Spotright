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
		CallDeferred("setUp");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	public void setUp()
	{
		foreach (Node2D child in pd.Items.GetChildren())
		{
			if (child is Weapon1)
			{
				anim.Play("Starting");
			}
		}
	}

	public void AnimPlay(string animName)
	{
		if (animName == "Starting")
		{
			locked = false;
		}
		if (!locked)
		{
			anim.Play(animName);
			anim.Seek(0);	
		}
		if (animName == "Ending")
		{
			locked = true;
		}
	}
}
