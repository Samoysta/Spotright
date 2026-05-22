using Godot;
using System;

public partial class Lamp : Node2D
{
	[Export] Character character;
	[Export] float rotDeg;
	float targetDeg;
	Tween t;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public void BodyEntered2D(Node2D body)
	{
		if (character.velocity.X != 0)
		{
			if (body is Character)
			{
				if (Mathf.Abs(character.velocity.X) > character.Speed / 3)
				{
					float a = character.velocity.X * (rotDeg / character.Speed);
					t?.Kill();
					t = CreateTween();
					t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
					targetDeg = a;
					a = Mathf.Clamp(a,-rotDeg,rotDeg);
					float b = GlobalRotationDegrees + -a;
					b = Mathf.Clamp(b,-rotDeg,rotDeg);
					t.TweenProperty(this, "global_rotation_degrees", -a + GlobalRotationDegrees, 0.3f).Finished += () =>
					{
						t?.Kill();
						t = CreateTween();
						t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
						t.TweenProperty(this, "global_rotation_degrees", 0, 5f);
						targetDeg = 0;
					};		
				}
			}	
		}
	}
}
