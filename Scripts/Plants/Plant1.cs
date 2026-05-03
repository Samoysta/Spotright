using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class Plant1 : Area2D
{
	[Export] Sprite2D plantSprite;
	[Export] Tween.TransitionType tweener;
	[Export] float dur;
	[Export] bool isCeiling;
	Tween tween;
	bool isShaking;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tween?.Kill();
		tween = CreateTween();
		tween.SetEase(Tween.EaseType.InOut).SetTrans(tweener);
		tween.TweenProperty(plantSprite,"skew", Mathf.DegToRad(10), dur).Finished += () => {AnimFinished();};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		

	}

	void AnimFinished()
	{
		int target = 0;
		if (plantSprite.Skew == Mathf.DegToRad(10))
		{
			target = -10;
		}
		else
		{
			target = 10;
		}
		if (!isShaking)
		{
			tween?.Kill();
			tween = CreateTween();
			tween.SetEase(Tween.EaseType.InOut).SetTrans(tweener);
			tween.TweenProperty(plantSprite,"skew", Mathf.DegToRad(target), dur).Finished += () => {AnimFinished();};	
		}
	}

	void BodyEntered2D(Node2D body)
	{
		int dir = 0;
		if (body is Character)
		{
			isShaking = true;
			CharacterBody2D character = (CharacterBody2D)body;
			if (character.Velocity.X > 0)
			{
				if (isCeiling)
				{
					dir = -1;
				}
				else
				{
					dir = 1;
				}
			}
			else if (character.Velocity.X < 0)
			{
				if (isCeiling)
				{
					dir = 1;
				}
				else
				{
					dir = -1;
				}
			}
			tween?.Kill();
			tween = CreateTween();
			tween.SetEase(Tween.EaseType.Out).SetTrans(tweener);
			tween.TweenProperty(plantSprite,"skew", Mathf.DegToRad(dir * 30), dur / 4).Finished += () => {AnimFinished();};
			isShaking = false;	
		}
	}
	void OnScreenEntered()
	{
		plantSprite.Visible = true;
	}

	void OnScreenExited()
	{
		plantSprite.Visible = false;
	}
}
