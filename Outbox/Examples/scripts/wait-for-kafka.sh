#!/bin/sh

echo "Wait for Kafka is available";

set -e;
wait-for-port \
  --host=kafka \
  --state=inuse \
  --timeout=120 \
  9092;

echo "Kafka is available";