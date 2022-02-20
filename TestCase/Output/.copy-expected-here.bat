@echo off
for /f "usebackq delims=|" %%f in (`dir /b "*.bmp"`) do if exist "..\Expected\%%f" copy "..\Expected\%%f" "%%f-expected.bmp"
for /f "usebackq delims=|" %%f in (`dir /b "*.jpg"`) do if exist "..\Expected\%%f" copy "..\Expected\%%f" "%%f-expected.jpg"
for /f "usebackq delims=|" %%f in (`dir /b "*.png"`) do if exist "..\Expected\%%f" copy "..\Expected\%%f" "%%f-expected.png"
