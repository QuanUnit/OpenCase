# OpenCase: карта проекта для агентов

Unity-игра про открытие кейсов (скины, инвентарь, продажа). Читай этот файл в начале сессии.

## 1. Стек
- Unity 6000.3.25f1, URP.
- Zenject (форк QCore), UniTask, Addressables, DOTween, NaughtyAttributes.
- QCore: UI (панели, презентеры), StateMachine, Tools, AddressablesWrapper (пакеты, не трогаем).
- NuGetForUnity: CsvHelper (`Assets/packages.config`). NSubstitute не используем (см. раздел 8).

## 2. Карта сборок
Код живёт в `Game/{Kernel,Shared,Features/*}` внутри `Assets`. Имя asmdef = корневой namespace = путь без `Assets/Game/`.

| Сборка | Путь | Назначение | Зависит от (наши) |
|---|---|---|---|
| `Shared` | `Assets/Game/Shared` | Lifecycle-интерфейсы, `BindPanelPresenter`, SelectableSet, Tools, общие шрифты/текстуры/шаблоны | нет |
| `Features.Wallet` | `Assets/Game/Features/Wallet` | кошелёк, мягкая валюта | нет |
| `Features.Skins` | `Assets/Game/Features/Skins` | модели скинов, базы, `SkinFactory`, конфиги | Shared |
| `Features.SlotMachine` | `Assets/Game/Features/SlotMachine` | виджет горизонтальной рулетки (без инсталлера) | Skins, Shared |
| `Features.Inventory` | `Assets/Game/Features/Inventory` | инвентарь, продажа, `SelectedSkinsBuffer` | Skins, Wallet, Shared |
| `Features.OthersDrop` | `Assets/Game/Features/OthersDrop` | лента дропа других игроков | Skins, Shared |
| `Features.Cases` | `Assets/Game/Features/Cases` | кейсы, спин кейса | Skins, Inventory, Wallet, SlotMachine, Shared |
| `Features.GameHub` | `Assets/Game/Features/GameHub` | главная панель-хаб | Cases, Inventory, OthersDrop, Wallet, Shared |
| `Kernel` | `Assets/Game/Kernel` | бутстрап, state machine, DI-корень, фабрика презентеров | все фичи, Shared |
| `Features.RemoteConfigs.Editor` | `Assets/Game/Features/RemoteConfigs/Editor` | импорт конфигов из таблиц (только Editor) | Skins, Cases, Shared |

- Граница закреплена asmdef: недопустимая зависимость не компилируется. Новую ссылку добавляй в asmdef только при реальной необходимости.
- Фичи не ссылаются на Kernel. Kernel ссылается на все фичи.

## 3. Анатомия фичи
```
Features/<Name>/
  Features.<Name>.asmdef
  Configs/        классы ScriptableObject-конфигов
  Data/           модели, enum, буферы
  Services/       интерфейсы и реализации
  Installers/     <Name>Installer.cs + <Name>Installer.prefab
  UI/Panels/  UI/Presenters/
  Editor/         свой asmdef Features.<Name>.Editor
  Content/Configs/  Content/Prefabs/  Content/Textures/   ассеты фичи
  Tests/EditMode/ Features.<Name>.Tests.EditMode.asmdef
```
- Пустые папки не создаём. Namespace = путь (`Features.Inventory.UI.Panels`, `Kernel.Flow.States`).
- Ассеты фичи лежат в её `Content/`, не в общих папках. Сцены: `Assets/Scenes`.

## 4. Рецепты
### Новая фича
1. Папка `Features/<Name>/` внутри `Assets/Game` по анатомии, `Features.<Name>.asmdef` (скопируй соседний, оставь минимум ссылок).
2. Инсталлер (рецепт ниже), код, тесты.
3. Kernel: добавь `Features.<Name>` в `Kernel.asmdef`, если Kernel использует типы фичи.

### Панель + презентер
- Панель: наследник `Panel` в `UI/Panels/`, презентер `IPanelPresenter<TPanel>` в `UI/Presenters/`.
- Регистрация в инсталлере фичи: `Container.BindPanelPresenter<TPanel, TPresenter>();` (`Shared.UI.PanelPresenters`). Презентер создаёт `Kernel.UI.PanelPresenterFactory`. Правок в Kernel не нужно.
- Префаб панели: `Content/Prefabs/`; подключение к `PersistentPanelsPrefabsSourceConfig` (`Assets/Game/Kernel/Content/Configs`) делается только для постоянных панелей.

### Сервис с жизненным циклом
- Реализуй `Shared.Lifecycle.IInitializableService` (`Init()`, вызывается в `BootstrapState`) и/или `IAssetsLoader` (`LoadAssets()` в `LoadingAssetsState`, `ReleaseAssets()` в `CleaningSessionState`).
- Биндинг одной строкой: `Container.Bind(typeof(IService), typeof(IInitializableService), typeof(IAssetsLoader)).To<Service>().AsSingle();`
- Kernel собирает `List<IInitializableService>` и `List<IAssetsLoader>` из контейнера сам.

