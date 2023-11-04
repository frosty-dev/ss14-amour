namespace Content.Shared.White.CharacterSize;

public static class SizeConstants
{
    public static int Default = 50;

    public static float GetSize(int size,float min,float max)
    {
        return min + (max - min) * size / 100;
    }

    public static int GetSizePersent(float size, float min, float max)
    {
        return (int)((size - min) / (max - min) * 100);
    }
}
