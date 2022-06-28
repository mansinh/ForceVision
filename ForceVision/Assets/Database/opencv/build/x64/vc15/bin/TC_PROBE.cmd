@ECHO OFF
ECHO ________________________________________________________________________________
ECHO ============================= TRAINING START! ==================================
cd /D "%~dp0"
ECHO Deleting Old Cascade
del /S TROOPER_Cascade\*
ECHO ________________________________________________________________________________
ECHO ============================== CREATE SAMPLES ==================================
opencv_createsamples.exe -info TROOPER_Positive.txt -w 24 -h 24 -num 5000 -vec pos.vec
ECHO ________________________________________________________________________________
ECHO =============================== TRAIN CASCADE ==================================
opencv_traincascade.exe -data TROOPER_Cascade/ -vec pos.vec -bg TROOPER_Negative.txt -w 24 -h 24 -numPos 500 -numNeg 500 -numStages 12 -maxFalseAlarmRate 0.2 -minHitRate 0.999 -acceptanceRatioBreakValue 0.0001
ECHO ________________________________________________________________________________
ECHO ============================= TRAINING FINISHED! ===============================
ECHO  •
PAUSE