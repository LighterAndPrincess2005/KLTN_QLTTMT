Option Explicit
Dim taskShell, taskFS, taskRoot, taskCommand
Set taskShell = CreateObject("WScript.Shell")
Set taskFS = CreateObject("Scripting.FileSystemObject")
taskRoot = taskFS.GetParentFolderName(WScript.ScriptFullName)
taskShell.CurrentDirectory = taskRoot
taskCommand = "powershell.exe -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File " & Chr(34) & taskRoot & "\Bat_API.ps1" & Chr(34)
taskShell.Run taskCommand, 0, False