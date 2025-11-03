MainViewModel.cs — документация

MainViewModel приложения для лабораторной работы по карте. Отвечает за загрузку фиксированной карты, сохранение/загрузку в JSON, 

предоставление данных для визуализации в XAML и показ информации о клетке.

Назначение и роль в MVVM:

MainViewModel — «тонкий» координатор между слоем данных (модели GameMap, Tile) и представлением (MainWindow.xaml).
Здесь нет отрисовки — только данные и команды:

предоставляет плоскую коллекцию тайлов для ItemsControl;

управляет состоянием текущей карты;

выполняет команды: загрузить статичную карту, сохранить/загрузить JSON, показать инфо о клетке.

Зависимости:

CommunityToolkit.Mvvm: ObservableObject, RelayCommand для нотификаций и команд.

Core.Models.Map: GameMap, Tile.

Infrastructure.Services:

StaticMapProvider — конструирует фиксированную карту по маскам;

SaveLoadService — сериализация/десериализация карты в JSON.

Жизненный цикл и поток данных:

В конструкторе VM создаётся SaveLoadService.

Сразу вызывается LoadStatic() → CurrentMap = StaticMapProvider.Build().

Сеттер CurrentMap делает OnPropertyChanged(nameof(TilesFlat)), чтобы UI перерисовал сетку.

XAML (ItemsControl) читает TilesFlat и рендерит сетку по UniformGrid.Columns = CurrentMap.Width.

Кнопки слева вызывают команды LoadStaticCommand, SaveMapCommand, LoadMapCommand.

Клик по клетке вызывает ShowTileInfoCommand(tile) — показ инфо в MessageBox.

Свойства:

GameMap CurrentMap
Текущая карта. В сеттере обязательно вызывается OnPropertyChanged(nameof(TilesFlat)), чтобы перерисовать сетку после смены карты.

IEnumerable<Tile> TilesFlat
Плоское перечисление тайлов для ItemsControl. Порядок — построчно: y от 0..Height-1, внутри строки x от 0..Width-1.
Это соответствует раскладке UniformGrid (слева→направо, сверху→вниз).

Команды:

LoadStaticCommand (LoadStatic())
Заполняет CurrentMap фиксированной картой из StaticMapProvider.

SaveMapCommand (SaveMap())
Сохраняет карту в saved_map.json (рядом с exe).

LoadMapCommand (LoadMap())
Загружает карту из saved_map.json, обновляет CurrentMap и тем самым сетку.

ShowTileInfoCommand (ShowTileInfo(Tile tile))
Формирует человекочитаемую сводку по выбранной клетке (координаты, рельеф, ресурсы, при наличии — здание) и показывает через MessageBox.