#!/bin/bash

set -eu

GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}Building Lambda function...${NC}"
swift build -c release

echo -e "${GREEN}Starting local Lambda server...${NC}"
echo -e "${BLUE}Server will be available at http://localhost:7000/invoke${NC}"
echo -e "${BLUE}Press Ctrl+C to stop${NC}"
echo ""




LOCAL_LAMBDA_SERVER_ENABLED=true .build/release/lambda

