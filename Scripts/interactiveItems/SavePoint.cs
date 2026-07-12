using Godot;
using System;

public partial class SavePoint : Area2D
{
	PlayerData pd;
	[Export] Node2D textBox;
	[Export] Character character;
	[Export] Camera2d cam;
	[Export] Node2D spawnPos;
	Tween t;
	Tween t2;
	bool canSave;
	bool locked;
	int tryAmount;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
		if (character.GlobalPosition == spawnPos.GlobalPosition)
		{
			character.cantInput = true;
			character.characterSprite.Play("Die");
			character.characterSprite.Frame = 11;
			character.borning = true;
			locked = true;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (canSave)
		{
			if (Input.IsActionJustPressed("Down"))
			{
				canSave = false;
				t?.Kill();
				t = CreateTween();
				t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
				t.TweenProperty(textBox, "scale", new Vector2(0, 0), 0.5f);
				pd.savedPos = spawnPos.GlobalPosition;
				pd.savedScene = GetTree().CurrentScene.Name;
				character.cantInput = true;
				character.velocity = Vector2.Zero;
				character.Velocity = Vector2.Zero;
				t2?.Kill();
				t2 = CreateTween();
				t2.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
				t2.TweenProperty(character, "global_position", pd.savedPos, 0.5f).Finished += () =>
				{
					locked = true;	
				};
				character.characterSprite.Play("Die");
			}
		}
		if (locked && !character.borning)
		{
			if (Input.IsActionJustPressed("Z"))
			{
				cam.Shake(7f);
				if (tryAmount < 3)
				{
					tryAmount++;
				}
				else
				{
					locked = false;
					character.cantInput = false;
					character.AddForce(new Vector2(0,-500));
					tryAmount = 0;
				}
			}
		}
	}
	public void BodyEntered2D(Node2D body)
	{
		if (body is Character)
		{
			canSave = true;
			t?.Kill();
			t = CreateTween();
			t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
			t.TweenProperty(textBox, "scale", new Vector2(1, 1), 0.8f);
		}
	}

	public void BodyExited2D(Node2D body)
	{
		if (body is Character)
		{
			canSave = false;
			t?.Kill();
			t = CreateTween();
			t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
			t.TweenProperty(textBox, "scale", new Vector2(0, 0), 0.5f);
		}
	}
}
