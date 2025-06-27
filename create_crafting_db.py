import argparse
import json
import sqlite3
from pathlib import Path


def build_database(json_path: Path, db_path: Path) -> None:
    data = json.loads(Path(json_path).read_text())
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()

    cur.execute(
        """
        CREATE TABLE IF NOT EXISTS items (
            id INTEGER PRIMARY KEY,
            name TEXT,
            tier INTEGER,
            rarity INTEGER,
            icon TEXT,
            extraction_skill INTEGER
        )
        """
    )
    cur.execute(
        """
        CREATE TABLE IF NOT EXISTS recipes (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            item_id INTEGER,
            output_quantity INTEGER,
            level_requirement INTEGER,
            FOREIGN KEY(item_id) REFERENCES items(id)
        )
        """
    )
    cur.execute(
        """
        CREATE TABLE IF NOT EXISTS recipe_ingredients (
            recipe_id INTEGER,
            ingredient_id INTEGER,
            quantity INTEGER,
            FOREIGN KEY(recipe_id) REFERENCES recipes(id),
            FOREIGN KEY(ingredient_id) REFERENCES items(id)
        )
        """
    )
    cur.execute(
        """
        CREATE TABLE IF NOT EXISTS recipe_possibilities (
            recipe_id INTEGER,
            quantity INTEGER,
            chance REAL,
            FOREIGN KEY(recipe_id) REFERENCES recipes(id)
        )
        """
    )
    conn.commit()

    for item_id_str, item in data.items():
        item_id = int(item_id_str)
        cur.execute(
            "INSERT OR REPLACE INTO items (id, name, tier, rarity, icon, extraction_skill) "
            "VALUES (?, ?, ?, ?, ?, ?)",
            (
                item_id,
                item.get("name"),
                item.get("tier"),
                item.get("rarity"),
                item.get("icon"),
                item.get("extraction_skill", -1),
            ),
        )
        for recipe in item.get("recipes", []):
            level_req = recipe.get("level_requirements")
            lvl = level_req[0] if level_req else None
            cur.execute(
                "INSERT INTO recipes (item_id, output_quantity, level_requirement) VALUES (?, ?, ?)",
                (item_id, recipe.get("output_quantity"), lvl),
            )
            recipe_id = cur.lastrowid
            for ing in recipe.get("consumed_items", []):
                cur.execute(
                    "INSERT INTO recipe_ingredients (recipe_id, ingredient_id, quantity) VALUES (?, ?, ?)",
                    (recipe_id, ing.get("id"), ing.get("quantity")),
                )
            for qty, chance in recipe.get("possibilities", {}).items():
                cur.execute(
                    "INSERT INTO recipe_possibilities (recipe_id, quantity, chance) VALUES (?, ?, ?)",
                    (recipe_id, int(qty), chance),
                )
    conn.commit()
    conn.close()


def main() -> None:
    parser = argparse.ArgumentParser(description="Convert crafting_data.json to SQLite database")
    parser.add_argument("json_path", nargs="?", default="BitPlanner/crafting_data.json", help="Path to crafting_data.json")
    parser.add_argument("db_path", nargs="?", default="crafting_data.db", help="Output SQLite database path")
    args = parser.parse_args()

    build_database(Path(args.json_path), Path(args.db_path))


if __name__ == "__main__":
    main()
