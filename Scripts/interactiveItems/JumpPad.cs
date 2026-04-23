using Godot;
using System;

public partial class JumpPad : Area2D
{
	Tween t;
	[Export] Sprite2D jumpPad;
	[Export] Sprite2D jumpPadChains;
	[Export] float jumpVel;
	[Export] float duration;
	[Export] Camera2d camera;
	AnimationPlayer anim;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 targetPos = new Vector2(0,-16);
		jumpPadChains.GlobalPosition = GlobalPosition + targetPos.Rotated(GlobalRotation);
	}

	void BodyEntered2D(Node2D body)
	{
		if (body is Character)
		{
			camera.Shake(5);
			Character player = (Character)body;
			if (GlobalRotationDegrees == 0)
			{
				player.AddForce(new Vector2(0,-jumpVel));
				player.dashCD = 0.02f;
			}
			else if (GlobalRotationDegrees == -90)
			{
				player.AddForce(new Vector2(-jumpVel,-jumpVel/1.3f));
				player.dashCD = player.dashCoolDown / 4;
			}
			else if (GlobalRotationDegrees == 90)
			{
				player.AddForce(new Vector2(jumpVel,-jumpVel/1.3f));
				player.dashCD = player.dashCoolDown / 4;
			}
			//JumpAnim
			jumpPad.Position = new Vector2(0,0);
			t?.Kill();
			t = CreateTween();
			t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
			t.TweenProperty(jumpPad, "position", new Vector2(0,-16), duration).Finished += () =>
			{
				t?.Kill();
				t = CreateTween();
				t.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Linear);
				t.TweenProperty(jumpPad, "position", new Vector2(0,0), duration);
			};
		}
	}
}
