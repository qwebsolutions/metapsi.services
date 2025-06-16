#!/bin/sh

if [ -z "$1" ]
then
    echo 'Version not specified'
    exit 1
else
    echo 'Packing version '$1
fi

REPO_URL=$(git config --get remote.origin.url)
REPO_BRANCH=$(git branch --show-current)
REPO_COMMIT=$(git rev-parse HEAD)

rm ./nugets -rf
version=$1
outputFolder=nugets
echo "Output folder: $outputFolder"
echo "Version: $version"
dotnet pack ../Metapsi.ServiceDoc -o $outputFolder -c Release -p:Version="$version" -p:RepositoryUrl="$REPO_URL" -p:RepositoryBranch="$REPO_BRANCH" -p:RepositoryCommit="$REPO_COMMIT" -p:RepositoryType="git"
dotnet pack ../Metapsi.WhatsApp -o $outputFolder -c Release -p:Version="$version" -p:RepositoryUrl="$REPO_URL" -p:RepositoryBranch="$REPO_BRANCH" -p:RepositoryCommit="$REPO_COMMIT" -p:RepositoryType="git"