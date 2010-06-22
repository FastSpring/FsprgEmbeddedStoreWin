#!/bin/sh

rm -R -f FsprgEmbeddedStoreWinSDK
./build_project.sh "TestApp" "Release" "FsprgEmbeddedStoreWinSDK"
./build_project.sh "FsprgEmbeddedStore" "Source" "FsprgEmbeddedStoreWinSDK/src"
./build_project.sh "Example1" "Source" "FsprgEmbeddedStoreWinSDK/src"

# FsprgEmbeddedStoreStyle.zip
zip -r FsprgEmbeddedStoreStyle FsprgEmbeddedStoreStyle -x *.svn* *.DS_Store*
mv FsprgEmbeddedStoreStyle.zip FsprgEmbeddedStoreWinSDK 

# README.html
mkdir -p FsprgEmbeddedStoreWinSDK/README
cp -R -f README/* FsprgEmbeddedStoreWinSDK/README
perl ./Markdown_1.0.1/Markdown.pl --html4tags README.mdown >> ./FsprgEmbeddedStoreWinSDK/README.html

# License.txt
cp -R License.txt FsprgEmbeddedStoreWinSDK

# Package and remove temp directory
zip -r FsprgEmbeddedStoreWinSDK FsprgEmbeddedStoreWinSDK
rm -R -f FsprgEmbeddedStoreWinSDK