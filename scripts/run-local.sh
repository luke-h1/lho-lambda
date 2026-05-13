#!/bin/bash

set -eu

GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}Building Lambda function...${NC}"
dotnet build src/Lho.Lambda.Local/Lho.Lambda.Local.csproj

echo -e "${GREEN}Starting local Lambda server...${NC}"
echo -e "${BLUE}Server will be available at http://localhost:7000/invoke${NC}"
echo -e "${BLUE}Press Ctrl+C to stop${NC}"
echo ""

dotnet run --project src/Lho.Lambda.Local/Lho.Lambda.Local.csproj
