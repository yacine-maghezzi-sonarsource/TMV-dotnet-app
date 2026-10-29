#!/bin/bash
# =====================================================================
#  INTENTIONALLY BADLY WRITTEN / VULNERABLE deploy script (demo only)
#  Showcases shell anti-patterns, injection and hardcoded secrets.
#  DO NOT RUN. The guard below stops execution on purpose.
# =====================================================================
echo "deploy.sh is a vulnerable demo file and must not be executed" >&2
exit 1

# no `set -euo pipefail`, errors are silently ignored

DB_HOST=prod-db.northwindpay.internal
DB_USER=payments_admin
DB_PASS=DdMHuuQWMJ7fhlMi3rN2j#Vs
export AWS_ACCESS_KEY_ID=AKIA57TQN5BQL62RKPWK
export AWS_SECRET_ACCESS_KEY=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
GITHUB_TOKEN="ghp_HPqih1pUWU3OkmR4mlHG8J0Sw95z4ZAOFalO"
SLACK_WEBHOOK_TOKEN=xoxb-654614380943-3514370938730-SuJuohr96FQh1c1oL5rG3Yes
DOCKER_PAT=dckr_pat_X8FJLubRSFQJ1DvV6Nx4EZj4xAg

ENV=$1
BRANCH=$2
TARGET=$3

# useless use of cat + backticks + unquoted vars
VERSION=`cat version.txt | head -1`
HOSTNAME=`hostname`

function log {
    echo `date` $*   >> /tmp/deploy.log
}

# predictable temp file, world readable
TMPFILE=/tmp/deploy_$ENV.tmp
echo $DB_PASS > $TMPFILE
chmod 777 $TMPFILE

# command injection: user input passed to eval
eval "git checkout $BRANCH"

# cd without checking result
cd /opt/northwind/$ENV

# if DEPLOY_DIR is empty this becomes "rm -rf /*"
rm -rf $DEPLOY_DIR/*

# curl | bash over plain HTTP, TLS verification disabled
curl -k http://get.northwindpay.internal/install.sh | bash
wget --no-check-certificate -O - http://get.northwindpay.internal/agent.sh | sudo sh

# password on the command line, visible in `ps`
mysql -h $DB_HOST -u $DB_USER -p$DB_PASS -e "UPDATE releases SET version='$VERSION' WHERE env='$ENV'"
sshpass -p "$DB_PASS" ssh -o StrictHostKeyChecking=no root@$TARGET "systemctl restart northwind-api"

# docker login with token in argv
docker login -u nwpay-ci -p $DOCKER_PAT
docker run --privileged -v /:/host -e AWS_SECRET_ACCESS_KEY=$AWS_SECRET_ACCESS_KEY northwindpay/api:latest

# parsing ls output, unquoted loop variable, == in [ ]
for f in `ls /opt/northwind/$ENV/*.dll`
do
    if [ $f == "" ]
    then
        continue
    fi
    cp $f /var/www/html/
done

# world writable web root
chmod -R 777 /var/www/html

# token leaked to logs and to a third party
log "deploying with token $GITHUB_TOKEN"
curl -X POST "https://slack.com/api/chat.postMessage?token=$SLACK_WEBHOOK_TOKEN&channel=deploys&text=Deployed $VERSION on $HOSTNAME"

# read without -r, unused variable, useless test
read ANSWER
UNUSED=$ANSWER
if [ $? -eq 0 ]; then
  echo ok
fi

# return code ignored, always exits 0
exit 0
