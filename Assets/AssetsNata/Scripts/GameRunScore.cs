using System.Collections.Generic;

/// <summary>Pontuação dos andares concluídos, preservada entre cenas da partida.</summary>
public static class GameRunScore
{
    private static readonly Dictionary<string, (int correct, int errors)> completed =
        new Dictionary<string, (int correct, int errors)>();

    public static void CompleteFloor(string scene, int correct, int errors)
    {
        // Voltar a um andar concluído não substitui nem duplica sua pontuação.
        if (!completed.ContainsKey(scene)) completed.Add(scene, (correct, errors));
    }

    public static (int correct, int errors) GetTotal(string currentScene, int currentCorrect, int currentErrors)
    {
        int correct = 0, errors = 0;
        foreach (var score in completed.Values)
        {
            correct += score.correct;
            errors += score.errors;
        }
        // Na derrota, inclui a tentativa atual; reiniciar descarta só essa tentativa.
        if (!completed.ContainsKey(currentScene))
        {
            correct += currentCorrect;
            errors += currentErrors;
        }
        return (correct, errors);
    }

    public static void Reset() => completed.Clear();
}
