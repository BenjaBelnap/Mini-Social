@echo off
REM Setup script for MongoDB and secrets configuration (Windows)

echo 🚀 Setting up Mini-Social Database Environment...

REM Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker is not running. Please start Docker and try again.
    exit /b 1
)

REM Navigate to database directory
cd /d "%~dp0database"

REM Copy environment template if .env doesn't exist
if not exist .env (
    echo 📝 Creating .env file from template...
    copy .env.example .env
    echo ⚠️  Please edit database\.env and set secure passwords before continuing!
    echo    Required variables:
    echo    - MONGO_ROOT_PASSWORD
    echo    - MONGO_APP_PASSWORD
    echo.
    pause
)

echo 🐳 Starting MongoDB container...
docker-compose up -d

echo ⏳ Waiting for MongoDB to be ready...
timeout /t 10 /nobreak >nul

:wait_for_mongo
docker exec minisocial-mongodb mongosh --eval "print('MongoDB is ready')" >nul 2>&1
if errorlevel 1 (
    echo    Still waiting for MongoDB...
    timeout /t 5 /nobreak >nul
    goto wait_for_mongo
)

echo ✅ MongoDB is ready!

REM Navigate back to API directory
cd /d "%~dp0src\MiniSocial.Api"

echo 🔐 Setting up User Secrets...
dotnet user-secrets init >nul 2>&1
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb://minisocial_user:minisocial_password@localhost:27017/minisocial"

echo 🧪 Running integration tests...
cd /d "%~dp0"
dotnet test --filter Category=Integration

echo.
echo 🎉 Setup complete!
echo.
echo 📊 Database Access:
echo    MongoDB:        mongodb://localhost:27017
echo    Web Interface:  http://localhost:8081
echo    Admin User:     admin
echo.
echo 🔧 Next Steps:
echo    1. Run: dotnet run --project src/MiniSocial.Api
echo    2. Visit: https://localhost:7000/swagger (or check launchSettings.json)
echo    3. Test API endpoints

pause
