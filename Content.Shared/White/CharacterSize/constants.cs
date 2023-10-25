namespace Content.Shared.White.CharacterSize;

public static class SizeConstants
{
    public static float MinSize = 0.8f;
    public static int Default = 50;
    public static float MaxSize = 1.2f;

    public static float GetSize(int size)
    {
        return MinSize + (MaxSize - MinSize) * size / 100;
    }
}
