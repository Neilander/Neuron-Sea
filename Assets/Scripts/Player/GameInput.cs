using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public struct VirtualJoystick
{
    public Vector2 Value { get => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) == Vector2.zero ? new Vector2(GameInput.Right.Checked() ? 1 : GameInput.Left.Checked() ? -1 : 0, GameInput.Up.Checked() ? 1 : GameInput.Down.Checked() ? -1 : 0) : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); }
}

public class VisualButton
{
    public KeyCode key;
    private float preInputTime;//预输入时间
    private bool fixedGetKeyDown;
    private bool fixedGetKey;
    private bool getKeyInFrame;
    private float preInputTimer;
    private float minEffectiveTime;//生效后最短持续时间
    private float minEffectiveTimer;

    public VisualButton(KeyCode key, float preInputTime = 0f, float minEffectiveTime = 0f)
    {
        this.key = key;
        this.preInputTime = preInputTime;
        this.preInputTimer = 0f;
        this.minEffectiveTime = minEffectiveTime;
        this.minEffectiveTimer = 0f;
        GameInput.Buttons.Add(this);
    }

    public bool Pressed(bool fixed_check = true)
    {
        if (fixed_check)
        {
            return fixedGetKeyDown || this.preInputTimer > 0f;
        }
        else
        {
            return Input.GetKeyDown(key);
        }
    }

    public bool Checked(bool fixed_check = true)
    {
        if (fixed_check)
        {
            return fixedGetKey || this.minEffectiveTimer > 0f;
        }
        else
        {
            return Input.GetKey(key);
        }
    }

    //按键生效时，设置最短持续时间，并结束预输入时间
    public void OnTrigger()
    {
        minEffectiveTimer = minEffectiveTime;
        preInputTimer = 0f;
    }

    public void Update(float deltaTime)
    {
        if (Input.GetKey(key))
        {
            getKeyInFrame = true;
        }
    }

    public void FixedUpdate(float deltaTime)
    {
        if (fixedGetKeyDown)
        {
            fixedGetKeyDown = false;
        }
        if (getKeyInFrame)
        {
            if (!fixedGetKey)
            {
                fixedGetKeyDown = true;
            }
            fixedGetKey = true;
        }
        else
        {
            fixedGetKey = false;
        }
        getKeyInFrame = false;
        if (preInputTime > 0)
        {
            this.preInputTimer -= deltaTime;
            if (fixedGetKeyDown)
            {
                this.preInputTimer = this.preInputTime;
            }
            else if (!fixedGetKey)
            {
                this.preInputTimer = 0f;
            }
        }
        if (minEffectiveTimer > 0)
        {
            minEffectiveTimer -= deltaTime;
        }
    }
}

public class BaseGameInput
{
    public static List<VisualButton> Buttons = new List<VisualButton>();

    public static void Update(float deltaTime)
    {
        foreach (VisualButton button in Buttons)
        {
            button.Update(deltaTime);
        }
    }

    public static void FixedUpdate(float deltaTime) 
    {
        foreach (VisualButton button in Buttons)
        {
            button.FixedUpdate(deltaTime);
        }
    }
}

public class GameInput : BaseGameInput
{
    public static VisualButton Jump = new VisualButton(KeyCode.Space, Constants.JumpPreInputTime, Constants.JumpMinEffectiveTime);
    public static VisualButton Up = new VisualButton(KeyCode.W);
    public static VisualButton Down = new VisualButton(KeyCode.S);
    public static VisualButton Left = new VisualButton(KeyCode.A);
    public static VisualButton Right = new VisualButton(KeyCode.D);
    public static VisualButton Confirm = new VisualButton(KeyCode.Space);
    public static VisualButton Menu = new VisualButton(KeyCode.Escape);
    public static VirtualJoystick Aim = new VirtualJoystick();
}
