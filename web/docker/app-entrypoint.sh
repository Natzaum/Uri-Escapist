#!/bin/sh

set -eu

if [ -n "${INITIAL_TEACHER_NAME:-}" ] \
    && [ -n "${INITIAL_TEACHER_EMAIL:-}" ] \
    && [ -n "${INITIAL_TEACHER_PASSWORD:-}" ]; then
    php /var/www/html/scripts/create_teacher.php \
        --name="$INITIAL_TEACHER_NAME" \
        --email="$INITIAL_TEACHER_EMAIL" \
        --password="$INITIAL_TEACHER_PASSWORD" \
        --only-if-missing
fi

exec docker-php-entrypoint apache2-foreground
