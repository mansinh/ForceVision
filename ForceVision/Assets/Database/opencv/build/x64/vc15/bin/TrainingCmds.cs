using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingCmds : MonoBehaviour
{
    // cd C:\Users\manhk\ForceVision\Assets\Database\opencv\build\x64\vc15\bin

    // opencv_createsamples.exe -info TROOPER_Positive.txt -w 30 -h 50 -num 1000 -vec pos.vec
    // opencv_traincascade.exe -data Cascade/ -vec pos.vec -bg TROOPER_Negative.txt -w 30 -h 50 -numPos 200 -numNeg 400 -numStages 10
}
//opencv_traincascade.exe -data Cascade/ -vec pos.vec -bg TROOPER_Negative.txt -w 30 -h 50 -numPos 500 -numNeg 400 -numStages 12 -maxFalseAlarmRate 0.3