@ECHO OFF
ECHO TrainingStart!
ECHO
ECHO DeleteOldCascade
ECHO
del /S C:\Users\manhk\ForceVision\Assets\Database\opencv\build\x64\vc15\bin\Cascade\*
cd C:\Users\manhk\ForceVision\Assets\Database\opencv\build\x64\vc15\bin
ECHO
ECHO CreateSamples
ECHO
opencv_createsamples.exe -info TROOPER_Positive.txt -w 30 -h 50 -num 1000 -vec pos.vec
ECHO
ECHO TrainCascade
ECHO
opencv_traincascade.exe -data Cascade/ -vec pos.vec -bg TROOPER_Negative.txt -w 30 -h 50 -numPos 1000 -numNeg 1000 -numStages 12 -maxFalseAlarmRate 0.3 -minHitRate 0.999 -acceptanceRatioBreakValue 0.0001
ECHO TrainingFinished!
ECHO  •
PAUSE