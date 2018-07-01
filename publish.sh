#!/bin/bash

rm -rf "./build"

dotnet publish ./SharpCraft.sln -c release -o "../build/SharpCraft"

mkdir -p "./build/SharpCraft/SharpCraft_Data"
cp -r "./SharpCraft/SharpCraft_Data" "./build/SharpCraft/SharpCraft_Data"