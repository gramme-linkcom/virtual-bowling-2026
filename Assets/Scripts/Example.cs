using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Example : MonoBehaviour
{
    private static readonly Joycon.Button[] m_buttons =
        Enum.GetValues(typeof(Joycon.Button)) as Joycon.Button[];

    private List<Joycon> m_joycons;
    private Joycon m_joyconL;
    private Joycon m_joyconR;
    private Joycon.Button? m_pressedButtonL;
    private Joycon.Button? m_pressedButtonR;
    private bool joyconEnabled = true;
    Transform tf;
    public class AxisAngle
    {
        public float angle{get; private set;}
        public Vector3 axis{get; private set;}
        public float deltaAngle{get; private set;}
        public float velocity{get; private set;}
        public float angularAcceleration{get; private set;}
        public AxisAngle(float angle,Vector3 axis,float deltaAngle,float velocity,float angularAcceleration)
        {
            this.angle = angle;
            this.axis = axis;
            this.deltaAngle = deltaAngle;
            this.velocity = velocity;
            this.angularAcceleration = angularAcceleration;
        }
    };

    private void Start()
    {
        Application.targetFrameRate = 30;
        m_joycons = JoyconManager.Instance.j;

        if (m_joycons == null || m_joycons.Count <= 0)
        {
            Debug.Log("Joyconがないよ☆");
            joyconEnabled = false;
            return;
        }

        m_joyconL = m_joycons.Find(c => c.isLeft);
        m_joyconR = m_joycons.Find(c => !c.isLeft);
        tf = transform;
    }

    private void Update()
    {
        m_pressedButtonL = null;
        m_pressedButtonR = null;

        if (m_joycons == null || m_joycons.Count <= 0) return;

        // foreach (var button in m_buttons)
        // {
        //     if (m_joyconL.GetButton(button))
        //     {
        //         m_pressedButtonL = button;
        //     }
        //     if (m_joyconR.GetButton(button))
        //     {
        //         m_pressedButtonR = button;
        //     }
        // }

    }
    private void FixedUpdate(){
        if (m_joycons == null || m_joycons.Count <= 0) return;
        tf.rotation = m_joyconR.GetVector();
    }

    [NonSerialized] public bool ballThrows = false;
    [NonSerialized] public float Power = 0;
    [NonSerialized] public float PowerZ = 0;
    float StartGyro = 0;
    float StartOrientZ = 0;
    float EndOrient = 0;
    float EndOrientZ = 0;
    [NonSerialized] public bool[] ones = { false, false };
    [SerializeField] private const int length = 1;
    float velocity = 0.0f;
    float endAngle = 0.0f;
    float totalTime = 0.0f;
    float angularAcceleration = 0.0f;
    AxisAngle AA = new AxisAngle(0f,Vector3.zero,0f,0f,0f);
    [SerializeField ,Range(0,1)]
    float abjust = 0.5f;
    public (float, Vector3) JoyConThrow()
    {
        if (!joyconEnabled) return (0f, Vector3.zero);
        
        if (m_joyconR.GetButton(Joycon.Button.SHOULDER_2))
        {
            totalTime += Time.deltaTime;
            AA = GetAxisAngle();
            Debug.Log("deltaangle:"+AA.deltaAngle);
            float deltaAngle = AA.deltaAngle;
            if(deltaAngle < -1 || 1 < deltaAngle && deltaAngle < 15){
                endAngle = AA.angle;
                velocity += AA.velocity;
                Debug.Log(velocity);
                angularAcceleration += AA.angularAcceleration;
            }
        }
        if (m_joyconR.GetButtonUp(Joycon.Button.SHOULDER_2))
        {
            if(!ballThrows){
                if(endAngle <= 0 && velocity < 0)
                {
                    Power = velocity / totalTime * abjust;
                    ballThrows = true;
                    Debug.Log(velocity/totalTime * abjust);    
                }
                else
                {
                    Debug.Log("後ろに投げたよ☆"+velocity+" "+endAngle);
                }
                m_joyconR.Recenter();
                firstTime = true;
                velocity = 0;
                float endAngularAcceleration = angularAcceleration / totalTime;
                totalTime = 0;
                Debug.Log(endAngularAcceleration+" "+ AA.axis);
                angularAcceleration = 0f;
                return (endAngularAcceleration, AA.axis);
            }
            //・ｽ・ｽ・ｽ・ｽ・ｽﾜゑｿｽ
        }
        return (0f, Vector3.zero);
    }
    bool firstTime = true;
    float firstAngle = 0.0f;
    AxisAngle GetAxisAngle(){
        AxisAngle AA;
        float angle = 0.0f;
        Vector3 axis = Vector3.zero;
        
        Quaternion q = transform.rotation;
        q.ToAngleAxis(out angle, out axis); //・ｽp・ｽx・ｽﾆ趣ｿｽ・ｽ謫ｾ
        angle = angle * axis.x; //・ｽ・ｽ・ｽﾉゑｿｽ・ｽ・ｽﾄ前・ｽ續ｻ・ｽ・ｽ
        //angle * axis.y ・ｽ・ｽ・ｽ・ｽ・ｽ・ｽ・ｽ・ｽ
        //angle * axis.z ・ｽ・ｽ・ｽ・ｽ・ｽX・ｽ・ｽ
        //Debug.Log(angle);
        if(firstTime)
        {
            float firstAngle = angle;
            firstTime = false;
            return new AxisAngle(angle,axis,0,0,0);
        }
        float deltaAngle = angle - firstAngle;
        float time = Time.deltaTime;
        float angularVelocity = deltaAngle / time;
        float angularAcceleration = angularVelocity / time;
        float velocity = angularVelocity * length;
        firstAngle = angle;
        AA = new AxisAngle(angle, axis, deltaAngle, velocity,angularAcceleration);
        return AA;
        /*
        ・ｽp・ｽ・ｽ・ｽx ・ｽ・ｽ = angle/t
        ・ｽ・ｽ・ｽﾌの托ｿｽ・ｽx V = ・ｽ・ｽ * r 
        r・ｽﾍ腕・ｽﾌ抵ｿｽ・ｽ・ｽ
        ・ｽO・ｽ・ｽ・ｽ・ｽ・ｽﾌ托ｿｽ・ｽx x = cos (angle) * V
        ・ｽ・ｽ・ｽ・ｽ・ｽ・ｽﾌ托ｿｽ・ｽx y = sin (angle) * V
        */
    }

    public float Rstick()
    {
        if (!joyconEnabled) return 0;
        var stick = m_joyconR.GetStick();
        return stick[0];
    }

    public void JoyConThrowRest()
    {
        ballThrows = false;
        ones[0] = false;
        ones[1] = false;
    }


    public bool RstickZR()
    {
        if (!joyconEnabled) return false;
        if (m_joyconR.GetButtonDown(Joycon.Button.SHOULDER_2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool AButtonR()
    {
        if (!joyconEnabled) return false;
        if (m_joyconR.GetButtonDown(Joycon.Button.DPAD_RIGHT))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnGUI()
    {
        /*var style = GUI.skin.GetStyle("label");
        style.fontSize = 24;

        if (m_joycons == null || m_joycons.Count <= 0)
        {
            GUILayout.Label("Joy-Con ・ｽ・ｽ・ｽﾚ托ｿｽ・ｽ・ｽ・ｽ・ｽﾄゑｿｽ・ｽﾜゑｿｽ・ｽ・ｽ");
            return;
        }

        if (!m_joycons.Any(c => c.isLeft))
        {
            GUILayout.Label("Joy-Con (L) ・ｽ・ｽ・ｽﾚ托ｿｽ・ｽ・ｽ・ｽ・ｽﾄゑｿｽ・ｽﾜゑｿｽ・ｽ・ｽ");
            return;
        }

        if (!m_joycons.Any(c => !c.isLeft))
        {
            GUILayout.Label("Joy-Con (R) ・ｽ・ｽ・ｽﾚ托ｿｽ・ｽ・ｽ・ｽ・ｽﾄゑｿｽ・ｽﾜゑｿｽ・ｽ・ｽ");
            return;
        }

        GUILayout.BeginHorizontal(GUILayout.Width(960));

        foreach (var joycon in m_joycons)
        {
            var isLeft = joycon.isLeft;
            var name = isLeft ? "Joy-Con (L)" : "Joy-Con (R)";
            var key = isLeft ? "Z ・ｽL・ｽ[" : "X ・ｽL・ｽ[";
            var button = isLeft ? m_pressedButtonL : m_pressedButtonR;
            var stick = joycon.GetStick();
            var gyro = joycon.GetGyro();
            var accel = joycon.GetAccel();
            var orientation = joycon.GetVector();

            GUILayout.BeginVertical(GUILayout.Width(480));
            GUILayout.Label(name);
            GUILayout.Label(key + "・ｽF・ｽU・ｽ・ｽ");
            GUILayout.Label("・ｽ・ｽ・ｽ・ｽ・ｽ・ｽﾄゑｿｽ・ｽ・ｽ{・ｽ^・ｽ・ｽ・ｽF" + button);
            GUILayout.Label(string.Format("・ｽX・ｽe・ｽB・ｽb・ｽN・ｽF({0}, {1})", stick[0], stick[1]));
            GUILayout.Label("・ｽW・ｽ・ｽ・ｽC・ｽ・ｽ・ｽF" + gyro);
            GUILayout.Label("・ｽ・ｽ・ｽ・ｽ・ｽx・ｽF" + accel);
            GUILayout.Label("・ｽX・ｽ・ｽ・ｽF" + orientation);
            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();*/
    }
}
