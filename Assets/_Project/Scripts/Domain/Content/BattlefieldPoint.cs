namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public readonly struct BattlefieldPoint
    {
        public BattlefieldPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }

        public float Y { get; }
    }
}
