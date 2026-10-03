using System;
using System.Collections;
using System.Collections.Generic;
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
        public string difficulty;
    }

    [Serializable]
    private class ApiMeta
    {
        public string gameMode;
        public string scene;
        public string[] difficulties;
    }

    [Serializable]
    private class ApiResponse
    {
        public bool success;
        public ApiQuestion[] data;
        public string message;
        public ApiMeta meta;
    }

    private static readonly HashSet<int> usedQuestionIds = new HashSet<int>();
    private static readonly Dictionary<string, ApiResponse> floorQuestions = new Dictionary<string, ApiResponse>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetSession()
    {
        usedQuestionIds.Clear();
        floorQuestions.Clear();
    }

    public static IEnumerator LoadAndAssign(string apiUrl, string sceneName, string mode,
        int timeoutSeconds, BookQuiz[] books, Action<string> onCompleted, Action<float> onProgress = null)
    {
        string cacheKey = mode + ":" + sceneName;
        if (floorQuestions.TryGetValue(cacheKey, out var cached))
        {
            string error = Assign(cached, books);
            if (error == null) onProgress?.Invoke(1f);
            onCompleted(error);
            yield break;
        }
        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            onCompleted("Configure a URL da API de perguntas no BookManager.");
            yield break;
        }
        string requestUrl = apiUrl + (apiUrl.Contains("?") ? "&" : "?")
            + "scene=" + UnityWebRequest.EscapeURL(sceneName)
            + "&mode=" + UnityWebRequest.EscapeURL(mode)
            + "&limit=" + books.Length + "&random=1"
            + "&exclude=" + UnityWebRequest.EscapeURL(string.Join(",", usedQuestionIds));

        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.timeout = Mathf.Clamp(timeoutSeconds, 2, 30);
            request.SetRequestHeader("Accept", "application/json");
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                // Sem tamanho conhecido não inventamos uma porcentagem.
                onProgress?.Invoke(request.downloadProgress > 0 ? request.downloadProgress * 0.85f : -1f);
                yield return null;
            }
            onProgress?.Invoke(0.9f);
            ApiResponse response = null;
            try { response = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text); }
            catch (Exception) { /* Network/proxy failures may return non-JSON bodies. */ }

            if (request.result != UnityWebRequest.Result.Success || response == null || !response.success)
            {
                onCompleted(response != null && !string.IsNullOrWhiteSpace(response.message)
                    ? response.message : "Não foi possível carregar as perguntas. Verifique a conexão com o servidor.");
                yield break;
            }
            // Reject old servers and partial batches before touching any book.
            if (response.meta == null || response.meta.gameMode != mode || response.meta.scene != sceneName
                || response.meta.difficulties == null || response.meta.difficulties.Length == 0
                || response.data == null || response.data.Length != books.Length)
            {
                onCompleted("A API não retornou uma partida completa para este modo. Atualize o servidor e confira as perguntas publicadas.");
                yield break;
            }
            var ids = new HashSet<int>();
            foreach (ApiQuestion question in response.data)
            {
                if (question == null || question.id <= 0 || !ids.Add(question.id) || usedQuestionIds.Contains(question.id)
                    || string.IsNullOrWhiteSpace(question.prompt) || question.options == null
                    || question.options.Length != 4 || Array.Exists(question.options, string.IsNullOrWhiteSpace)
                    || question.correctIndex < 0 || question.correctIndex > 3
                    || Array.IndexOf(response.meta.difficulties, question.difficulty) < 0)
                {
                    onCompleted("A API retornou perguntas inválidas ou já usadas nesta partida. Atualize o servidor e confira o banco de perguntas.");
                    yield break;
                }
            }
            string assignmentError = Assign(response, books);
            if (assignmentError != null) { onCompleted(assignmentError); yield break; }
            // Reserve todos os livros do andar, inclusive os que não foram respondidos.
            foreach (int id in ids) usedQuestionIds.Add(id);
            floorQuestions[cacheKey] = response;
            onProgress?.Invoke(1f);
            onCompleted(null);
        }
    }

    private static string Assign(ApiResponse response, BookQuiz[] books)
    {
        if (response.data.Length != books.Length || Array.Exists(books, book => book == null))
            return "A quantidade de livros deste andar mudou. Inicie uma nova partida.";
        BookQuiz[] shuffledBooks = (BookQuiz[])books.Clone();
        for (int i = shuffledBooks.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            BookQuiz temp = shuffledBooks[i]; shuffledBooks[i] = shuffledBooks[j]; shuffledBooks[j] = temp;
        }
        for (int i = 0; i < shuffledBooks.Length; i++)
        {
            ApiQuestion question = response.data[i];
            if (!shuffledBooks[i].SetQuestionData(question.id, question.prompt, question.options, question.correctIndex))
                return "Não foi possível preencher todos os livros. Tente novamente.";
        }
        return null;
    }
}
