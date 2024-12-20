#!/bin/bash
echo "Restoring db from dump..."
pg_restore -U postgres -d PostomatDB /docker-entrypoint-initdb.d/dump.dump
echo "Restore completed"