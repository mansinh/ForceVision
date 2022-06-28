@ECHO OFF
ECHO ________________________________________________________________________________
ECHO ============================= TRAINING START! ==================================
cd /D "%~dp0"
ECHO Deleting Old Cascade
ECHO del /S WALKER_Cascade\*
ECHO ________________________________________________________________________________
ECHO ============================== CREATE SAMPLES ==================================
opencv_createsamples.exe -info WALKER_Positive.txt -w 48 -h 48 -num 5000 -vec walker.vec
ECHO ________________________________________________________________________________
ECHO =============================== TRAIN CASCADE ==================================
opencv_traincascade.exe -data WALKER_Cascade/ -vec walker.vec -bg WALKER_Negative.txt -w 48 -h 48 -numPos 500 -numNeg 500 -numStages 12 -maxFalseAlarmRate 0.2 -minHitRate 0.999 -acceptanceRatioBreakValue 0.0001
ECHO ________________________________________________________________________________
ECHO ============================= TRAINING FINISHED! ===============================
ECHO  •
PAUSE