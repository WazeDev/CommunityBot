#!/bin/bash

# ==============================================================================
# Script: startbot.sh
# Purpose: Wakes up the existing Community Bot container.
# Note: This does NOT pull new code or rebuild the bot. Use updatebot.sh for that.
# ==============================================================================

echo "Starting Community Bot..."
docker start wazebotdiscord-communitybot-1

# Provide a clean break before the log output begins
echo "--- Bot started. Tailing live logs (Press Ctrl+C to exit) ---"
docker logs -f wazebotdiscord-communitybot-1
