@echo off
setlocal

set PROJECT_DIR=C:\Users\HN409P25\source\repos\Almacen STLCC
set PUBLISH_DIR=%PROJECT_DIR%\publish
set REMOTE_USER=soporte
set REMOTE_HOST=172.16.3.30
set REMOTE_DIR=/var/www/almacen-stlcc
set SERVICE_NAME=almacen-stlcc

echo =====================================
echo   DEPLOY ALMACEN STLCC
echo =====================================
echo.

REM Cambiar al directorio del proyecto
cd /d "%PROJECT_DIR%" || (
    echo ERROR: No se pudo cambiar al directorio del proyecto.
    pause
    exit /b 1
)

REM Limpiar publicación anterior
if exist "%PUBLISH_DIR%" (
    echo Limpiando publicacion anterior...
    rmdir /s /q "%PUBLISH_DIR%"
)

REM Publicar la aplicación
echo [1/3] Publicando la aplicacion...
dotnet publish "Almacen STLCC.sln" -c Debug -o "%PUBLISH_DIR%"
if %ERRORLEVEL% neq 0 (
    echo ERROR: Fallo en dotnet publish
    pause
    exit /b 1
)
echo      OK - Aplicacion publicada

REM Subir archivos con scp
echo [2/3] Subiendo archivos al servidor remoto...
REM Usar comillas y barra normal para Windows
scp -r "%PUBLISH_DIR%/*" %REMOTE_USER%@%REMOTE_HOST%:%REMOTE_DIR%/
if %ERRORLEVEL% neq 0 (
    echo ERROR: Fallo en SCP
    pause
    exit /b 1
)
echo      OK - Archivos subidos

REM Reiniciar el servicio remoto
echo [3/3] Reiniciando servicio remoto...
ssh -t %REMOTE_USER%@%REMOTE_HOST% "sudo systemctl restart %SERVICE_NAME% && echo 'Servicio reiniciado' && sudo systemctl status %SERVICE_NAME% --no-pager -l"
if %ERRORLEVEL% neq 0 (
    echo ADVERTENCIA: Revisa el estado del servicio manualmente
)

echo.
echo =====================================
echo   DEPLOY COMPLETADO CON EXITO
echo =====================================
echo.
echo Accede a: http://172.16.3.30:8380
echo Ver logs:  ssh %REMOTE_USER%@%REMOTE_HOST% "sudo journalctl -u %SERVICE_NAME% -f"
echo.
pause
endlocal