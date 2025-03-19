#!/bin/bash
# Request cursor position
echo -en "\033[6n"

# Read response from terminal
IFS='[;' read -sdR -p "" _ row col

# Output row and column as CSV
echo "$row,$col"
