import argparse
import sqlite3
from pathlib import Path


def query_prerequisites(conn: sqlite3.Connection, item_name: str):
    cur = conn.cursor()
    cur.execute(
        """
        WITH RECURSIVE deps(id, name, depth, path) AS (
            SELECT id, name, 0, ',' || id || ',' FROM items WHERE name LIKE ?
            UNION ALL
            SELECT ri.ingredient_id, i.name, depth + 1, path || ri.ingredient_id || ','
            FROM deps
            JOIN recipes r ON r.item_id = deps.id
            JOIN recipe_ingredients ri ON ri.recipe_id = r.id
            JOIN items i ON i.id = ri.ingredient_id
            WHERE path NOT LIKE '%,' || ri.ingredient_id || ',%'
        )
        SELECT id, name, depth FROM deps ORDER BY depth, name
        """,
        (item_name,)
    )
    for item_id, name, depth in cur.fetchall():
        print("  " * depth + f"{name} (#{item_id})")


def main() -> None:
    parser = argparse.ArgumentParser(description="Query prerequisites for an item")
    parser.add_argument("item", help="Item name or substring to search")
    parser.add_argument("db_path", nargs="?", default="crafting_data.db", help="Path to SQLite database")
    args = parser.parse_args()

    conn = sqlite3.connect(Path(args.db_path))
    query_prerequisites(conn, args.item)
    conn.close()


if __name__ == "__main__":
    main()
