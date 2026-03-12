#!/bin/bash

# ==============================================================================
# Script: restartbot.sh
# Purpose: Stops and immediately restarts the current Community Bot container.
# Note: Ideal for clearing out API hiccups without rebuilding the whole image.
# ==============================================================================

echo "Restarting Community Bot..."
docker restart wazebotdiscord-communitybot-1

# Provide a clean break before the log output begins
echo "--- Restart triggered. Tailing live logs (Press Ctrl+C to exit) ---"
docker logs -f wazebotdiscord-communitybot-1
