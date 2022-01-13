#!/bin/sh

for topic in $(cat ./scripts/topics.txt)
do
    echo "Ensure topic $topic exists"
    /opt/bitnami/kafka/bin/kafka-topics.sh \
      --create \
      --if-not-exists \
      --bootstrap-server kafka:9092 \
      --replication-factor 1 \
      --partitions 1 \
      --topic $topic
done
