import json

root = 'BitCraft_GameData/server/region'
npcs = json.load(open(f'{root}/npc_desc.json'))
tasks = json.load(open(f'{root}/traveler_task_desc.json'))
crafting_data = json.load(open('../BitPlanner/crafting_data.json'))

cargo_offset = 0xffffffff
travelers_data = []

print('Getting NPCs info...')
for npc in npcs:
    if len(npc['task_skill_check']) == 0:
        continue
    skill = npc['task_skill_check'][0]
    traveler = {
        'name': npc['name'],
        'skill': skill,
        'tasks': []
    }
    travelers_data.append(traveler)

print('Collecting tasks...')
for task in tasks:
    id = task['id']
    skill = task['level_requirement']['skill_id']
    if skill != task['rewarded_experience']['skill_id']:
        print(f'Task {id} gives experience to a skill other than the one that is required, skipping the task')
        continue

    traveler = None
    for t in travelers_data:
        if t['skill'] == skill:
            traveler = t
            break
    if traveler == None:
        print(f'Task {id} requires skill with unknown id {skill}, skipping the task')
        continue

    required_items = {}
    for item in task['required_items']:
        item_id = item[0] + (cargo_offset if item[2][0] == 1 else 0)
        if str(item_id) in crafting_data.keys():
            required_items[item_id] = item[1]
        else:
            required_items.clear()
            print(f'Task {id} requires unavailable item {item_id}, skipping the task')
            break
    if len(required_items) == 0:
        continue

    reward = task['rewarded_items']
    if len(reward) > 1 or reward[0][0] != 1:
        print(f'Unexpected reward in task {id}, skipping the task')
        continue

    output = {
        'levels': [
            task['level_requirement']['min_level'],
            task['level_requirement']['max_level']
        ],
        'required_items': required_items,
        'reward': reward[0][1],
        'experience': task['rewarded_experience']['quantity']
    }
    traveler['tasks'].append(output)

for traveler in travelers_data:
    traveler['tasks'].sort(key=lambda task: task['levels'][0])

json.dump(travelers_data, open('../BitPlanner/travelers_data.json', 'w'), indent=2)
