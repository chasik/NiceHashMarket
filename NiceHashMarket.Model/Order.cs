namespace NiceHashMarket.Model
{
    public class Order
    {
        public Order(int id, decimal price, decimal amount, decimal speed, int workers, int type, int active, ServerEnum server = ServerEnum.Unknown)
        {
            Server = server;

            Id = id;
            Price = price;
            Amount = amount;
            Speed = speed;
            Workers = workers;
            Active = active == 1;

            if (type == 0)
                Type = OrderTypeEnum.Standart;
            else if (type == 1)
                Type = OrderTypeEnum.Fixed;
        }


        public int Id { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal Speed { get; set; }
        public int Workers { get; set; }
        public ServerEnum Server { get; set; }
        public OrderTypeEnum Type { get; set; }
        public bool Active { get; set; }
    }
}
