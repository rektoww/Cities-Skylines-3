Этот файл описывает главное окно приложения для визуализации статичной карты рельефа и ресурсов. Окно построено на WPF и следует MVVM-подходу: 

вся логика/данные приходят из MainViewModel, а XAML отвечает только за отображение и привязки.

Назначение

Окно отображает статичную карту как сетку клеток. Цвет клетки зависит от рельефа (Terrain). При наведении показывается всплывающая подсказка с ресурсами клетки. При клике по клетке открывается диалог с подробной информацией.

Зависимости и контекст данных

Окно ожидает, что его DataContext — экземпляр MainViewModel.

Вью-модель предоставляет:

свойства: CurrentMap, TilesFlat;

команды: LoadStaticCommand, SaveMapCommand, LoadMapCommand, ShowTileInfoCommand.

Обычно DataContext назначается в MainWindow.xaml.cs:

/// <summary>Код-бихайнд окна: установка DataContext.</summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}

Общая структура макета

Макет — DockPanel с двумя зонами:

+------------------------------+--------------------------------------+
| Левая панель (Dock=Left)     | Правая область (карта)               |
| кнопки / размеры / легенда   | ScrollViewer -> ItemsControl         |
+------------------------------+--------------------------------------+


Левая панель: Border + StackPanel (кнопки и информация).

Правая область: ScrollViewer c ItemsControl, панель элементов — UniformGrid.

Левая панель (управление)

Кнопки привязаны к командам VM:

«Загрузить статичную карту» → LoadStaticCommand

«Сохранить в JSON» → SaveMapCommand

«Загрузить из JSON» → LoadMapCommand

Также выводятся габариты карты:

<Run Text="{Binding CurrentMap.Width,  Mode=OneWay}" />
<Run Text="{Binding CurrentMap.Height, Mode=OneWay}" />


Важно: использовать Mode=OneWay, т.к. это read-only свойства.

Легенда объясняет соответствие цветов рельефу (зелёный — равнина, синий — вода, серый — горы).

Правая область (карта)
Плоская коллекция и панель элементов

ItemsControl.ItemsSource="{Binding TilesFlat, Mode=OneWay}"

Панель элементов:

<ItemsPanelTemplate>
  <UniformGrid Columns="{Binding CurrentMap.Width, Mode=OneWay}" />
</ItemsPanelTemplate>


UniformGrid выкладывает элементы слева направо и сверху вниз, создавая визуальную сетку.
Количество колонок равно ширине карты; количество строк определяется автоматически.

Шаблон тайла, подсказка и клик

Каждая клетка — это Button (прозрачная, чтобы клик был по всей площади), внутри — Border 25×25 с цветом фона по Terrain.
В ToolTip выводятся координаты, рельеф и список ресурсов (или «Ресурсы: нет», если список пуст).

Клик вызывает ShowTileInfoCommand и передаёт объект Tile через CommandParameter="{Binding}".

Цвета:

Plain → LightGreen (значение по умолчанию),

Water → LightBlue,

Mountain → DarkGray.

ребования к моделям

GameMap:

Width/Height (read-only).

Tiles — двумерный массив Tile[,], проинициализирован в конструкторе.

Tile:

int X, int Y.

TerrainType Terrain (Plain, Water, Mountain и т.п.).

List<NaturalResource> Resources — инициализирована (например, = new();), не null.

(опц.) Building? Building.

NaturalResource:

ResourceType Type (Iron, Oil, Gas).

int Amount.

Как изменить внешний вид

Размер клетки: меняйте Width="25" Height="25" у Border в ItemTemplate.

Цвета рельефа: правила внутри Style.Triggers (DataTrigger по Terrain).

Подсказка (ToolTip): дополняйте содержимое Button.ToolTip.

Толщина сетки: BorderThickness="0.5" у тайла.