#FDEV NDA licensed https://assets.station13.ru/LICENSE.txt
import yaml
import os
import re

locale_template = "ent-{} = {}\n   .desc = {}\n"
suffix_template = "   .suffix = {}\n"

enitites_path = "../Resources/Prototypes/Entities"
locales_path = "../Resources/Locale/ru-RU"
new_locales_path = locales_path + "/_white/locales-new"

existing_prototypes = {}
existing_locales = []

#counter for poor ss14 to now break with > 20 locales in one file
locales = 0
new_filename = 0
num = 0
locale_text = ""

#treat custom constructors as text
def default_ctor(loader, tag_suffix, node):
    return f"{tag_suffix} {node.value}"

yaml.add_multi_constructor('', default_ctor)

#cache all existing enitites
for root, dirs, files in os.walk(enitites_path):
    for filename in files:
        with open(f"{root}/{filename}", "r", encoding="utf-8") as file:
            data = yaml.full_load_all(file)
            if not data: continue
            for entrylist in data:
                for entry in entrylist:
                    entry: dict
                    if entry.get("type", None) != "entity": continue
                    if entry.get("abstract", False) or entry.get("noSpawn", False): continue
                    name = entry.get("name", None)
                    if not name: continue
                    existing_prototypes[entry["id"]] = {"name":name, "desc":entry.get("description", f'"{name}"') or "", "suffix":entry.get("suffix", None)}

#cache all existing locales
for root, dirs, files in os.walk(locales_path):
    for filename in files:
        if "autotranslate-" in filename:
            _num = filename.split("autotranslate-")[1]
            _num = int(_num.split(".")[0])
            if _num >= num:
                num = _num
                new_filename = _num + 1
        with open(f"{root}/{filename}", "r", encoding="utf-8") as file:
            data = re.findall(r'(?<=^ent-)(.+)(?= =)', file.read(), re.MULTILINE)
            if not len(data): continue
            for entity in data:
                existing_locales.append(entity)

os.makedirs(new_locales_path, exist_ok=True)

#create locales for not existing entities
for entity, entity_data in existing_prototypes.items():
    if entity in existing_locales: continue
    name = entity_data["name"]
    desc = entity_data["desc"]
    suffix = entity_data["suffix"]
    locale_text += locale_template.format(entity, name, desc)
    if suffix:
        locale_text += suffix_template.format(suffix)
    locales += 1
    if locales == 20:
        with open(f"{new_locales_path}/autotranslate-{new_filename}.ftl", "w", encoding="utf-8") as file:
            file.write(locale_text)
        locale_text = ""
        new_filename += 1
        locales = 0

#write remaining stuff if we have that
if locale_text:
    with open(f"{new_locales_path}/autotranslate-{new_filename}.ftl", "w", encoding="utf-8") as file:
            file.write(locale_text)
    new_filename += 1

print(f"Wrote {new_filename-num-1} new files.")
