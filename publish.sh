#!/bin/bash

rm -rf "./build"

dotnet publish ./SharpCraft.sln -c release -o "../build/SharpCraft"

cp -r "./SharpCraft/SharpCraft_Data" "./build/SharpCraft/SharpCraft_Data"