using Godot;
using System;

public partial class Door2 : Area2D
{
	[Export] int dir;
	[Export] int doorId;
	[Export] Character character;
	[Export] AnimationPlayer anim;
	[Export] CollisionShape2D col;
	[Export] string sceneName;
	PlayerData pd;
	bool entered;
	bool exited;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
		if (pd.doorID == doorId)
		{
			exited = true;
			col.CallDeferred("set_disabled",true);
			character.characterSprite.Play("Run");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (entered)
		{
			character.Velocity = new Vector2(dir * character.Speed,character.Velocity.Y);		
		}
		else if (exited)
		{
			character.Velocity = new Vector2(-dir * character.Speed,character.Velocity.Y);	
		}
	}
    public override void _PhysicsProcess(double delta)
    {
		
    }


	public void BodyEntered2D(Node2D body)
	{
		if (body == character)
		{
			entered = true;
			pd.doorID = doorId;
			anim.Play("FadeIn");
			character.cantInput = true;
			col.CallDeferred("set_disabled",true);
		}
	}
	public void AnimFinished(string name)
	{
		if (name == "FadeIn")
		{
			pd.Items.Reparent(GetTree().Root);
			pd.lastDir = dir;
			GetTree().ChangeSceneToFile($"res://Scenes/Levels/{sceneName}.tscn");
		}
		else if (name == "FadeOut")
		{
			character.cantInput = false;
			col.CallDeferred("set_disabled",false);
			exited = false;
		}
	}
	public void Starting()
	{
		anim.Play("FadeOut");
	}
}
