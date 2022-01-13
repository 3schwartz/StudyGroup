#!/bin/sh

curl -s \
     -X "POST" "http://localhost:8083/connectors/" \
     -H "Content-Type: application/json" \
     -d '{
  "name": "jdbc_source_postgres_outbox",
  "config": {
    "connector.class": "io.confluent.connect.jdbc.JdbcSourceConnector",
      "connection.url": "jdbc:postgresql://postgres:5432/outbox",
    "connection.user": "postgres",
    "connection.password": "postgres",
    "topic.prefix": "postgres-outbox-messages",
    "mode": "timestamp",
    "query": "select \"Id\"::varchar as \"Id\", \"Message\", \"CreatedDateTime\" from outbox.outbox_messages_connect",  
    "timestamp.column.name": "CreatedDateTime",
    "validate.non.null": "false",
    "poll.interval.ms": 500
  }
}'