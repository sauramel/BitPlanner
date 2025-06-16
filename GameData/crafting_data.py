import copy
import json
import os.path

root = 'BitCraft_GameData/server/region'
crafting_recipes = json.load(open(f'{root}/crafting_recipe_desc.json'))
items = json.load(open(f'{root}/item_desc.json'))
item_lists = json.load(open(f'{root}/item_list_desc.json'))
cargos = json.load(open(f'{root}/cargo_desc.json'))

crafting_data = {}

def find_recipes(id):
	recipes = []
	for recipe in crafting_recipes:
		for result in recipe['crafted_item_stacks']:
			if result[0] == id:
				consumed_items = []
				consumes_itself = False

				for item in recipe['consumed_item_stacks']:
					if item[0] == id:
						consumes_itself = True
						break
					consumed_items.append({ 'id': item[0], 'quantity': item[1] })
				
				if consumes_itself:
					continue

				recipe_data = {
					'level_requirements': recipe['level_requirements'][0], 
					'consumed_items': consumed_items,
					'output_quantity': result[1],
					'possibilities': {}
				}
				recipes.append(recipe_data)
	return recipes

print('Collecting items...')
for item in items:
	id = item['id']
	crafting_data[id] = {
		'name': item['name'],
		'tier': item['tier'],
		'rarity': item['rarity'][0],
		'icon': item['icon_asset_name'].replace('GeneratedIcons/', ''),
		'recipes': find_recipes(id)
	}

print('Collecing cargos...')
for item in cargos:
	id = item['id']
	if id in crafting_data.keys() and crafting_data[id]['tier'] > -1:
		continue
	crafting_data[id] = {
		'name': item['name'],
		'tier': item['tier'],
		'rarity': item['rarity'][0],
		'icon': item['icon_asset_name'].replace('GeneratedIcons/', ''),
		'recipes': find_recipes(id),
		'visible': True
	}

print('Checking icons...')
missing_icons = []
for item in crafting_data.values():
	icon = item['icon']
	if not os.path.exists(f'../BitPlanner/Assets/{icon}.png'):
		if os.path.exists(f'../BitPlanner/Assets/{icon.replace('Other/', '')}.png'):
			item['icon'] = icon.replace('Other/', '')
		else:
			missing_icons.append(icon)
if len(missing_icons) > 0:
	print('Missing icons:')
	for icon in sorted(set(missing_icons)):
		print('  ' + icon)

print('Reorganizing recipes...')
for item in items:
	id = item['id']
	list_id = item['item_list_id']
	if list_id == 0 or item['tier'] < 0:
		continue
	del crafting_data[id]

	for item_list in item_lists:
		if item_list['id'] != list_id:
			continue

		possible_recipes = {}
		for possibility in item_list['possibilities']:
			chance = possibility[0]

			for details in possibility[1]:
				target_id = details[0]
				if not target_id in crafting_data.keys():
					continue
				if not target_id in possible_recipes.keys():
					possible_recipes[target_id] = {}

				quantity = details[1]
				if not quantity in possible_recipes[target_id]:
					possible_recipes[target_id][quantity] = 0.0
				possible_recipes[target_id][quantity] += chance

		recipes = find_recipes(id)

		for target_id, possibilities in possible_recipes.items():
			if not target_id in crafting_data.keys():
				print(f'Warning: no ID {target_id} in crafting data')
				continue
			new_recipes = copy.deepcopy(recipes)
			for recipe in new_recipes:
				recipe['possibilities'] = {k: possibilities[k] for k in sorted(possibilities)}
			crafting_data[target_id]['recipes'].extend(new_recipes)

		break

print('Cleanup...')
for item in crafting_data.values():
	recipes = item['recipes']
	deduplicated_recipes = {json.dumps(r, sort_keys=True) for r in recipes}
	recipes = [json.loads(r) for r in deduplicated_recipes]
	recipes.sort(key=lambda recipe: recipe['consumed_items'][0]['quantity'])
	item['recipes'] = recipes

json.dump(crafting_data, open('../BitPlanner/crafting_data.json', 'w'), indent=2)
