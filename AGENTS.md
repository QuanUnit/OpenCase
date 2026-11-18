# OpenCase: карта проекта для агентов

Unity-игра про открытие кейсов (скины, инвентарь, продажа). Читай этот файл в начале сессии. Работаешь с UI или префабами: читай также `UI_DESIGN.md` (токены, типографика, эффекты, правила).

## 1. Стек
- Unity 6000.3.25f1, URP.
- Zenject (форк QCore), UniTask, Addressables, DOTween, NaughtyAttributes.
- QCore: UI (панели, презентеры), StateMachine, Tools, AddressablesWrapper (пакеты, не трогаем).
- NuGetForUnity: CsvHelper (`Assets/packages.config`). NSubstitute не используем (см. раздел 8).

## 2. Карта сборок
Код живёт в `Game/{Kernel,Shared,Features/*}` внутри `Assets`. Имя asmdef = корневой namespace = путь без `Assets/Game/`.

| Сборка | Путь | Назначение | Зависит от (наши) |
|---|---|---|---|
| `Shared` | `Assets/Game/Shared` | Lifecycle-интерфейсы, `BindPanelPresenter`, SelectableSet, Tools, общие шрифты/текстуры/шаблоны, `Shared.UI.Theme` (`UiTheme`), `Shared.UI.Effects`, `Content/Configs/UiTheme.asset` | нет |
| `Features.Wallet` | `Assets/Game/Features/Wallet` | кошелёк, мягкая валюта | нет |
| `Features.Skins` | `Assets/Game/Features/Skins` | модели скинов, базы, `SkinFactory`, конфиги | Shared |
| `Features.SlotMachine` | `Assets/Game/Features/SlotMachine` | виджет горизонтальной рулетки (без инсталлера) | Skins, Shared |
| `Features.Inventory` | `Assets/Game/Features/Inventory` | инвентарь, продажа, `SelectedSkinsBuffer` | Skins, Wallet, Shared |
| `Features.OthersDrop` | `Assets/Game/Features/OthersDrop` | лента дропа других игроков | Skins, Shared |
| `Features.Clicker` | `Assets/Game/Features/Clicker` | вкладка EARN: клик/удержание монеты даёт мягкую валюту (`ClickerConfig`: награда, интервал удержания, частицы) | Wallet, Shared |
| `Features.Cases` | `Assets/Game/Features/Cases` | кейсы, спин кейса | Skins, Inventory, Wallet, SlotMachine, Shared |
| `Features.GameHub` | `Assets/Game/Features/GameHub` | главная панель-хаб | Cases, Clicker, Inventory, OthersDrop, Wallet, Shared |
| `Kernel` | `Assets/Game/Kernel` | бутстрап, state machine, DI-корень, фабрика презентеров | все фичи, Shared |
| `Features.RemoteConfigs.Editor` | `Assets/Game/Features/RemoteConfigs/Editor` | импорт конфигов из таблиц `Tools/Remotes Configs Importer` (страницы по очереди, иконки не трогает, итоговый отчёт; опция «Download missing icons after import»), иконки скинов `Tools/Skin Icons` (CSGO-API → `<Id>.png` в папке оружия, режимы Only missing / Repair mismatches / Replace all, dry run, существующие файлы не перезаписывает и не удаляет, папки оружия с иконками регистрирует в Addressables (`Default Local Group`, адрес = путь); настройки `Content/SkinIconsConfig.asset`), экспорт скинов в таблицу `Tools/Skins Sheet Exporter` (CSGO-API + цены market.csgo.com, запись через Google Sheets API сервисным аккаунтом; путь к JSON-ключу в EditorPrefs, ключ в ассеты не кладём), редактор кейсов `Tools/Case Editor` (папка `Cases/`: категории `AllCasesConfig`, скины и веса, живая экономика EV/RTP по модели `SkinFactory`, веса по шансам редкостей, цена под целевой RTP, Monte-Carlo, Push/Pull страницы Cases; настройки `Content/CaseEditorConfig.asset`) (только Editor) | Skins, Cases, Clicker, Shared (+ Unity.Addressables.Editor) |

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
- В контейнер биндим только конфиги, сервисы, фабрики и подобные сущности, не данные (модели, состояние выбора и т.п.). Состояние, нужное только пока открыта панель, храним полем в презентере (презентер создаётся заново при каждом открытии).

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
- Общую логику, не привязанную к фиче (математика, коллекции, лейаут), выносим в `Shared/Tools` (тулы, экстеншены), а не во вспомогательные классы фичи.
- UI-раскладка: компонентами Unity Layout; своя логика лейаута в коде только для того, что Unity Layout сделать не может (подробно в `UI_DESIGN.md` §8.1).
- Приоритет источников стиля: `.codestyle/DECISIONS.md` (если есть), затем соседние файлы, затем чаще встречающийся стиль.

