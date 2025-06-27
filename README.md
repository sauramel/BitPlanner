# Game Data Tools

This repository contains scripts for converting BitCraft game data into JSON
files and a SQLite database for offline use.

## Generating JSON data

The `GameData` directory includes a submodule with the original BitCraft game
files and helper scripts:

```bash
cd GameData
./generate_data.sh
```

Running the script creates `crafting_data.json` and `travelers_data.json` in the
repository root along with a `data_version.txt` file containing the commit date
of the game data.

## Creating the SQLite database

Use `create_crafting_db.py` to convert `crafting_data.json` into a SQLite
database:

```bash
python3 create_crafting_db.py
```

By default it produces `crafting_data.db` in the current directory.
