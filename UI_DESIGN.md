# UI_DESIGN: правила дизайна OpenCase

Тёмный минималистичный UI, один тёплый акцент. Эталон токенов: `Assets/Game/Shared/Content/Configs/UiTheme.asset` (класс `Shared.UI.Theme.UiTheme`). Цвета, шрифты, спрайты, иконки и тайминги берём из него, в префабах не придумываем свои. Экран 1920x1080 (CanvasScaler ScaleWithScreenSize, match 0.5).

## 1. Токены
| Токен | Hex | Применение |
|---|---|---|
| Background | #0B0D12 | фон экрана |
| Surface | #151821 | карточки, панели, верхняя панель |
| SurfaceRaised | #1D212C | hover, пилюли, вложенные блоки |
| Stroke | #2A2F3D | обводки 1-2px, разделители |
| TextPrimary | #F2F4F8 | основной текст |
| TextMuted | #8A93A6 | вторичный текст, неактивные табы |
| Accent | #FF8A1F | CTA, активный таб, цены, фокус |
| AccentLight | #FFB547 | верх градиента, hover акцента |
| Success | #3DDC84 | продажа, рост |
| Danger | #FF4D5E | падение, ошибки |
| Overlay | #000000 @ 0.85 | подложка попапа (тюнено под linear) |

Цвета редкости живут в `SkinsUIContextDataConfig` (геймплейные данные). В UI их используем только как: нижняя 4px полоса (слева для drop-плитки), мягкое свечение за иконкой, обводка победного слота. Текст цветом редкости не красим.

Проект в Linear color space: бледные альфы выглядят в 2-3 раза сильнее, чем в sRGB. Слабые значения подбирай на глаз по скриншоту. Подобранные значения: подсветка победы/экрана `ScreenFlash._peakAlpha` 0.025; backdrop попапа 0.85 (чёрный в linear выглядит светлее); свечение редкости 0.3 (drop-плитка 0.2); свечение иконки кейса Accent 0.03; пульс CTA 0.08-0.2; свечение маркера 0.22; пульс победного слота 0.35-0.75. Чёрный/белый с альфой тоже "плывёт": проверяй глазами.

## 2. Типографика
Шрифт Rubik (SIL OFL 1.1, latin + cyrillic): `Shared/Content/Fonts/Rubik/` (TTF, `OFL.txt`, `SOURCE.md`, TMP dynamic `RubikMedium SDF` / `RubikBold SDF`). Старые DIN и LiberationSans в UI не использовать.
| Роль | Шрифт | Размер | Прочее |
|---|---|---|---|
| H1 | Bold | 44 | UPPERCASE, spacing 2 |
| H2 | Bold | 30 | UPPERCASE |
| Body | Medium | 22 | |
| Caption | Medium | 16 | подписи, UPPERCASE с spacing 2 для лейблов |
| Number / цена | Bold | 24 | Accent |

## 3. Формы и спрайты
`Shared/Content/Textures/UI/Generated/` (белые, красятся через `Image.color`, Sliced): RoundedRect12 (кнопки, плитки), RoundedRect16 (карточки), RoundedRect24 (попапы), Pill (полностью скруглённый; для высоты >64 уменьши `pixelsPerUnitMultiplier`), RoundedOutline16 (2px обводка), SoftGlow (радиальный), VerticalGradient (белый сверху, прозрачный снизу), Circle.
Сетка 8px, отступы 16/24/32. Теней нет, кроме SoftGlow. Карточка: Surface + Outline Stroke, при hover обводка до Accent 60%.

## 4. Иконки и лицензии
`Shared/Content/Textures/UI/Icons/`: `icon_coin`, `icon_box`, `icon_grid`, `icon_close`, `icon_check`, `icon_tap` (128px, белые на прозрачном, генерируются процедурно, лицензия проекта), `icon_case` (старый ассет). Реестр: `Icons/LICENSES.md`. Сторонние ассеты только с лицензией OFL / CC0 / MIT / ISC / Apache-2.0: файл лицензии рядом, источник записать в `LICENSES.md`/`SOURCE.md`.

## 5. Компоненты и шаблоны
`Shared/Content/Prefabs/Templates/`:
- `Button`: основная, 240x56, Accent -> AccentLight при hover, иконка и подпись цвета Background.
- `ButtonGhost`: вторичная, RoundedOutline16 Stroke -> Accent 60%, текст TextPrimary.
- `Currency`: цена, Rubik Bold 24 Accent (`TextedCurrencyView`, форматирует `NumberCounterFormatter.FormatMoney`: "1234.50 $", InvariantCulture).

Единый паттерн кнопки, карточки и таба: на корне лежат `Button` (transition None), прозрачный raycast `Image` (alpha 0, raycastTarget включён) и `ButtonFeedback`. Вся графика в дочернем `Visual`; он же `Button.targetGraphic` (его `Image` с raycastTarget выключен) и `ButtonFeedback._target` (`_colorTarget` = Visual или вложенный Outline). Масштаб при hover/press меняет только `Visual`, а зона клика (корень) не сжимается, поэтому нет дребезга на краю. Корень остаётся для layout, `SetActive` и `StaggeredAppear`. Ссылки презентеров (`GameHubPanel._casesButton`, `CaseView._openButton` и т.п.) указывают на `Button` корня.