## 8. Тесты
- Unity Test Framework, EditMode. Расположение: `<Сборка>/Tests/EditMode/<Сборка>.Tests.EditMode.asmdef` (скопируй существующий: `includePlatforms: Editor`, `UNITY_INCLUDE_TESTS`, `nunit.framework.dll`). Исключение: `Features/RemoteConfigs/Editor/Tests/`.
- Имена: класс `<Type>Tests`, метод `Method_Condition_ExpectedResult`.
- NSubstitute НЕ используем: не грузится в этой версии Unity (Castle.Core требует `System.Diagnostics.EventLog`). Пиши ручные фейки в папке теста (`FakeAssetsLoader`, `FakeUIPanelsService`).
- Фейковые MonoBehaviour нельзя `AddComponent` из editor-сборки теста: `PanelPresenterFactoryTests` берёт `FormatterServices.GetUninitializedObject`, фабрике нужен только тип.
- ScriptableObject с private `[SerializeField]` заполняем через `SerializedObject` (пример: `Assets/Game/Features/Skins/Tests/EditMode/SkinsGlobalSettingsConfigBuilder.cs`).
- Покрыто: Wallet, Inventory (`InventoryService`, `SelectedSkinsBuffer`), `SkinFactory`, Shared (`IntRange`, математика, `MoneyMath`, коллекции, `Base64UrlTools`, `JsonStringTools`, `JsonReader`, `RsaKeyTools`), Clicker (`ClickerService`), Kernel (`PanelPresenterFactory`, состояния, `ProjectContext`), `RemoteConfigsData`, экспорт скинов в таблицу (сборка строк, слияние, CSV, JWT, запросы и порядок вызовов Google Sheets API через фейковый `IHttpSender`), импорт конфигов (`RemoteConfigsImporter`: порядок страниц, ошибки загрузки; неизвестные скины в кейсах), планирование иконок скинов (маппинг id и фаз Doppler, папки оружия, режимы, имена файлов), экономика кейсов (`SkinValueSampler`, `CaseStatsCalculator`, `RarityOddsWeighting`, `CasePriceCalculator`, строки страницы Cases и round-trip через `CasesSheetParser`, guard чужих кейсов и отказ без записи через `CasesSheetUploader` с фейковым `IHttpSender`, сверка аналитики с Monte-Carlo `SkinFactory`).
- Пробелы: `SkinsDatabaseService`, `CasesDatabaseService`, презентеры, PlayMode, IO-часть `SkinIconsRunner` (загрузка, импорт, Addressables), окно и правка ассетов `Tools/Case Editor` (`CaseConfigEditing`).
- Тесты в `*/Tests/` часть проекта и не удаляются перед коммитом; удаляются только временные проверочные тесты агента.

## 9. Подводные камни
- Переносить файл только вместе с `.meta` (`git mv`); папку: папка + `<папка>.meta`. GUID не терять.
- Никогда не двигай файлы при открытом редакторе (Unity пересоздаёт `.meta`, держит `Temp/`).
- `Editor Remote Configs Data.asset` (папка `Features/RemoteConfigs/Editor/Content`) хранит имена типов (`ClassTypeReference`: `ns`, `asm`, `classRef`). Переименование/перенос типа RemoteConfigs или смена namespace требует правки ассета; ловит `RemoteConfigsDataTests`.
- Сцена грузится по Addressables-адресу `RuntimeScene` (`ProjectKernelConfig`); адрес не менять.
- Записи Addressables для папок текстур ссылаются на GUID папок; при переносе папки сохраняй `.meta`.
- `.sln`, `.csproj`, `.idea` в `.gitignore`; их не правим и не коммитим.
- `GameHubPanel` имеет сериализованное поле с опечаткой `_inventoryButonl`: переименовывай только с `[FormerlySerializedAs]`. Баланс показывает `_softCurrencyCounter` (`NumberCounter`).
- `RubikBold/Medium SDF` динамические: после play-сессий в них появляются новые глифы, этот шум не коммить.
- Визуальные значения берём из `UiTheme.asset` (эталон, совпадает с подобранными значениями в `UI_DESIGN.md` §1), правила в `UI_DESIGN.md`. Префабы правим только через `PrefabUtility` (editor открыт).
- Тип не должен называться как сегмент `Features.<Имя>` (класс `Wallet` внутри `Features.Wallet` перекрывается namespace). Снаружи фичи используй `IWallet`/`SoftCurrencyWallet`; при конфликте пиши `Services.Wallet`.
- `Kernel.asmdef` ссылается на все фичи; новая фича без записи в `ProjectContext` не получит биндинги.
- Мультиоткрытие кейсов: диапазон x1–x5 и задержка каскада в `CaseSpinConfig.asset` (`Features/Cases/Content/Configs`), выбранный множитель живёт полем в `CaseSpinSubPanelPresenter`. Сначала создаются все скины, потом одно списание `цена × N`.
- Кейсы: источник правды — страница Cases таблицы (`Id,Name,Price,SkinId,Weight`, запятая как десятичный разделитель). Правки кейсов в `Tools/Case Editor` пушь кнопкой «Push cases to sheet», иначе следующий импорт их откатит. Иконки кейсов лежат в `Cases/Content/Textures` (папка адресуемая целиком).

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

## 12. Unity MCP
- Пакет `com.coplaydev.unity-mcp`. Сервер стартует вместе с редактором: `http://localhost:8080/mcp`. `.mcp.json` в корне репо регистрирует его для Claude Code как `UnityMCP`.
- Нужен `uvx` в PATH (поставлен через pip, симлинк в `/usr/local/bin`).
- Редактор должен быть в фокусе для скриншотов и анимаций: без фокуса player loop не тикает, кадры чёрные.
- Пока редактор открыт, batchmode-команды недоступны (проект залочен): используй MCP `run_tests` и `read_console`. Если `run_tests` не стартует, запускай `TestRunnerApi` через `execute_code`.
- Перед правкой ассетов выходи из play mode.
