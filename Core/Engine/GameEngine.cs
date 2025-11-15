using Core.Enums;
using Core.Interfaces;
using Core.Models.Base;
using Core.Models.Map;
using Core.Models.Mobs;
using Core.Resourses;
using Core.Services;


namespace Core.GameEngine
{
    public class GameEngine
    {
        private readonly List<IGameService> _services = new();
        private readonly GameMap _map;

        // Сервисы
        public EconomyService EconomyService { get; private set; }
        public PopulationService PopulationService { get; private set; }
        public TransportService TransportService { get; private set; }
        public PoliceService PoliceService { get; private set; }
        public ProductionService ProductionService { get; private set; }

        // Доп. сервисы (подключены и доступны из движка)
        public UtilityManager UtilityManager { get; private set; }
        public NatureManager NatureManager { get; private set; }
        public DisasterManager DisasterManager { get; private set; }
        public HRManager HRManager { get; private set; }

        // Системы
        public FinancialSystem FinancialSystem { get; private set; }
        public PlayerResources PlayerResources { get; private set; }
        public ConstructionCompany ConstructionCompany { get; private set; }
        public ExternalConnectionsManager ExternalConnections { get; private set; }
        public MarketService MarketService { get; private set; }
        public ResourceProductionService ResourceProductionService { get; private set; }

        public GameEngine(GameMap map)
        {
            _map = map;
            InitializeSystems();
            InitializeServices();
        }

        private void InitializeSystems()
        {
            FinancialSystem = new FinancialSystem(initialBudget: 1_000_000m);
            PlayerResources = new PlayerResources(1_000_000m, new Dictionary<ConstructionMaterial, int>()
            {
                { ConstructionMaterial.Steel, 500 },
                { ConstructionMaterial.Glass, 300 },
                { ConstructionMaterial.Concrete, 800 },
                { ConstructionMaterial.Plastic, 400 },
                { ConstructionMaterial.Wood, 600 }
            });

            ExternalConnections = new ExternalConnectionsManager(PlayerResources, FinancialSystem);
            ConstructionCompany = new ConstructionCompany(PlayerResources, FinancialSystem);
            MarketService = new MarketService(ExternalConnections);
            ResourceProductionService = new ResourceProductionService(PlayerResources, ExternalConnections);

            UtilityManager = new UtilityManager();
            NatureManager = new NatureManager();

            DisasterManager = new DisasterManager(_map, UtilityManager);

            HRManager = new HRManager();
        }

        private void InitializeServices()
        {
            EconomyService = new EconomyService(_map, FinancialSystem, PlayerResources);
            PopulationService = new PopulationService(_map);
            TransportService = new TransportService(_map, new PathfindingService(_map));
            PoliceService = new PoliceService(_map);
            ProductionService = new ProductionService(_map);

            _services.Add(EconomyService);
            _services.Add(PopulationService);
            _services.Add(TransportService);
            _services.Add(PoliceService);
            _services.Add(ProductionService);
            _services.Add(DisasterManager);
        }

        public void Update()
        {
            foreach (var service in _services)
            {
                service.Update();
            }
        }

        public void AddVehicle(Transport vehicle)
        {
            _map.Vehicles.Add(vehicle);
        }

        public void AddCitizen(Citizen citizen)
        {
            PopulationService.AddCitizen(citizen);
        }

        public bool TryPlaceBuilding(Building building, int x, int y)
        {
            return ConstructionCompany.TryBuild(building, x, y, _map);
        }
    }
}