## 6. Эффекты (`Shared.UI.Effects`, DOTween)
| Компонент | Назначение и куда вешать |
|---|---|
| `ButtonFeedback` | hover 1.03 / press 0.96, лерп цвета; на корень каждой Button, `_target` = Visual |
| `PanelAppear` | fade + сдвиг по Y 24 -> 0 за 0.25s при OnEnable; на дочерний Content панели, не на элемент под LayoutGroup (OnValidate предупредит) |
| `StaggeredAppear` | каскад появления детей (шаг 0.03s); на контейнер сетки/списка, новые дети подхватываются сами |
| `PopupAppear` | backdrop fade + scale карточки 0.9 -> 1 OutBack; на корень попапа |
| `PulseGlow` | пинг-понг альфы SoftGlow; CTA, live-точка, победный слот |
| `ShineSweep` | диагональный блик по CTA раз в 3s; `_area` с Mask |
| `TabIndicator` (+`TabIndicatorItem`) | подчёркивание едет к активному табу, цвета текста Primary/Muted; панель хаба |
| `NumberCounter` | число тянется к новому значению 0.4s, вспышка Success/Danger; баланс |
| `MoneyBurst` (+`MoneyBurstVariant`) | пул Image-частиц (монеты/купюры из `Textures/UI/Particles`): разлёт, гравитация, вращение, затухание; награда кликера |
| `ScalePunch` | пунш масштаба 1.12; победный слот |
| `ScreenFlash` | вспышка цветом редкости на весь экран; на последний ребёнок панели спина |

Две конвенции для каждого эффекта:
1. Твины с `SetUpdate(true)` и `SetLink(gameObject)`; в `OnDisable` они убиваются и объект возвращается в итоговое состояние (alpha 1, rest scale).
2. Сериализованные ссылки назначаются до активации объекта (Awake/OnEnable без null-проверок упадут). Значения тайминга и цвета лежат на компоненте, не в коде; не менять тайминги, на которые рассчитывают презентеры.

## 7. Экраны
- GameHub: верхняя панель 96px (Surface + 1px Stroke): логотип + "OPEN CASE" (CASE акцентом), табы CASES/INVENTORY (иконка + Caption, TabIndicator), справа пилюля баланса и профиль. Слева лента OthersDrop 240px (Surface, "LIVE DROPS" с пульсирующей точкой). Остальное: Background.
- Кейсы: H1 категории с 4px акцентной полосой; сетка CaseView 260x300, gap 24: иконка + свечение, название, пилюля цены.
- Спин: Back (ghost) слева, "OPENING CASE" + H1 название, лента на всю ширину (плитки 240x232, gap 24, маркер Accent со свечением, затухание по краям 240px), CTA "OPEN . цена" 400x76 (ShineSweep + PulseGlow). Победа: пунш слота, обводка и свечение редкости, подпись "YOU GOT", вспышка.
- Инвентарь: H1 + счётчик и сумма, кнопки выбора справа (пилюли 44px), SELL (Success), сетка SkinView 200x220; выбранный: Accent 2px обводка + Check-бейдж; пустое состояние "NO SKINS YET".
- Попап скина: RoundedRect24 720 шириной, иконка с свечением, H2 имя, цена Accent, SELL (Success) / CLOSE (ghost).

## 8. Do / Don't
- Do: токены из `UiTheme`; Rubik; спрайты из `Generated`; `Visual`-паттерн; PanelAppear на Content; проверка скриншотом.
- Don't: свои цвета и шрифты в префабах; тени; цвет редкости в тексте; PanelAppear на элементе внутри LayoutGroup; магические числа в эффектах; правка YAML префабов при открытом редакторе (только `PrefabUtility`).

## 8.1. Лейаут
- Раскладку делаем компонентами Unity Layout: `Horizontal/Vertical/GridLayoutGroup`, `LayoutElement` (min/preferred/flexible), `ContentSizeFitter`, `AspectRatioFitter`, якоря и `RectTransform`.
- Свою логику лейаута в коде (расчёт позиций, размеров, `anchoredPosition` в цикле) пишем, только если нужное поведение или его часть Unity Layout сделать не может. Пример: равномерный `localScale` содержимого, который layout-группы игнорируют. Тогда кастомной остаётся только эта часть, в виде маленького переиспользуемого компонента в `Shared.UI`, остальное по-прежнему на Unity Layout.

## 9. Как проверять (Unity MCP)
1. Play из `BootstrappScene`, пройти экран через `execute_code` (найти `Button`, вызвать `onClick.Invoke()`).
2. Скриншот: `manage_camera` action `screenshot` (файл в `Assets/Screenshots/`, папку удалить после). Редактор должен быть в фокусе, иначе кадр чёрный или анимации стоят.
3. Без фокуса: `EditorApplication.isPaused = true` + `EditorApplication.Step()` по кадрам, анимации прыжком `TweenExtensions.Goto`, снимок через временную камеру и RenderTexture.
4. `read_console` без ошибок; EditMode через `run_tests` (см. AGENTS.md).