### Новый конфиг
1. Класс в `Configs/` (ScriptableObject), ассет в `Content/Configs/`.
2. В инсталлере: `[SerializeField] private XxxConfig _xxxConfig;` и `Container.BindInstance(_xxxConfig).AsSingle();`.
3. Назначь ассет в поле префаба инсталлера (в редакторе).

### Новый инсталлер
1. `Installers/<Name>Installer.cs` (`MonoInstaller`, только `[SerializeField]` конфигов + `InstallBindings`).
2. Проще всего создать префаб в Unity-редакторе: пустой GameObject, компонент инсталлера, сохранить как `<Name>Installer.prefab`. Префаб с рукописным `.meta` тоже возможен, но хрупок.
3. Добавь префаб в `_installerPrefabs` в `Assets/Game/Kernel/Resources/ProjectContext.prefab`. Записи ссылаются на fileID компонента MonoInstaller, не GameObject.
4. `ProjectContextTests` падает, если инсталлер фичи не зарегистрирован.

## 5. DI-правила
- Обычные классы (сервисы, презентеры, стейты, фабрики): зависимости только параметрами конструктора в `private readonly` полях. `[Inject]` на полях не используем.
- MonoBehaviour: `[Inject] public void Construct(...)` (сейчас только `Bootstrapper`).
- MonoInstaller: только `[SerializeField]` конфигов и биндинги.
- Биндинг интерфейса: `Container.Bind<IX>().To<X>().AsSingle()`; несколько ролей: `Container.Bind(typeof(A), typeof(B)).To<X>().AsSingle()`.
- Все локальные переменные через `var`.

## 6. Флоу запуска
`Bootstrapper` -> `BootstrapState` -> `SettingPersistentWithoutSaveState` -> `LoadingAssetsState` -> `OpeningSessionState` -> `PlayingSessionState`.

| Состояние | Что делает |
|---|---|
| `BootstrapState` | `IUIPanelsService.Init()`, `Init()` всех `IInitializableService`, затем `SettingPersistentWithoutSaveState` |
| `SettingPersistentWithoutSaveState` | стартовая валюта в `SoftCurrencyWallet` (нет сохранений), затем `LoadingAssetsState` |
| `LoadingSaveState` | заглушка: сразу `LoadingAssetsState` |
| `LoadingAssetsState` | `LoadAssets()` всех `IAssetsLoader` параллельно, затем `OpeningSessionState` |
| `OpeningSessionState` | грузит сцену по Addressables-адресу из `ProjectKernelConfig` (`RuntimeScene`), открывает `GameHubPanel` |
| `PlayingSessionState` | пустое состояние игры |
| `CleaningSessionState` | `ReleaseAssets()` всех `IAssetsLoader` |

Ожидает работы: `BootstrapState` ведёт в `SettingPersistentWithoutSaveState`; когда появится система сохранений, переход должен идти в `LoadingSaveState`.

## 7. Кодстайл
Форматирование и нейминг: `.editorconfig` (Rider). Не выражаемые в нём правила:
- Обработчики событий называем `XxxHandle`.
- Bool и null только явным сравнением: `== true`, `== false`, `!= null`, `== null`. Нельзя `!x`, `if (obj)`, `if (flag)`.
- Без комментариев и закомментированного кода.
- Без сокращений слов (`button`, не `btn`; `index`, не `idx`). Исключения: id, sum, avg, min, max, UI, URL.
- Без защитных null-проверок там, где null невалиден (пусть падает `NullReferenceException`); без `ArgumentNullException` для параметров. Проверка допустима, где null нормален (опциональное значение, `TryGet`); `target?.Do()` допустим.
- Без магических чисел для настраиваемых значений (скорость, длительность, порог): выноси в конфиг/`[SerializeField]`. Константы формул (`0.5f`, `1f`, `2f * Mathf.PI`) оставляем.
- Статические методы только в статических классах; в нестатическом классе не объявляем.
- Пустая строка между смысловыми блоками; без однострочных «схлопываний» ради краткости.
- Приоритет источников стиля: `.codestyle/DECISIONS.md` (если есть), затем соседние файлы, затем чаще встречающийся стиль.

