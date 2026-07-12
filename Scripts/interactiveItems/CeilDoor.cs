using Godot;
using System;

public partial class CeilDoor : Area2D
{
	PlayerData pd;
	[Export] int doorId;
	[Export] Character character;
	[Export] string sceneName;
	[Export] float jumpSpeed;
	[Export] float xSpeed;
	[Export] bool isFloored;
	[Export] CollisionShape2D col;
	[Export] AnimationPlayer anim;
	[Export] Node2D spawnPos;
	bool applyGravity;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
		if (pd.doorID == doorId)
		{
			col.Disabled = true;
			applyGravity = true;
			character.GlobalPosition = GlobalPosition;
			if (!isFloored)
			{
				character.characterSprite.Play("Fall");
			}
			else
			{
				character.Velocity = new Vector2(xSpeed, -jumpSpeed);
				character.characterSprite.Play("Jump");
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (applyGravity)
		{
			character.Velocity += character.GetGravity() * 2 * (float)delta;
		}
	}
	public void BodyEntered2D(Node2D body)
	{
		if (body is Character)
		{
			anim.Play("FadeIn");
			if (!isFloored)
			{
				pd.doorID = doorId;
				character.cantInput = true;
				character.Velocity = new Vector2(character.Velocity.X, -character.JumpVelocity);
			}
		}
	}
	public void AnimFinished(string name)
	{
		if (name == "FadeIn")
		{
			pd.Items.Reparent(GetTree().Root);
			GetTree().ChangeSceneToFile($"res://Scenes/Levels/{sceneName}.tscn");
		}
		else if (name == "FadeOut")
		{
			character.cantInput = false;
			col.Disabled = false;
			applyGravity = false;
		}
	}

	public void Starting()
	{
		anim.Play("FadeOut");
	}
}
