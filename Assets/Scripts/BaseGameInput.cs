using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class VisualButton
{
    public KeyCode key;
    private float buffer_time;//预输入时间
    private bool fixed_get_key_down;
    private bool fixed_get_key;
    private bool get_key_in_frame;
    private float buffer_timer;
    private float min_time;//生效后最短持续时间
    private float min_timer;

    public VisualButton(KeyCode key, float buffer_time = 0f, float min_time = 0f)
    {
        this.key = key;
        this.buffer_time = buffer_time;
        this.buffer_timer = 0f;
        this.min_time = min_time;
        this.min_timer = 0f;
        JumpInput.Buttons.Add(this);
    }

    public bool Pressed(bool fixed_check = true)
    {
        // 如果游戏暂停（timeScale为0），不处理任何输入
        if (Time.timeScale == 0f)
        {
            return false;
        }

        if (fixed_check)
        {
            return fixed_get_key_down || this.buffer_timer > 0f;
        }
        else
        {
            return Input.GetKeyDown(key);
        }
    }

    public bool Checked(bool fixed_check = true)
    {
        // 如果游戏暂停（timeScale为0），不处理任何输入
        if (Time.timeScale == 0f)
        {
            return false;
        }

        if (fixed_check)
        {
            return fixed_get_key || this.min_timer > 0f;
        }
        else
        {
            return Input.GetKey(key);
        }
    }

    //按键生效时，设置最短持续时间
    public void OnTrigger()
    {
        min_timer = min_time;
    }

    public void Update(float deltaTime)
    {
        if (Input.GetKey(key))
        {
            get_key_in_frame = true;
        }
    }

    public void FixedUpdate(float deltaTime)
    {
        if (fixed_get_key_down)
        {
            fixed_get_key_down = false;
        }
        if (get_key_in_frame)
        {
            if (!fixed_get_key)
            {
                fixed_get_key_down = true;
            }
            fixed_get_key = true;
        }
        else
        {
            fixed_get_key = false;
        }
        get_key_in_frame = false;
        if (buffer_time > 0)
        {
            this.buffer_timer -= deltaTime;
            if (fixed_get_key_down)
            {
                this.buffer_timer = this.buffer_time;
            }
            else if (!fixed_get_key)
            {
                this.buffer_timer = 0f;
            }
        }
        if (min_timer > 0)
        {
            min_timer -= deltaTime;
        }
    }
}

public class BaseGameInput : MonoBehaviour
{
    public static List<VisualButton> Buttons = new List<VisualButton>();

    public void Update()
    {
        foreach (VisualButton button in Buttons)
        {
            button.Update(Time.unscaledDeltaTime);
        }
    }

    public void FixedUpdate()
    {
        foreach (VisualButton button in Buttons)
        {
            button.FixedUpdate(Time.fixedDeltaTime);
        }
    }
}

public class JumpInput : BaseGameInput
{
    public static VisualButton Jump = new VisualButton(KeyCode.Space, 0.08f, 0.08f);

    private void Awake()
    {
        Buttons.Add(Jump);
    }
}
