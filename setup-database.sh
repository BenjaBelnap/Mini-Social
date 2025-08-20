#!/bin/bash
# Setup script for MongoDB and secrets configuration
# Works on Windows (Git Bash), macOS, and Linux

set -e

echo "🚀 Setting up Mini-Social Database Environment..."

# Check if Docker is running
if ! docker info >/dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Navigate to database directory
cd "$(dirname "$0")/database"

# Copy environment template if .env doesn't exist
if [ ! -f .env ]; then
    echo "📝 Creating .env file from template..."
    cp .env.example .env
    echo "⚠️  Please edit database/.env and set secure passwords before continuing!"
    echo "   Required variables:"
    echo "   - MONGO_ROOT_PASSWORD"
    echo "   - MONGO_APP_PASSWORD"
    echo ""
    read -p "Press Enter after updating the .env file..."
fi

# Source the .env file
set -a
source .env
set +a

echo "🐳 Starting MongoDB container..."
docker-compose up -d

echo "⏳ Waiting for MongoDB to be ready..."
sleep 10

# Check if MongoDB is accessible
until docker exec minisocial-mongodb mongosh --eval "print('MongoDB is ready')" >/dev/null 2>&1; do
    echo "   Still waiting for MongoDB..."
    sleep 5
done

echo "✅ MongoDB is ready!"

# Navigate back to API directory
cd "../src/MiniSocial.Api"

echo "🔐 Setting up User Secrets..."
dotnet user-secrets init 2>/dev/null || true
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb://${MONGO_APP_USERNAME}:${MONGO_APP_PASSWORD}@localhost:27017/${MONGO_DATABASE}"

echo "🧪 Running integration tests..."
cd "../../"
dotnet test --filter Category=Integration

echo ""
echo "🎉 Setup complete!"
echo ""
echo "📊 Database Access:"
echo "   MongoDB:        mongodb://localhost:27017"
echo "   Web Interface:  http://localhost:8081"
echo "   Admin User:     ${MONGO_ROOT_USERNAME}"
echo ""
echo "🔧 Next Steps:"
echo "   1. Run: dotnet run --project src/MiniSocial.Api"
echo "   2. Visit: https://localhost:7000/swagger (or check launchSettings.json)"
echo "   3. Test API endpoints"