## 8. Тесты
- Unity Test Framework, EditMode. Расположение: `<Сборка>/Tests/EditMode/<Сборка>.Tests.EditMode.asmdef` (скопируй существующий: `includePlatforms: Editor`, `UNITY_INCLUDE_TESTS`, `nunit.framework.dll`). Исключение: `Features/RemoteConfigs/Editor/Tests/`.
- Имена: класс `<Type>Tests`, метод `Method_Condition_ExpectedResult`.
- NSubstitute НЕ используем: не грузится в этой версии Unity (Castle.Core требует `System.Diagnostics.EventLog`). Пиши ручные фейки в папке теста (`FakeAssetsLoader`, `FakeUIPanelsService`).
- Фейковые MonoBehaviour нельзя `AddComponent` из editor-сборки теста: `PanelPresenterFactoryTests` берёт `FormatterServices.GetUninitializedObject`, фабрике нужен только тип.
- ScriptableObject с private `[SerializeField]` заполняем через `SerializedObject` (пример: `Assets/Game/Features/Skins/Tests/EditMode/SkinsGlobalSettingsConfigBuilder.cs`).
- Покрыто: Wallet, Inventory (`InventoryService`, `SelectedSkinsBuffer`), `SkinFactory`, Shared (`IntRange`, математика, коллекции), Kernel (`PanelPresenterFactory`, состояния, `ProjectContext`), `RemoteConfigsData`.
- Пробелы: `SkinsDatabaseService`, `CasesDatabaseService`, презентеры, PlayMode.
- Тесты в `*/Tests/` часть проекта и не удаляются перед коммитом; удаляются только временные проверочные тесты агента.

## 9. Подводные камни
- Переносить файл только вместе с `.meta` (`git mv`); папку: папка + `<папка>.meta`. GUID не терять.
- Никогда не двигай файлы при открытом редакторе (Unity пересоздаёт `.meta`, держит `Temp/`).
- `Editor Remote Configs Data.asset` (папка `Features/RemoteConfigs/Editor/Content`) хранит имена типов (`ClassTypeReference`: `ns`, `asm`, `classRef`). Переименование/перенос типа RemoteConfigs или смена namespace требует правки ассета; ловит `RemoteConfigsDataTests`.
- Сцена грузится по Addressables-адресу `RuntimeScene` (`ProjectKernelConfig`); адрес не менять.
- Записи Addressables для папок текстур ссылаются на GUID папок; при переносе папки сохраняй `.meta`.
- `.sln`, `.csproj`, `.idea` в `.gitignore`; их не правим и не коммитим.
- `GameHubPanel` имеет сериализованные поля с опечатками (`_inventoryButonl`, `_softCurencyView`): переименовывай только с `[FormerlySerializedAs]`.
- `CaseConfig_Case_Test.asset` содержит 4 висячих ссылки на скины (было до рефакторинга).
- Тип не должен называться как сегмент `Features.<Имя>` (класс `Wallet` внутри `Features.Wallet` перекрывается namespace). Снаружи фичи используй `IWallet`/`SoftCurrencyWallet`; при конфликте пиши `Services.Wallet`.
- `Kernel.asmdef` ссылается на все фичи; новая фича без записи в `ProjectContext` не получит биндинги.

## 10. Рабочие правила
- Коммит только после явного подтверждения в чате. Перед коммитом показывай список файлов; спрашивай: «Сделать коммит?».
- Один коммит = одна логически завершённая задача.
- Перед коммитом удали планы, заметки, черновики (`docs/superpowers/**`, `.md` планирования) и временные тесты. Тесты в `*/Tests/` остаются.
- В сообщении коммита нет упоминаний агента, `Co-Authored-By`, «Generated with».
- Файлы, изменённые пользователем до задачи (`ProjectSettings/*`, `Packages/*`, `RenderingSettings/*`), не трогай и не включай в коммит без решения пользователя.
- Unity-редактор должен быть закрыт на время batchmode-команд.

## 11. Команды
Закрой редактор. Задай переменные (`$TMPDIR_AGENT` любая временная папка вне проекта):
```bash
UNITY="/Applications/Unity/Hub/Editor/6000.3.25f1/Unity.app/Contents/MacOS/Unity"
TMPDIR_AGENT="/tmp/opencase"; mkdir -p "$TMPDIR_AGENT"
```
Компиляция (вывод grep должен быть пустым):
```bash
"$UNITY" -batchmode -nographics -quit -projectPath "$PWD" -logFile "$TMPDIR_AGENT/compile.log"; grep -E "error CS|Scripts have compiler errors" "$TMPDIR_AGENT/compile.log"
```
Тесты EditMode (`result="Passed"`, `failed="0"`):
```bash
"$UNITY" -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform EditMode -testResults "$TMPDIR_AGENT/results.xml" -logFile "$TMPDIR_AGENT/tests.log"; grep -o '<test-run[^>]*>' "$TMPDIR_AGENT/results.xml"
```
Один тест: добавь `-testFilter "<полное.имя.Класса>"`. Логи: `$TMPDIR_AGENT/*.log`.
Битые ссылки: `grep -rn "m_Script: {fileID: 0}" Assets/Game Assets/Scenes` -> пусто.
