set /p version=<version.txt
cd ..
set target=..\..\..\JPacman-%version%.zip
set tempdir=temp_release

rmdir /S /Q %tempdir%
mkdir %tempdir%
mkdir %tempdir%\Resources
copy Build\README.txt %tempdir%
copy Bin\JPacman_Cocos2dx.exe %tempdir%\JPacman.exe
copy Bin\glew32.dll %tempdir%
copy Bin\libcocos2d.dll %tempdir%
copy Bin\libcurl.dll %tempdir%
copy Bin\libmpg123.dll %tempdir%
copy Bin\libogg.dll %tempdir%
copy Bin\libvorbis.dll %tempdir%
copy Bin\libvorbisfile.dll %tempdir%
copy Bin\OpenAL32.dll %tempdir%
copy Bin\sqlite3.dll %tempdir%
copy Bin\websockets.dll %tempdir%
copy Bin\Resources\*.map %tempdir%\Resources
copy Bin\Resources\*.png %tempdir%\Resources
copy Bin\Resources\*.mp3 %tempdir%\Resources
copy Bin\Resources\*.wav %tempdir%\Resources

cd %tempdir%
del %target%
"%ProgramFiles%\7-Zip\7z.exe" a -r -tZip -mx1 %target% *.*
cd ..
rmdir /S /Q %tempdir%
cd ..
cd ..
start .
pause