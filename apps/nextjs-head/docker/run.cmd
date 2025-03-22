@echo off
echo.
echo ### Restore packages
call npm ci
echo.
echo ### Building app (and SSG)
call npm run build
echo.
echo ### Starting app in prod mode
call npm run prod
