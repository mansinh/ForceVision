@ECHO OFF
ECHO ________________________________________________________________________________
ECHO ======================== ANNOTATION START! ===============================
cd /D "%~dp0"

opencv_annotation --images=PHOTO_Positive/ --annotations=PHOTO_Positive.txt

ECHO ________________________________________________________________________________
ECHO ============================= ANNOTATION FINISHED! ===============================
ECHO  •
PAUSE