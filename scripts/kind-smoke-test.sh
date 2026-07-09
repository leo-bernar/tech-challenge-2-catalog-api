#!/usr/bin/env bash
set -euo pipefail

USERS_PORT="${K8S_USERS_PORT:-18090}"
CATALOG_PORT="${K8S_CATALOG_PORT:-18091}"
ADMIN_PASSWORD="${TC2_ADMIN_PASSWORD:?Set TC2_ADMIN_PASSWORD}"

USERS_LOG="$(mktemp)"
CATALOG_LOG="$(mktemp)"
USERS_PID=""
CATALOG_PID=""

cleanup() {
  if [ -n "$USERS_PID" ]; then
    kill "$USERS_PID" >/dev/null 2>&1 || true
  fi
  if [ -n "$CATALOG_PID" ]; then
    kill "$CATALOG_PID" >/dev/null 2>&1 || true
  fi
  rm -f "$USERS_LOG" "$CATALOG_LOG"
}

trap cleanup EXIT

kubectl port-forward service/users-api "$USERS_PORT:80" >"$USERS_LOG" 2>&1 &
USERS_PID="$!"

kubectl port-forward service/catalog-api "$CATALOG_PORT:80" >"$CATALOG_LOG" 2>&1 &
CATALOG_PID="$!"

sleep 3

TC2_ADMIN_PASSWORD="$ADMIN_PASSWORD" \
  USERS_URL="http://127.0.0.1:$USERS_PORT" \
  CATALOG_URL="http://127.0.0.1:$CATALOG_PORT" \
  ./scripts/smoke-test.sh
