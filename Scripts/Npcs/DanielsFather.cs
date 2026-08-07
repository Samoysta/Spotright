using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public partial class DanielsFather : Node2D
{
    [Export] Character character;
	Camera2d cam;
    [Export] Node2D readBox;
    Tween t;
    bool characterEntered;
    bool canRead;
    [Export] string[] Texts;
    [Export] float[] textSpeeds;
    RichTextLabel text;
    [Export] float textSpeed;
    [Export] AudioStreamPlayer2D popAudio;
    bool textingStarted;
    int currentTextId;
    int textBoxPastCharacterAmount;
    [Export] AudioStreamPlayer2D textBoxAudio;
    bool isSelecting;
    int selectIndex;
    PlayerData pd;
    Tween t2;
    [Export] Node2D characterTargetPos;
    bool SetUpAnim;
    bool canSelect;
    Tween t3;
	[Export] AnimationTree animtree;
	[Export] PackedScene heart;
	[Export] bool startSpawn;
	bool spawned;
	[Export] CpuParticles2D takeCpu;
	[Export] Node2D heartSpawnPos;
	Node2D currentheart;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        pd = GetNode<PlayerData>("/root/PlayerData");
        character.dialogAnim.AnimationFinished += (animName) => {AnimFinished(animName);};
        text = character.dialogText;
        text.VisibleRatio = 0;
        for (int i = 0; i < Texts.Length; i++)
        {
            Texts[i] = Texts[i].Replace("\\n","\n");
			Texts[i] = Texts[i].Replace("//","/");
        }
		if (pd.talkedNpcs.Contains("DanielsFather"))
		{
			currentTextId = 12;
			if (!pd.takedHearts.Contains("Forest"))
			{
				CallDeferred("SpawnHeart");
			}
		}
		cam = character.camera;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
		if (!spawned)
		{
			if (startSpawn)
			{
				SpawnHeart();
				cam.Shake(20);
				spawned = true;
			}
		}
        if (SetUpAnim)
        {
            character.lastDir = Mathf.Sign(characterTargetPos.GlobalPosition.X - character.GlobalPosition.X);
            character.velocity.X = Mathf.Sign(characterTargetPos.GlobalPosition.X - character.GlobalPosition.X) * character.Speed / 2;
            if (Mathf.Abs(character.GlobalPosition.X - characterTargetPos.GlobalPosition.X) < character.Speed / 2 * (float)delta)
            {
                SetUpAnim = false;
                character.dialogAnim.Play("Opening");
                character.velocity = Vector2.Zero;
                character.Velocity = Vector2.Zero;
                character.lastDir = -1;
                character.characterSprite.Play("Idle");
            }
            character.Velocity = character.velocity;
        }
        if (characterEntered)
        {
            if (character.Velocity.Y == 0)
            {
                if (!canRead)
                {
                    canRead = true;
                    AskAnimStart();
                }
            }
            else
            {
                if (canRead)
                {
                    canRead = false;
                    AskAnimEnd();
                }
            }
        }
        else
        {
            if (canRead)
            {
                canRead = false;
                AskAnimEnd();
            }
        }

        if (canRead)
        {
            if (Input.IsActionJustPressed("Down") && !character.cantInput)
            {
                character.cantInput = true;
                character.velocity = Vector2.Zero;
                character.Velocity = Vector2.Zero;
                character.characterSprite.Play("Run");
                AskAnimEnd();
                SetUpAnim = true;
            }
        }

        if (textingStarted)
        {
            float i = textSpeeds[currentTextId];
            int visibleLength = Regex.Replace(Texts[currentTextId], @"\[.*?\]", "").Length;
            text.VisibleRatio += Mathf.Clamp(i * (float)delta * textSpeed * (1f / visibleLength),0,1);
            if (text.VisibleCharacters > 0)
            {
                string a = Texts[currentTextId].ElementAt(text.VisibleCharacters-1).ToString();
                if (a == " " || a == "\n")
                {
                    text.VisibleCharacters++;
                }   
            }
            if (isSelecting)
            {
                if (Input.IsActionJustPressed("Right") && canSelect)
                {
                    t2?.Kill();
                    t2 = CreateTween();
                    t2.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
                    t2.TweenProperty(character.buttons[selectIndex], "scale", new Vector2(1f,1f), 0.4f);
                    if (selectIndex == 1)
                    {
                        selectIndex = 0;
                    }
                    else
                    {
                        selectIndex++;
                    }
                    t?.Kill();
                    t = CreateTween();
                    t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
                    t.TweenProperty(character.buttons[selectIndex], "scale", new Vector2(1.3f,1.3f), 0.4f);
                }
                if (Input.IsActionJustPressed("Left") && canSelect)
                {
                    t?.Kill();
                    t = CreateTween();
                    t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
                    t.TweenProperty(character.buttons[selectIndex], "scale", new Vector2(1f,1f), 0.4f);
                    if (selectIndex == 0)
                    {
                        selectIndex = 1;
                    }
                    else
                    {
                        selectIndex = 0;
                    }
                    t2?.Kill();
                    t2 = CreateTween();
                    t2.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
                    t2.TweenProperty(character.buttons[selectIndex], "scale", new Vector2(1.3f,1.3f), 0.4f);
                }
            }
            if (Input.IsActionJustPressed("Z"))
            {
                if (text.VisibleRatio < 1 )
                {
                    text.VisibleRatio = 1;
                }
                else
                {
                    if (!isSelecting)
                    {
                        if (Texts.Length > currentTextId + 1)
                        {
                            if (new[] {9,11,12}.Contains(currentTextId))
                            {
                                character.dialogAnim.Play("Closing");
								textingStarted = false;
								if (currentTextId == 9)
								{
									animtree.Set("parameters/conditions/give", true);
									animtree.Set("parameters/conditions/idle", false);
									currentTextId++;
								}
								else if (currentTextId == 12 || currentTextId == 11)
								{
									currentTextId++;
								}
                            }
                            else
                            {
                                currentTextId ++;
                                text.VisibleRatio = 0;
                                text.Text = Texts[currentTextId];   
                            }     
                        }
                        else
                        {
                            character.dialogAnim.Play("Closing");
							textingStarted = false;
							currentTextId --;
                        }   
                    }
                    else
                    {
                        if (canSelect)
                        {
                            if (currentTextId == 4)
                            {
                                if (selectIndex == 0)
                                {
                                    currentTextId = 7;
                                    pd.openedAbilityIds.Add(1);
                                }
                                else
                                {
                                    currentTextId = 5;
                                }
                            }  
							SelectFinished(); 
                            canSelect = false;
                        }
                    }
                }
            }
            if (textBoxPastCharacterAmount != character.dialogText.VisibleCharacters)
            {
                textBoxPastCharacterAmount = character.dialogText.VisibleCharacters;
                if (textBoxPastCharacterAmount != 0)
                {
					if (!textBoxAudio.Playing)
					{
						textBoxAudio.Play();	
					}
                }
            }
        }
    }

    void AnimFinished(string animName)
    {
        if (animName == "Opening")
        {
            textingStarted = true;   
            text.VisibleRatio = 0;
            text.Text = Texts[currentTextId];
        }
        else if(animName == "Closing")
        {
            textingStarted = false;
            text.Text = "";
			if (currentTextId - 1 != 9)
			{
				character.cantInput = false;
				AskAnimStart();
			}

        }
    }

    void AskAnimStart()
    {
        t?.Kill();
        t = CreateTween();
        t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        t.TweenProperty(readBox, "scale", new Vector2(1, 1), 0.8f);
        popAudio.Stop();
        popAudio.Play();
    }

    void AskAnimEnd()
    {
        t?.Kill();
        t = CreateTween();
        t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
        t.TweenProperty(readBox, "scale", new Vector2(0, 0), 0.5f);
    }

    public void BodyEntered2D(Node2D body)
    {
        if (body is Character)
        {
            characterEntered = true;
        }
    }

    public void BodyExited2D(Node2D body)
    {
        if (body is Character)
        {
            characterEntered = false;
        }
    }

	public void SetButton(string but1, string but2)
	{
		t3?.Kill();
        t3 = CreateTween();
        t3.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        t3.TweenProperty(character.buttonMain, "position", new Vector2(0,0), 0.3f);
        canSelect = true;
        character.buttonMain.Visible = true;
        isSelecting = true;
        character.buttonTexts[0].Text = but1;
        character.buttonTexts[1].Text = but2;
        selectIndex = 0;
        t2?.Kill();
        t2 = CreateTween();
        t2.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        t2.TweenProperty(character.buttons[0], "scale", new Vector2(1.3f,1.3f), 0.4f);
        t?.Kill();
        t = CreateTween();
        t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        t.TweenProperty(character.buttons[1], "scale", new Vector2(1f,1f), 0.4f);
	}

	public void SelectFinished()
	{
		text.VisibleRatio = 0;
        text.Text = Texts[currentTextId];
        isSelecting = false;
        t3?.Kill();
        t3 = CreateTween();
        t3.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        t3.TweenProperty(character.buttonMain, "position", new Vector2(0,-64), 0.3f).Finished += () =>
        {
            character.buttonMain.Visible = false;
        };
	}

	public void GiveAnimFinished(string animName)
	{
		if (animName == "Give Element")
		{
			animtree.Set("parameters/conditions/idle", true);
			animtree.Set("parameters/conditions/give", false);
			character.dialogAnim.Play("Opening");
		}
	}

	public void SpawnHeart()
	{
		Area2D item = (Area2D)heart.Instantiate();
		item.BodyEntered += (body) => heartCollected(body);
		item.GlobalPosition = heartSpawnPos.GlobalPosition;
		GetTree().CurrentScene.AddChild(item);
		currentheart = item;
		if (!pd.talkedNpcs.Contains("DanielsFather"))
		{
			pd.talkedNpcs.Add("DanielsFather");	
		}
	}
	public void heartCollected(Node2D body)
	{
		if (body is Character)
		{
			takeCpu.Emitting = true;
			currentheart.QueueFree();
			if (!pd.takedHearts.Contains("Forest"))
			{
				pd.takedHearts.Add("Forest");		
			}
			cam.Shake(20);
		}
	}
}
