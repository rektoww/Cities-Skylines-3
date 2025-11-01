namespace Core.Models.Base
{
    public abstract class GameObject
    {
        private static int _nextId = 1;
        public int Id { get; }
        public string Name { get; set; } = string.Empty;
        
        // TODO: Думаю стоит поменять логику
        // Сделать приватными сеттеры и добавить методы для задавания координат
        public int X { get; set; } 
        public int Y { get; set; }
        
        public bool IsActive { get; set; } = true;

        public GameObject()
        {
            Id = _nextId++;
        }
        public (int X, int Y) Position
        {
            get => (X, Y);
            set { X = value.X; Y = value.Y; }
        }
    }
}