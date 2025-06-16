#!/bin/bash

python3 ./crafting_data.py
python3 ./travelers_data.py

echo "Writing game data version..."
cd BitCraft_GameData
commitDate=`git show -s --format=%ci | cut -f1 -d' '`
echo "${commitDate}" > ../../BitPlanner/data_version.txt
echo "Done!"