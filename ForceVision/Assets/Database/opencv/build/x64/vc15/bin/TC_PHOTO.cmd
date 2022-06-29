@ECHO OFF
ECHO ________________________________________________________________________________
ECHO ======================== BASE TRAINING START! ===============================
cd /D "%~dp0"
ECHO ________________________________________________________________________________
ECHO ============================== CREATE SAMPLES ==================================
opencv_createsamples.exe -info PHOTO_Positive.txt -w 24 -h 24 -num 1000 -vec photo.vec
ECHO ________________________________________________________________________________
ECHO =============================== TRAIN CASCADE ==================================
opencv_traincascade.exe -data PHOTO_Cascade/ -vec photo.vec -bg PHOTO_Negative.txt -w 24 -h 24 -numPos 250 -numNeg 250 -numStages 12 -maxFalseAlarmRate 0.2 -minHitRate 0.999 -acceptanceRatioBreakValue 0.0001
ECHO ________________________________________________________________________________
ECHO ============================= TRAINING FINISHED! ===============================
ECHO  •
PAUSE