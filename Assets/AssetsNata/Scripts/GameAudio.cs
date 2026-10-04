using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Fontes separadas para ambientes, movimento e eventos; volume via AudioListener.</summary>
public sealed class GameAudio : MonoBehaviour
{
    private static GameAudio instance;
    public static GameAudio Instance
    {
        get
        {
            if (instance == null) instance = new GameObject("URI • Áudio").AddComponent<GameAudio>();
            return instance;
        }
    }
    public GameSoundLibrary Library { get; private set; }
    private AudioSource ambience, steps, chase, breathing, events, ui;
    private PlayerMove player;
    private PlayerStamina stamina;
    private PlayerStaminaSimple simpleStamina;
    private EnemyAI enemy;
    private float randomCountdown;
    private bool worldPaused;
    private bool simpleExhausted;

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        Library = Resources.Load<GameSoundLibrary>("GameSoundLibrary");
        ambience = Source(true,.4f); steps = Source(true,.45f); chase = Source(true,.55f);
        breathing = Source(true,.55f); events = Source(false,.35f); ui = Source(false,.75f);
        SceneManager.sceneLoaded += SceneLoaded;
        RefreshScene();
    }
    private AudioSource Source(bool loop, float volume)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false; source.loop = loop; source.volume = volume;
        source.spatialBlend = 0; return source;
    }
    private void SceneLoaded(Scene scene, LoadSceneMode mode) => RefreshScene();
    private void RefreshScene()
    {
        foreach (var source in new[]{ambience,steps,chase,breathing,events}) source.Stop();
        ui.Stop();
        worldPaused = false; simpleExhausted = false;
        player = FindFirstObjectByType<PlayerMove>(); stamina = FindFirstObjectByType<PlayerStamina>();
        simpleStamina = FindFirstObjectByType<PlayerStaminaSimple>(); enemy = FindFirstObjectByType<EnemyAI>();
        randomCountdown = Random.Range(25f,45f);
        if (Library == null) return;
        string scene = SceneManager.GetActiveScene().name;
        ambience.clip = scene == "UriMenu" ? Library.som_menus
            : scene == "MainScene" ? Library.som_ambiente_entrada
            : scene.StartsWith("andar") ? Library.som_andares : null;
        steps.clip = Library.som_passos; chase.clip = Library.som_perseguicao; breathing.clip = Library.som_exausto;
        if (ambience.clip != null) ambience.Play();
    }
    public void Click() { if (Library != null) ui.PlayOneShot(Library.som_clique); }
    public void Answer(bool correct)
    {
        if (Library != null) ui.PlayOneShot(correct ? Library.som_clique_correto : Library.som_clique_errado);
    }
    public void Death() { if (Library != null) ui.PlayOneShot(Library.som_morte); }
    private void Update()
    {
        if (Library == null) return;
        bool menu = SceneManager.GetActiveScene().name == "UriMenu";
        bool paused = !menu && GameInterface.WorldPaused;
        bool terminal = GameInterface.Instance.IsTerminal;
        if (terminal)
        {
            foreach (var source in new[]{ambience,steps,chase,breathing,events}) source.Stop();
            return;
        }
        if (paused != worldPaused)
        {
            foreach (var source in new[]{ambience,steps,chase,breathing,events})
                if (paused) source.Pause(); else source.UnPause();
            worldPaused = paused;
        }
        if (paused || menu) return;
        bool running = stamina != null ? stamina.IsSprinting() : simpleStamina != null && simpleStamina.IsSprinting();
        steps.pitch = running ? 1.35f : 1f;
        Loop(steps, player != null && player.IsWalkingOnGround && !GameInterface.BlocksInput);
        Loop(chase, enemy != null && enemy.isActiveAndEnabled && enemy.IsChasing);
        if (simpleStamina != null)
        {
            if (simpleStamina.GetStaminaPercent() <= 0) simpleExhausted = true;
            else if (simpleStamina.GetStaminaPercent() >= 1) simpleExhausted = false;
        }
        Loop(breathing, stamina != null ? stamina.IsExhausted : simpleExhausted);
        ambience.volume = chase.isPlaying ? .18f : .4f;
        if (SceneManager.GetActiveScene().name.StartsWith("andar"))
        {
            randomCountdown -= Time.deltaTime;
            if (randomCountdown <= 0)
            {
                if (!events.isPlaying && Library.som_aleatorio != null) events.PlayOneShot(Library.som_aleatorio);
                randomCountdown = Random.Range(25f,45f);
            }
        }
    }
    private static void Loop(AudioSource source, bool active)
    {
        if (active && source.clip != null) { if (!source.isPlaying) source.Play(); }
        else if (source.isPlaying) source.Stop();
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        if (instance == this) instance = null;
    }
}
