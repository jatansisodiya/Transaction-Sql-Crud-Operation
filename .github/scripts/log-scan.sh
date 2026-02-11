#!/bin/bash
if grep -r -E "password=|token=|secret=" ./logs; then
  echo "Sensitive data found in logs!"
  exit 1
else
  echo "No secrets found in logs."
fi
