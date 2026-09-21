public static class GameState
{
    private static bool isCompleteEP1 = false;


    public static void toggleIsCompleteEP1()
    {
        isCompleteEP1 = !isCompleteEP1;
    }
    
    public static bool getIsCompleteEP1()
    {
        return isCompleteEP1;
    }
    
}