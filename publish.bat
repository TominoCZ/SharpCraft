@echo off
rmdir ".\build" /s/q>nul

dotnet publish .\SharpCraft.sln --runtime win10-x64 -c release -o "..\build\SharpCraft_A0.0.3_win10-x64"

md ".\build\SharpCraft_win10-x64\SharpCraft_Data">nul
xcopy ".\SharpCraft\SharpCraft_Data" ".\build\SharpCraft_win10-x64\SharpCraft_Data" /s/h/e/k/f/c>nul
