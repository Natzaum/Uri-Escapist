/// <summary>Limites por andar; o erro que atinge o limite encerra a partida.</summary>
public static class GameDifficulty
{
    public static int BooksRequired(string mode) => mode == "facil" ? 5 : mode == "dificil" ? 9 : 7;
    public static int ErrorLimit(string mode) => mode == "facil" ? 5 : mode == "dificil" ? 1 : 3;
}
