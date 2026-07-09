#!/usr/bin/env bash
set -euo pipefail

USERS_URL="${USERS_URL:-http://localhost:${TC2_USERS_PORT:-18080}}"
CATALOG_URL="${CATALOG_URL:-http://localhost:${TC2_CATALOG_PORT:-18081}}"
ADMIN_EMAIL="${TC2_ADMIN_EMAIL:-admin@fcg.local}"
ADMIN_PASSWORD="${TC2_ADMIN_PASSWORD:?Set TC2_ADMIN_PASSWORD in the environment or .env}"

require_tool() {
  command -v "$1" >/dev/null 2>&1 || {
    echo "Missing required tool: $1" >&2
    exit 1
  }
}

wait_for_http() {
  local url="$1"
  local name="$2"

  for _ in $(seq 1 60); do
    if curl -fsS "$url" >/dev/null; then
      echo "$name is ready"
      return 0
    fi
    sleep 2
  done

  echo "$name did not become ready: $url" >&2
  exit 1
}

post_json() {
  local url="$1"
  local payload="$2"
  local token="${3:-}"

  if [ -n "$token" ]; then
    curl -fsS \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $token" \
      -d "$payload" \
      "$url"
  else
    curl -fsS \
      -H "Content-Type: application/json" \
      -d "$payload" \
      "$url"
  fi
}

require_tool curl
require_tool jq

wait_for_http "$USERS_URL/health" "UsersAPI"
wait_for_http "$CATALOG_URL/health" "CatalogAPI"

run_id="$(date +%s)"
customer_email="customer-$run_id@fcg.local"
customer_password="UserPass1!"

admin_login="$(post_json "$USERS_URL/api/auth/login" \
  "$(jq -n --arg email "$ADMIN_EMAIL" --arg password "$ADMIN_PASSWORD" \
    '{email:$email,password:$password}')")"
admin_token="$(jq -r '.accessToken // .AccessToken' <<<"$admin_login")"

approved_game="$(post_json "$CATALOG_URL/api/games" \
  "$(jq -n --arg title "Approved Demo $run_id" \
    '{title:$title,description:"Approved purchase smoke test",developer:"FIAP",price:59.90}')" \
  "$admin_token")"
approved_game_id="$(jq -r '.id // .Id' <<<"$approved_game")"

rejected_game="$(post_json "$CATALOG_URL/api/games" \
  "$(jq -n --arg title "Rejected Demo $run_id" \
    '{title:$title,description:"Rejected purchase smoke test",developer:"FIAP",price:150.00}')" \
  "$admin_token")"
rejected_game_id="$(jq -r '.id // .Id' <<<"$rejected_game")"

register_response="$(post_json "$USERS_URL/api/auth/register" \
  "$(jq -n --arg email "$customer_email" --arg password "$customer_password" \
    '{name:"Smoke Customer",email:$email,password:$password}')")"
customer_token="$(jq -r '.accessToken // .AccessToken' <<<"$register_response")"

post_json "$CATALOG_URL/api/me/library/games/$approved_game_id" "{}" "$customer_token" >/dev/null
sleep 8

library_after_approved="$(curl -fsS \
  -H "Authorization: Bearer $customer_token" \
  "$CATALOG_URL/api/me/library/games")"

if ! jq -e --arg id "$approved_game_id" \
  'map((.id // .Id) == $id) | any' <<<"$library_after_approved" >/dev/null; then
  echo "Approved game was not added to the user library." >&2
  echo "$library_after_approved" >&2
  exit 1
fi

post_json "$CATALOG_URL/api/me/library/games/$rejected_game_id" "{}" "$customer_token" >/dev/null
sleep 8

library_after_rejected="$(curl -fsS \
  -H "Authorization: Bearer $customer_token" \
  "$CATALOG_URL/api/me/library/games")"

if jq -e --arg id "$rejected_game_id" \
  'map((.id // .Id) == $id) | any' <<<"$library_after_rejected" >/dev/null; then
  echo "Rejected game was unexpectedly added to the user library." >&2
  echo "$library_after_rejected" >&2
  exit 1
fi

echo "Smoke test passed."
echo "Customer: $customer_email"
echo "Approved game: $approved_game_id"
echo "Rejected game: $rejected_game_id"
