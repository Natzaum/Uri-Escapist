/// <summary>Limites por andar; o erro que atinge o limite encerra a partida.</summary>
public static class GameDifficulty
{
    public static int BooksRequired(string mode) => mode == "facil" ? 5 : mode == "dificil" ? 9 : 7;
    public static int ErrorLimit(string mode) => mode == "facil" ? 5 : mode == "dificil" ? 1 : 3;
    public static bool PausesReading => MenuPrincipal.GameMode == "facil";
    public static bool CanLeaveBook => MenuPrincipal.GameMode != "dificil";
    public static float EnemySpeedMultiplier => MenuPrincipal.GameMode == "facil" ? .65f : MenuPrincipal.GameMode == "dificil" ? 1.5f : 1f;
    public static float EnemyGrowthMultiplier => MenuPrincipal.GameMode == "facil" ? .25f : MenuPrincipal.GameMode == "dificil" ? 2.5f : 1f;
}
