#!/usr/bin/env bash
set -euo pipefail

SQL_PASSWORD="${TC2_SQL_PASSWORD:?Set TC2_SQL_PASSWORD}"
RABBITMQ_USERNAME="${TC2_RABBITMQ_USERNAME:?Set TC2_RABBITMQ_USERNAME}"
RABBITMQ_PASSWORD="${TC2_RABBITMQ_PASSWORD:?Set TC2_RABBITMQ_PASSWORD}"
JWT_KEY="${TC2_JWT_KEY:?Set TC2_JWT_KEY}"
ADMIN_PASSWORD="${TC2_ADMIN_PASSWORD:?Set TC2_ADMIN_PASSWORD}"

if [ "${#JWT_KEY}" -lt 32 ]; then
  echo "TC2_JWT_KEY must contain at least 32 characters." >&2
  exit 1
fi

cat > k8s/all/secret.local.yaml <<EOF
apiVersion: v1
kind: Secret
metadata:
  name: sqlserver-secret
type: Opaque
stringData:
  MSSQL_SA_PASSWORD: "$SQL_PASSWORD"
---
apiVersion: v1
kind: Secret
metadata:
  name: rabbitmq-secret
type: Opaque
stringData:
  RabbitMq__Username: "$RABBITMQ_USERNAME"
  RabbitMq__Password: "$RABBITMQ_PASSWORD"
---
apiVersion: v1
kind: Secret
metadata:
  name: users-api-secrets
type: Opaque
stringData:
  ConnectionStrings__UsersDatabase: "Server=sqlserver,1433;Database=FcgUsersDb;User Id=sa;Password=$SQL_PASSWORD;TrustServerCertificate=True;Encrypt=False"
  Jwt__Key: "$JWT_KEY"
  RabbitMq__Username: "$RABBITMQ_USERNAME"
  RabbitMq__Password: "$RABBITMQ_PASSWORD"
  AdminSeed__Password: "$ADMIN_PASSWORD"
---
apiVersion: v1
kind: Secret
metadata:
  name: catalog-api-secret
type: Opaque
stringData:
  ConnectionStrings__CatalogDatabase: "Server=sqlserver,1433;Database=FcgCatalogDb;User Id=sa;Password=$SQL_PASSWORD;TrustServerCertificate=True;Encrypt=False"
  Jwt__Key: "$JWT_KEY"
  RabbitMq__Username: "$RABBITMQ_USERNAME"
  RabbitMq__Password: "$RABBITMQ_PASSWORD"
---
apiVersion: v1
kind: Secret
metadata:
  name: payments-api-secrets
type: Opaque
stringData:
  RabbitMq__Username: "$RABBITMQ_USERNAME"
  RabbitMq__Password: "$RABBITMQ_PASSWORD"
---
apiVersion: v1
kind: Secret
metadata:
  name: notifications-api-secrets
type: Opaque
stringData:
  RabbitMq__Username: "$RABBITMQ_USERNAME"
  RabbitMq__Password: "$RABBITMQ_PASSWORD"
EOF

echo "Generated k8s/all/secret.local.yaml"
