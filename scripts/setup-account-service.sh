#!/bin/bash

# Setup script for Glense Account Service
echo "Setting up Glense Account Service..."

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo -e "${RED}Docker is not installed. Please install Docker first.${NC}"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}Docker Compose is not installed. Please install Docker Compose first.${NC}"
    exit 1
fi

# Create .env file if it doesn't exist
if [ ! -f services/Glense.AccountService/.env ]; then
    echo -e "${YELLOW}Creating .env file from .env.example...${NC}"
    cp services/Glense.AccountService/.env.example services/Glense.AccountService/.env
    echo -e "${GREEN}✓ .env file created${NC}"
else
    echo -e "${GREEN}✓ .env file already exists${NC}"
fi

# Start PostgreSQL and Account Service
echo -e "${YELLOW}Starting PostgreSQL and Account Service...${NC}"
docker-compose up -d postgres_account account_service

# Wait for services to be healthy
echo -e "${YELLOW}Waiting for services to be ready...${NC}"
sleep 10

# Check if services are running
if docker ps | grep -q glense_postgres_account && docker ps | grep -q glense_account_service; then
    echo -e "${GREEN}✓ Services are running!${NC}"
    echo ""
    echo "Account Service is available at: http://localhost:5001"
    echo "Swagger UI is available at: http://localhost:5001/swagger"
    echo "Health check: http://localhost:5001/health"
    echo ""
    echo "PostgreSQL is running on port 5432"
    echo "  Database: glense_account"
    echo "  Username: glense"
    echo "  Password: glense123"
    echo ""
    echo "To view logs: docker-compose logs -f account_service"
    echo "To stop services: docker-compose down"
else
    echo -e "${RED}✗ Services failed to start. Check logs with: docker-compose logs${NC}"
    exit 1
fi
