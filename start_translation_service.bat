@echo off
echo ============================================
echo     KHOI DONG TRANSLATION SERVICE
echo ============================================
echo.

REM Kiểm tra xem Python có được cài đặt không
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERRO: Python chua duoc cai dat!
    echo Vui long cai dat Python tu https://python.org
    pause
    exit /b 1
)

echo Python da duoc cai dat.
echo.

REM Kiểm tra xem có virtual environment không
if not exist "translation_env" (
    echo Tao virtual environment...
    python -m venv translation_env
    if %errorlevel% neq 0 (
        echo ERRO: Khong the tao virtual environment!
        pause
        exit /b 1
    )
)

REM Kích hoạt virtual environment
echo Kich hoat virtual environment...
call translation_env\Scripts\activate.bat

REM Cài đặt dependencies
echo Cai dat cac package can thiet...
pip install -r requirements.txt

REM Kiểm tra xem model có tồn tại không
if not exist "C:\Users\Admin\OneDrive\Desktop\model\translated_model" (
    echo.
    echo CANH BAO: Model khong tim thay tai:
    echo C:\Users\Admin\OneDrive\Desktop\model\translated_model
    echo.
    echo Vui long kiem tra lai duong dan model!
    pause
    exit /b 1
)

echo.
echo ============================================
echo Model da san sang. Bat dau translation service...
echo Service se chay tren: http://localhost:5000
echo Nhan Ctrl+C de dung service.
echo ============================================
echo.

REM Chạy translation service
python translation_service.py

pause 