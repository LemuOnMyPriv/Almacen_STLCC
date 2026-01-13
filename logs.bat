@echo off
echo Conectando a logs del servidor...
echo Presiona Ctrl+C para salir
echo.
ssh -t soporte@172.16.3.30 "sudo journalctl -u almacen-stlcc -f --since '5 minutes ago'"