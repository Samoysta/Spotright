using Godot;
using System;
using System.ComponentModel;

public partial class Door : Area2D
{
	bool canSelect;
	bool selected;
	[Export] int doorId;
	[Export] Character character;
	[Export] AnimationPlayer anim;
	[Export] Node2D TextBox;
	[Export] string sceneName;
	Tween t;
	PlayerData pd;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (canSelect && !character.isDashing)
		{
			if (Input.IsActionJustPressed("Down") && character.IsOnFloor() && !character.cantInput)
			{
				if (!selected)
				{
					selected = true;
					character.cantInput = true;
					character.AddForce(new Vector2(0,0));
					character.velocity = Vector2.Zero;
					character.Velocity = Vector2.Zero;
					character.characterSprite.Play("Idle");
					anim.Play("FadeIn");	
					t?.Kill();
					t = CreateTween();
					t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Linear);
					t.TweenProperty(TextBox, "scale", new Vector2(0,0), 0.7f);
					pd.doorID = doorId;
				}
			}	
		}
	}

	public void BodyEntered2D(Node2D body)
	{
		if (body == character)
		{
			t?.Kill();
			t = CreateTween();
			t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
			t.TweenProperty(TextBox, "scale", new Vector2(1,1), 0.7f);
			canSelect = true;
		}
	}

	public void BodyExited2D(Node2D body)
	{
		if (body == character)
		{
			t?.Kill();
			t = CreateTween();
			t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Linear);
			t.TweenProperty(TextBox, "scale", new Vector2(0,0), 0.3f);
			canSelect = false;
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
		}
	}

	public void Starting()
	{
		anim.Play("FadeOut");
	}

}
