using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class RemoteQuestionLoader
{
    [Serializable]
    private class ApiQuestion
    {
        public int id;
        public string prompt;
        public string[] options;
        public int correctIndex;
    }

    [Serializable]
    private class ApiResponse
    {
        public bool success;
        public ApiQuestion[] data;
        public string message;
    }

    public static IEnumerator LoadAndAssign(
        string apiUrl,
        string sceneName,
        int timeoutSeconds,
        BookQuiz[] books,
        Action<int> onCompleted = null)
    {
        if (books == null || books.Length == 0)
        {
            onCompleted?.Invoke(0);
            yield break;
        }

        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            Debug.LogWarning("URL da API de perguntas nao configurada. Usando perguntas locais.");
            onCompleted?.Invoke(0);
            yield break;
        }

        string scene = string.IsNullOrWhiteSpace(sceneName) ? "unknown" : sceneName.Trim();
        string separator = apiUrl.Contains("?") ? "&" : "?";
        string requestUrl = apiUrl
            + separator
            + "scene=" + UnityWebRequest.EscapeURL(scene)
            + "&limit=" + books.Length
            + "&random=1";

        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.timeout = Mathf.Clamp(timeoutSeconds, 2, 30);
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(
                    $"Nao foi possivel buscar perguntas online ({request.responseCode}: {request.error}). "
                    + "Os livros continuarao usando as perguntas locais."
                );
                onCompleted?.Invoke(0);
                yield break;
            }

            ApiResponse response;

            try
            {
                response = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Resposta invalida da API de perguntas: {exception.Message}. Usando perguntas locais.");
                onCompleted?.Invoke(0);
                yield break;
            }

            if (response == null || !response.success || response.data == null || response.data.Length == 0)
            {
                string detail = response != null && !string.IsNullOrWhiteSpace(response.message)
                    ? " Detalhe: " + response.message
                    : string.Empty;
                Debug.LogWarning($"A API nao retornou perguntas publicadas para a cena '{scene}'.{detail} Usando perguntas locais.");
                onCompleted?.Invoke(0);
                yield break;
            }

            int assignedCount = 0;
            int assignmentLimit = Mathf.Min(books.Length, response.data.Length);

            for (int index = 0; index < assignmentLimit; index++)
            {
                BookQuiz book = books[index];
                ApiQuestion question = response.data[index];

                if (book != null && question != null &&
                    book.SetQuestionData(question.id, question.prompt, question.options, question.correctIndex))
                {
                    assignedCount++;
                }
            }

            onCompleted?.Invoke(assignedCount);
        }
    }
}
