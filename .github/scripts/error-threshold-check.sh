#!/bin/bash
# Dummy example: block if more than 10 errors in logs/errors.log
if [ -f ./logs/errors.log ]; then
  ERRORS=$(grep -c "Exception" ./logs/errors.log)
  if [ "$ERRORS" -gt 10 ]; then
    echo "Error threshold exceeded: $ERRORS errors"
    exit 1
  fi
fi
echo "Error threshold OK"
