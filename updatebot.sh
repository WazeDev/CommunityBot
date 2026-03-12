#!/bin/bash
echo "--- Starting Full Bot Update/Rebuild ---"

echo "1. Pulling latest code and scripts from GitHub..."
git pull

echo "2. Stopping and removing existing container..."
docker stop wazebotdiscord-communitybot-1 || true
docker rm wazebotdiscord-communitybot-1 || true

echo "3. Rebuilding Docker image with fresh local code..."
docker build -t communitybot:latest .

echo "4. Launching new container..."
docker run -d --name wazebotdiscord-communitybot-1 --env-file .env --restart always communitybot:latest

echo "--- Update Complete ---"
echo "Watching live logs (Press Ctrl+C to exit):"
docker logs -f wazebotdiscord-communitybot-1
