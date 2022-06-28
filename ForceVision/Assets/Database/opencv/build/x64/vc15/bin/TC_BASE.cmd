@ECHO OFF
ECHO ________________________________________________________________________________
ECHO ======================== BASE TRAINING START! ===============================
cd /D "%~dp0"
ECHO ________________________________________________________________________________
ECHO ============================== CREATE SAMPLES ==================================
opencv_createsamples.exe -info BASE_Positive.txt -w 24 -h 24 -num 5000 -vec base.vec
ECHO ________________________________________________________________________________
ECHO =============================== TRAIN CASCADE ==================================
opencv_traincascade.exe -data BASE_Cascade/ -vec base.vec -bg BASE_Negative.txt -w 24 -h 24 -numPos 500 -numNeg 700 -numStages 12 -maxFalseAlarmRate 0.2 -minHitRate 0.999 -acceptanceRatioBreakValue 0.0001
ECHO ________________________________________________________________________________
ECHO ============================= TRAINING FINISHED! ===============================
ECHO  •
PAUSE