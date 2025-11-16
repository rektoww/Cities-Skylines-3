using Core.Interfaces;
using Core.Models.Map;
using Core.Resourses;


namespace Core.Services
{
    public class EconomyService : IGameService
    {
        private readonly GameMap _map;
        private readonly FinancialSystem _financial;
        private readonly PlayerResources _playerResources;

        public EconomyService(GameMap map, FinancialSystem financialSystem, PlayerResources playerResources)
        {
            _map = map;
            _financial = financialSystem;
            _playerResources = playerResources;
        }

        public void Initialize()
        {
        }

        public void Update()
        {
        }

    }
}