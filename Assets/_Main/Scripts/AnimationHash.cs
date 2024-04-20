using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHash
{
    public static int Run = Animator.StringToHash("Run");
    public static int Idle = Animator.StringToHash("Idle");
    public static int Jump = Animator.StringToHash("Jump");
    public static int Grounded = Animator.StringToHash("Grounded");

    public static int AirTime = Animator.StringToHash("AirTime");
    public static int RandomFallingAnim = Animator.StringToHash("RandomFallingAnim");
    
    public static int XAxis = Animator.StringToHash("XAxis");
}
