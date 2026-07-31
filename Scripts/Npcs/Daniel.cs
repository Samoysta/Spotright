using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public partial class Daniel : Area2D
{
    [Export] Character character;
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
	[Export] Node2D eyes;
    bool isSelecting;
    int selectIndex;
    PlayerData pd;
    Tween t2;
    [Export] Node2D characterTargetPos;
    bool SetUpAnim;
    bool canSelect;
    Tween t3;
    [Export] AnimationPlayer giveGunAnim;
    [Export] CpuParticles2D giveEf;
    float timer1;
    bool givedGun = false;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        pd = GetNode<PlayerData>("/root/PlayerData");
        character.dialogAnim.AnimationFinished += (animName) => {AnimFinished(animName);};
        giveGunAnim.AnimationFinished += (animName) =>
        {
            if (animName == "GiveGun")
            {
                giveEf.Emitting = true;
                timer1 = 1.5f;
                character.TakedItemEf("Blue Danigun");
                givedGun = true;
            }
        };
        text = character.dialogText;
        text.VisibleRatio = 0;
        for (int i = 0; i < Texts.Length; i++)
        {
            Texts[i] = Texts[i].Replace("\\n","\n");
			Texts[i] = Texts[i].Replace("//","/");
        }
        if (pd.talkedNpcs.Contains("Daniel"))
        {
            if (pd.openedAbilityIds.Contains(1))
            {
                currentTextId = 8;
            }
            else
            {
                currentTextId = 4;
            }
            text.Text = Texts[currentTextId];
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if (timer1 > 0)
        {
            timer1 -= (float)delta;
        }
        else
        {
            if (givedGun)
            {
                givedGun = false;
                character.cantInput = false;   
                AskAnimStart();
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
		if (character.GlobalPosition.DistanceTo(GlobalPosition) < 200)
		{
			eyes.Position = eyes.Position.Lerp(new Vector2(
			(character.GlobalPosition.X - GlobalPosition.X) / 30,0),5 * (float)delta);
			eyes.Position = new Vector2(Mathf.Clamp(eyes.Position.X,-4,4),0);
		}
		else
		{
			eyes.Position = eyes.Position.Lerp(Vector2.Zero, 5 * (float)delta);
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
            if (text.VisibleRatio == 1 && !isSelecting)
            {
                if (currentTextId == 4)
                {
                    t3?.Kill();
                    t3 = CreateTween();
                    t3.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
                    t3.TweenProperty(character.buttonMain, "position", new Vector2(0,0), 0.3f);
                    canSelect = true;
                    character.buttonMain.Visible = true;
                    isSelecting = true;
                    character.buttonTexts[0].Text = "Yes, please";
                    character.buttonTexts[1].Text = "No, thanks";
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
                            if (new[] {6,7,8,9}.Contains(currentTextId))
                            {
                                character.dialogAnim.Play("Closing");
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
                    textBoxAudio.Play();
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
            if (currentTextId != 0)
            {
                if (currentTextId == 6)
                {
                    currentTextId = 4;   
                }
                else if (currentTextId == 7)
                {
                    currentTextId = 8;
                }
                else if (currentTextId == 8)
                {
                    currentTextId = 9;
                }
                else if (currentTextId == 9)
                {
                    currentTextId = 8;
                }
            }
            else
            {
                currentTextId = 0;
            }
            text.Text = Texts[currentTextId];
        }
        else if(animName == "Closing")
        {
            textingStarted = false;
            character.cantInput = false;
            text.Text = "";
            if (!pd.talkedNpcs.Contains("Daniel"))
            {
                pd.talkedNpcs.Add("Daniel");                
            }
            if (currentTextId == 7)
            {
                character.cantInput = true;
                giveGunAnim.Play("GiveGun");
            }
            else
            {
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
}
