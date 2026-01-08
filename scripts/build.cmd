@echo off
cd /d "%~dp0"

echo Iniciando Build e Atualizacao do Tekla...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "build_macro.ps1" -UpdateTekla

if %ERRORLEVEL% EQU 0 (
    echo [SUCESSO] Macro compilada e enviada para o Tekla.
) else (
    echo [ERRO] Falha no processo.
)

pause