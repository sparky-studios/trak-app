#!/usr/bin/env bash

touch $APPCENTER_SOURCE_DIRECTORY/SparkyStudios.TrakLibrary.Android/google-services.json
GOOGLE_JSON_FILE=$APPCENTER_SOURCE_DIRECTORY/SparkyStudios.TrakLibrary.Android/google-services.json

# Update the google json file with the value provided by the environment variable.
if [ -e "$GOOGLE_JSON_FILE" ]
then
    echo "Updating Google Json"
    echo "$GOOGLE_JSON" > $GOOGLE_JSON_FILE
    sed -i -e 's/\\"/'\"'/g' $GOOGLE_JSON_FILE
fi
