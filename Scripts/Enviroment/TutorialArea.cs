using Godot;
using System;

public partial class TutorialArea : Area2D
{
	[Export] AnimationPlayer anim;
	[Export] Character character;
	[Export] string tutoName;
	[Export] string text;
	[Export] Label textBox;
	bool isPlayed;
	bool isFaded;
	PlayerData pd;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pd = GetNode<PlayerData>("/root/PlayerData");
		textBox.Text = text;
		if (pd.showedTutos.Contains(tutoName))
		{
			QueueFree();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!isPlayed && isFaded)
		{
			if (Input.IsActionJustPressed(tutoName))
			{
				bool canClose = true;
				if (tutoName == "X")
				{
					canClose = false;
					if (character.currentAbilityNo == 1)
					{
						canClose = true;
					}
				}
				if (canClose)
				{
					anim.Play("FadeIn");
					isPlayed = true;
					pd.showedTutos.Add(tutoName);
				}
			}
		}
	}

	public void BodyEntered2D(Node2D body)
	{
		if (body is Character)
		{
			if (!isFaded)
			{
				bool canFade = true;
				if (tutoName == "X")
				{
					canFade = false;
					if (pd.openedAbilityIds.Contains(1))
					{
						canFade = true;
					}
				}
				if (canFade)
				{
					anim.Play("FadeOut");	
					isFaded = true;
				}	
			}
		}
	}
}
