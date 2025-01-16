@echo off
chcp 65001 >nul

setlocal
:: 註冊表路徑 及 數值名稱
set "regPath=HKEY_CURRENT_USER\SOFTWARE\Microsoft\IME\15.0\IMETC"
set "valueName=Enable Simplified Chinese Output"

:: 查詢註冊表(確保有這個值的存在)
for /f "tokens=5*" %%a in ('reg query "%regPath%" /v "%valueName%" 2^>nul') do (
    set "type=%%a"
    set "currentValue=%%b"
)

:: 顯示查詢結果
if defined currentValue (
    echo 查詢結果
    echo ===================
    echo 數值名稱: %valueName%
    echo 數值類型: %type%
    echo 數值資料: %currentValue%
    echo ===================
) else (
    echo 查詢結果
    echo ===================
    echo 查無 %valueName%。
    echo ===================

)

:: 檢查當前數值資料並該改
if "%currentValue%"=="0x00000001" (
    echo 切換成繁體中文
    reg add "%regPath%" /v "%valueName%" /t REG_SZ /d "0x00000000" /f
) else if "%currentValue%"=="0x00000000" (
    echo 切換成简体中文
    reg add "%regPath%" /v "%valueName%" /t REG_SZ /d "0x00000001" /f
) else (
    echo 數值資料有問題
)
pause

exit