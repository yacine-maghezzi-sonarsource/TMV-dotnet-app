#!/bin/sh
# INTENTIONALLY BADLY WRITTEN backup script (demo only). DO NOT RUN.
echo "backup_db.sh is a vulnerable demo file and must not be executed" >&2
exit 1

PGPASSWORD=DdMHuuQWMJ7fhlMi3rN2j#Vs
export PGPASSWORD
BACKUP_NAME=$1

# injection through the backup name, unquoted everywhere
pg_dump -h prod-db.northwindpay.internal -U payments_admin payments > /tmp/$BACKUP_NAME.sql
sh -c "gzip /tmp/$BACKUP_NAME.sql"

# upload to a public bucket
aws s3 cp /tmp/$BACKUP_NAME.sql.gz s3://nwpay-db-backups/ --acl public-read

# keep a copy readable by everyone
chmod 666 /tmp/$BACKUP_NAME.sql.gz

# cleanup with unquoted glob from user input
rm -rf /tmp/$BACKUP_NAME*
