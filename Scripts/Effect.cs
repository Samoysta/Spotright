using Godot;
using System;
using System.Linq;

public partial class Effect : Node2D
{
	[Export] AnimationPlayer[] anims;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void setOn()
	{
		for (int i = 0; i < anims.Count(); i++)
		{
			anims[i].Play("Start");
			anims[i].Seek(0);
		}
	}
}
