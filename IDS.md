# Идентификаторы сущностей (EntityId)

Все связи между модельными сущностями (конфиги, экземпляры в инвентаре, будущие сейвы) строятся через типизированные строковые id. Ссылки на ассеты, `Guid` и `QGuid` для этого не используем: `QGuid` из проекта удалён.

## 1. Формат
`Entity_Variant`: имя сущности, затем вариант, разделитель `_`. Слова варианта тоже через `_`, регистр сохраняется.

| Тип | Пример | Откуда берётся |
|---|---|---|
| `SkinId` | `AK-47_Redline`, `Desert_Eagle_Blaze` | таблица (страница Skins), оружие + имя скина |
| `CaseId` | `Case_Inferno` | таблица (страница Cases), `Tools/Case Editor` |
| `SkinCollectionId` | `Collection_Vertigo`, `Collection_2021_Dust_2` | `Tools/Skin Collections/Fill From CSGO-API` |
| `SkinInstanceId` | `SkinInstance_6318af7f6cbd45e4aa9f09f084d479d7` | генерируется `SkinFactory` при создании скина |

- Для конфигов вариант читаемый и стабильный: id не меняем после того, как он попал в сейвы.
- Для экземпляров (runtime-сущностей) вариант — `Guid` без дефисов: `EntityIds.CreateUnique<TId>(TId.EntityName)`.
- Вариант из произвольного текста: `EntityIds.ToVariant("Dreams & Nightmares")` = `Dreams_And_Nightmares`; `EntityIds.Compose<TId>(entityName, text)` собирает id целиком.

## 2. Ядро (`Shared.Identifiers`)
- `EntityId`: абстрактный `[Serializable]` класс с одной сериализуемой строкой `_value`.
  - `Value`: строка id, никогда не `null`; `IsEmpty`: значение пустое.
  - Равенство по типу и значению (ordinal): `SkinId("x") != CaseId("x")`. Есть `==`/`!=`, `GetHashCode`, поэтому id годится ключом `Dictionary`/`HashSet` и элементом кортежа-ключа.
  - `ToString()` возвращает `Value`.
- `EntityIds` (статический): `Parse<TId>(string)` (строка обратно в id), `CreateUnique<TId>(entityName)`, `Compose<TId>(entityName, text)`, `ToVariant(text)`, `Separator`.

**Конфликт имён.** В Unity 6.3 есть свой `UnityEngine.EntityId`. В файле, где есть `using UnityEngine;` и нужен наш базовый тип, добавь алиас:
```csharp
using EntityId = Shared.Identifiers.EntityId;
```
Конкретные id (`SkinId` и т.д.) конфликта не вызывают.

## 3. Новый тип id
1. Класс в `Data/` фичи, которой принадлежит сущность:
```csharp
[Serializable]
public sealed class WeaponId : EntityId
{
    public const string EntityName = "Weapon";

    public WeaponId()
    {
    }

    public WeaponId(string value) : base(value)
    {
    }
}
```
   Обязательно: `sealed`, `[Serializable]`, публичные конструкторы без параметров и со строкой (на них опирается `EntityIds.Parse`). Это проверяет `EntityIdTests.ConcreteIds_AllSealedWithStringConstructor`. `EntityName` нужен, если id генерируется или собирается из текста.
2. Поле в конфиге инициализируй: `public WeaponId Id = new WeaponId();`. Тогда пустое значение — это `IsEmpty`, а не `null`. Поле, которое задаёт id самой сущности (а не ссылается на чужую), помечай `[EntityIdDefinition]`: `[EntityIdDefinition] public WeaponId Id = new WeaponId();`.
3. Drawer в `Editor/Identifiers/` фичи (сборка `Features.<Name>.Editor` со ссылками на `Shared` и `Shared.Editor`):
```csharp
[CustomPropertyDrawer(typeof(WeaponId))]
public class WeaponIdPropertyDrawer : EntityIdPropertyDrawer<WeaponId>
{
    protected override IEnumerable<WeaponId> GetIds()
    {
        return AssetDatabaseTools.FindAllAssets<WeaponConfig>().Select(config => config.Id);
    }
}
```
   Пара `XxxId` / `XxxIdPropertyDrawer`. Опционально переопредели `GetGroup(id)`, чтобы сгруппировать выпадающий список (так `SkinIdPropertyDrawer` группирует по оружию).
4. Тест контента: id заполнены и уникальны, ссылки указывают на существующие id (примеры: `SkinsContentIdsTests`, `CasesContentIdsTests`).

## 4. Инспектор (`Shared.Editor.Identifiers`)
`EntityIdPropertyDrawer<TId>`:
- ссылка на сущность — только выпадающий список с поиском (`AdvancedDropdown`) из `GetIds()` плюс `<none>`; значение, которого нет в списке, подсвечивается красным;
- поле с `[EntityIdDefinition]` (собственный id конфига или записи реестра) — исключение: обычное поле ввода без выпадающего списка. Пустое значение или id, который уже есть у другой сущности того же типа, подсвечивается красным.

## 5. Реестры
Если у сущности нет собственного конфига, список допустимых id хранится в конфиге-реестре. Пример: `SkinCollectionsConfig` (`Features/Skins/Content/Configs/SkinCollectionsConfig.asset`): `Id` + отображаемое `Name`. Его заполняет `Tools/Skin Collections/Fill From CSGO-API` (`SkinCollectionNaming.ToId`: `The Vertigo Collection` → `Collection_Vertigo`), он же источник для `SkinCollectionIdPropertyDrawer`.

## 6. Связи и сейвы
- Модель ссылается на конфиг через id, а не через ссылку на ScriptableObject: `Skin.ConfigId` (`SkinId`) → `ISkinsDatabaseService.GetSkinConfigById`; `SkinConfig.Collection` (`SkinCollectionId`) → `SkinCollectionsConfig`; кейс открывается по `CaseId` (`ICasesDatabaseService.GetCaseById`).
- Экземпляры адресуются своим id: инвентарь хранит `Dictionary<SkinInstanceId, Skin>`, панели и попапы передают `SkinInstanceId`.
- Сейвы пишут id строкой (`Value`) или как `[Serializable]`-поле (`JsonUtility` даёт `{"_value":"..."}`), а читают через `new XxxId(value)` или `EntityIds.Parse<XxxId>(value)`. Ссылки на ассеты в сейвы не кладём.
- Переименование id конфига ломает сейвы и таблицы: если всё же нужно, делай миграцию старого значения в новое.
