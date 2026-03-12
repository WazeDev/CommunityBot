#!/bin/bash

# ==============================================================================
# Script: stopbot.sh
# Purpose: Safely shuts down the active Community Bot container.
# Note: The bot will remain offline until startbot.sh is run. No logs are tracked.
# ==============================================================================

echo "Stopping Community Bot..."
docker stop wazebotdiscord-communitybot-1

echo "Bot is securely offline."
