#!/bin/sh

rm -R -f FsprgEmbeddedStoreWinSDK
./build_project.sh "TestApp" "Release" "FsprgEmbeddedStoreWinSDK"
./build_project.sh "FsprgEmbeddedStore" "Source" "FsprgEmbeddedStoreWinSDK/src"
./build_project.sh "Example1" "Source" "FsprgEmbeddedStoreWinSDK/src"

# FsprgEmbeddedStoreStyle.zip
zip -r FsprgEmbeddedStoreStyle FsprgEmbeddedStoreStyle -x *.svn* *.DS_Store*
mv FsprgEmbeddedStoreStyle.zip FsprgEmbeddedStoreWinSDK 

# HOW_TO.html
mkdir -p FsprgEmbeddedStoreWinSDK/HOW_TO
cp -R -f HOW_TO/* FsprgEmbeddedStoreWinSDK/HOW_TO
perl ./Markdown_1.0.1/Markdown.pl --html4tags HOW_TO.mdown >> ./FsprgEmbeddedStoreWinSDK/HOW_TO.html

# License.txt
cp -R License.txt FsprgEmbeddedStoreWinSDK

# Package and remove temp directory
zip -r FsprgEmbeddedStoreWinSDK FsprgEmbeddedStoreWinSDK
rm -R -f FsprgEmbeddedStoreWinSDK