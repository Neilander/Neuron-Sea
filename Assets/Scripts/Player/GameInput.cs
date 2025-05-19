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
    public Vector2 Value { get => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) == Vector2.zero ? new Vector2(GameInput.MoveRight.Checked() ? 1 : GameInput.MoveLeft.Checked() ? -1 : 0, GameInput.MoveUp.Checked() ? 1 : GameInput.MoveDown.Checked() ? -1 : 0) : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); }
}

public class VisualButton
{
    public KeyCode key;
    private float preInputTime;//预输入时间
    private bool fixedGetKeyDown;
    private bool fixedGetKey;
    private float getKeyTime = -1f;
    private float preInputTimer;
    private float minEffectiveTime;//生效后最短持续时间
    private float minEffectiveTimer;
    private bool blockedDuringPause;//是否在暂停时被阻止，一般UI类输入为False，游戏内输入为True
    private bool canBeBlockedByBlockingInput;//是否可被【屏蔽全局输入】影响

    public VisualButton(KeyCode key, float preInputTime = 0f, float minEffectiveTime = 0f, bool blockedDuringPause = false, bool canBeBlockedByBlockingInput = true)
    {
        this.key = key;
        this.preInputTime = preInputTime;
        this.preInputTimer = 0f;
        this.minEffectiveTime = minEffectiveTime;
        this.minEffectiveTimer = 0f;
        GameInput.Buttons.Add(this);
        this.blockedDuringPause = blockedDuringPause;
        this.canBeBlockedByBlockingInput = canBeBlockedByBlockingInput;
    }

    public bool Pressed(bool fixed_check = true)
    {
        if (blockedDuringPause && Time.timeScale == 0) return false;
        if (canBeBlockedByBlockingInput && GameInput.isBlockingInput) return false;
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
        if (blockedDuringPause && Time.timeScale == 0) return false;
        if (canBeBlockedByBlockingInput && GameInput.isBlockingInput) return false;
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
            getKeyTime = Time.unscaledTime;
        }
    }

    public void FixedUpdate(float deltaTime)
    {
        if (fixedGetKeyDown)
        {
            fixedGetKeyDown = false;
        }
        if (Time.unscaledTime - getKeyTime < deltaTime)
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

public static class GameInput
{
    public static List<VisualButton> Buttons = new List<VisualButton>();

    public static VisualButton Jump = new VisualButton(KeyCode.Space, Constants.JumpPreInputTime, Constants.JumpMinEffectiveTime, true);
    public static VisualButton MoveUp = new VisualButton(KeyCode.W, blockedDuringPause : true);
    public static VisualButton MoveDown = new VisualButton(KeyCode.S, blockedDuringPause: true);
    public static VisualButton MoveLeft = new VisualButton(KeyCode.A, blockedDuringPause: true);
    public static VisualButton MoveRight = new VisualButton(KeyCode.D, blockedDuringPause: true);
    public static VisualButton Confirm = new VisualButton(KeyCode.Space);
    public static VisualButton Menu = new VisualButton(KeyCode.Escape);
    public static VisualButton TimeStopsHere = new VisualButton(KeyCode.Mouse1);
    public static VisualButton SwitchableSelection = new VisualButton(KeyCode.Mouse0, blockedDuringPause: true);
    public static VisualButton SwitchObjects = new VisualButton(KeyCode.E, blockedDuringPause: true);
    public static VirtualJoystick Aim = new VirtualJoystick();
    public static bool isBlockingInput = false;

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
