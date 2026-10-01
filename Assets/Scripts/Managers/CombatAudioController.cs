using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityUtils;

/// <summary>
///     Audio de combate y ambiente con el AudioManager (SoundManager/SoundData) y AudioMixers.
///     Es persistente entre escenas para que el ambiente suene todo el rato.
///     - Ambiente (AmbienteCatacumbas_1.1.wav): loop siempre. Al entrar en combate
///     (animación de transición) se apaga con fade y entra lo de combate.
///     - Música de combate (MusicaCCombate): loop siempre en combate; con poca vida
///     se le aplica lowpass (snapshots del mixer + filtro en el source de respaldo).
///     1. Latidos normales: siempre en combate, excepto con poca vida.
///     2. Latidos rápidos: solo en combate Y con poca vida (<= lowHealthThreshold).
///     Su mixer ("Latidos Rápidos") ya trae Lowpass para el efecto "a punto de morir".
///     SETUP EN INSPECTOR:
///     - ambientSound: clip = AmbienteCatacumbas_1.1.wav, mixerGroup = grupo del mixer
///     "Musica de fondo" (o ninguno), loop = true.
///     - combatMusic: clip = MusicaCCombate.wav, mixerGroup = grupo del mixer
///     "Musica de combate", loop = true.
///     - heartbeatNormal: clip = LatidosNormales.wav, mixerGroup = grupo del mixer
///     "Latidos Normales", loop = true.
///     - heartbeatFast: clip = LatidosRapidos.wav, mixerGroup = grupo del mixer
///     "Latidos Rápidos", loop = true.
///     - combatMusicMixer + snapshots Normal/PocaVida del mixer "Musica de combate".
/// </summary>
public class CombatAudioController : Singleton<CombatAudioController>
{
    [Header("Ambiente (loop siempre, fade out al entrar en combate)")]
    [Tooltip("AmbienteCatacumbas_1.1.wav con el grupo del mixer 'Musica de fondo' (o ninguno)")]
    public SoundData ambientSound;

    public float ambientVolume = 1f;

    [Header("Sonidos de combate (SoundData: clip + mixerGroup, se fuerzan a loop)")]
    [Tooltip("MusicaCCombate.wav con el grupo del mixer 'Musica de combate'")]
    public SoundData combatMusic;

    [Tooltip("LatidosNormales.wav con el grupo del mixer 'Latidos Normales'")]
    public SoundData heartbeatNormal;

    [Tooltip("LatidosRapidos.wav con el grupo del mixer 'Latidos Rápidos' (ese mixer ya trae Lowpass)")]
    public SoundData heartbeatFast;

    public float combatMusicVolume = 1f;
    public float heartbeatVolume = 1f;

    [Header("Transiciones")] [Tooltip("Duración del fade del ambiente al entrar/salir de combate")]
    public float transitionFadeDuration = 1.5f;

    [Tooltip("Duración del fade-in de la música al entrar en combate")]
    public float combatFadeInDuration = 1.5f;

    [Tooltip("Duración del fade-out de lo de combate al salir")]
    public float combatFadeOutDuration = 0.8f;

    [Header("Reglas de vida")] [Tooltip("Con esta vida o menos entran latidos rápidos + lowpass en la música")]
    public int lowHealthThreshold = 1;

    [Header("Lowpass 'a punto de morir' en la música (vía mixer)")]
    [Tooltip("AudioMixer 'Musica de combate'. Si es null, el lowpass se hace solo con el filtro del source.")]
    public AudioMixer combatMusicMixer;

    [Tooltip("Nombre del parámetro expuesto del cutoff en el mixer (si lo expones)")]
    public string combatMusicCutoffParam = "MusicaCombateLowpass";

    [Tooltip("Snapshots Normal/PocaVida del mixer 'Musica de combate' (opcional pero recomendado)")]
    public AudioMixerSnapshot normalSnapshot;

    public AudioMixerSnapshot lowHealthSnapshot;
    public float snapshotTransitionTime = 1f;

    [Header("Lowpass de respaldo (filtro en el source, siempre funciona)")]
    [Tooltip("Cutoff sano = sin filtrar (el lowpass deja pasar todo)")]
    public float healthyCutoff = 22000f;

    [Tooltip("Cutoff moribundo = suena apagado/ahogado")]
    public float lowHealthCutoff = 600f;

    public float cutoffTransitionTime = 1f;

    private SoundEmitter _ambientEmitter;
    private Coroutine _ambientFade;
    private Coroutine _cutoffRoutine;
    private SoundEmitter _fastEmitter;
    private bool _inCombat;
    private SoundEmitter _musicEmitter;
    private Coroutine _musicFade;
    private AudioLowPassFilter _musicLowpass;
    private SoundEmitter _normalEmitter;
    private bool _ownsMusicLowpass;
    private PlayerHealth _playerHealth;
    private Coroutine _stopRoutine;

    private void Start()
    {
        EnsureAmbient();
    }

    private void OnDestroy()
    {
        // Al destruir el objeto persistente (cierre del juego) corta todo al momento.
        if (_cutoffRoutine != null) StopCoroutine(_cutoffRoutine);
        if (_musicFade != null) StopCoroutine(_musicFade);
        if (_ambientFade != null) StopCoroutine(_ambientFade);
        if (_stopRoutine != null) StopCoroutine(_stopRoutine);
        StopEmitter(ref _normalEmitter);
        StopEmitter(ref _fastEmitter);
        StopEmitter(ref _musicEmitter);
        StopEmitter(ref _ambientEmitter);
    }

    /// <summary>Ambiente en loop. Si ya suena, no hace nada.</summary>
    public void EnsureAmbient()
    {
        if (_ambientEmitter != null && _ambientEmitter.gameObject.activeInHierarchy)
            return;

        _ambientEmitter = PlayLooped(ambientSound, ambientVolume);
    }

    /// <summary>
    ///     Llama CombatManager al empezar la transición a combate:
    ///     el ambiente se apaga con fade durante la animación.
    /// </summary>
    public void BeginCombatTransition()
    {
        EnsureAmbient();
        FadeAmbientTo(0f, transitionFadeDuration);
    }

    /// <summary>Llama CombatManager al entrar en combate (tras la transición).</summary>
    public void StartCombatAudio(PlayerHealth playerHealth)
    {
        StopCombatAudio(true);

        _inCombat = true;
        _playerHealth = playerHealth;

        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged -= HandleHealthChanged;
            _playerHealth.OnHealthChanged += HandleHealthChanged;
        }

        // El ambiente queda apagado mientras dure el combate.
        FadeAmbientTo(0f, 0.5f);

        // Música de combate: siempre suena en combate, en loop, con fade-in.
        _musicEmitter = PlayLooped(combatMusic, 0f);
        if (_musicEmitter != null)
        {
            _musicLowpass = _musicEmitter.GetComponent<AudioLowPassFilter>();
            if (_musicLowpass == null)
            {
                _musicLowpass = _musicEmitter.gameObject.AddComponent<AudioLowPassFilter>();
                _ownsMusicLowpass = true;
            }

            _musicLowpass.cutoffFrequency = healthyCutoff;
            _musicLowpass.enabled = true;
            FadeEmitter(_musicEmitter, combatMusicVolume, combatFadeInDuration, ref _musicFade);
        }

        var current = _playerHealth != null ? _playerHealth.currentHealth : int.MaxValue;
        var max = _playerHealth != null ? _playerHealth.maxHealth : int.MaxValue;
        RefreshState(current, max, true);
    }

    /// <summary>
    ///     Llama CombatManager al salir de combate: corta lo de combate con
    ///     fade-out y el ambiente vuelve con fade-in.
    /// </summary>
    public void StopCombatAudio(bool instant = false)
    {
        _inCombat = false;

        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged -= HandleHealthChanged;
            _playerHealth = null;
        }

        if (_cutoffRoutine != null)
        {
            StopCoroutine(_cutoffRoutine);
            _cutoffRoutine = null;
        }

        if (_musicFade != null)
        {
            StopCoroutine(_musicFade);
            _musicFade = null;
        }

        if (_stopRoutine != null)
        {
            StopCoroutine(_stopRoutine);
            _stopRoutine = null;
        }

        if (instant)
        {
            StopEmitter(ref _normalEmitter);
            StopEmitter(ref _fastEmitter);
            StopEmitter(ref _musicEmitter);
            CleanupMusicLowpass();
        }
        else
        {
            _stopRoutine = StartCoroutine(StopCombatLoopsFaded());
        }

        // Deja el mixer como estaba (sano) para el próximo combate.
        if (combatMusicMixer != null)
        {
            if (normalSnapshot != null)
                normalSnapshot.TransitionTo(0.1f);
            else
                combatMusicMixer.SetFloat(combatMusicCutoffParam, healthyCutoff);
        }

        // El ambiente vuelve con fade-in.
        EnsureAmbient();
        FadeAmbientTo(ambientVolume, transitionFadeDuration);
    }

    private IEnumerator StopCombatLoopsFaded()
    {
        var duration = combatFadeOutDuration;
        var t = 0f;
        var musicStart = _musicEmitter != null ? _musicEmitter.AudioSource.volume : 0f;
        var normalStart = _normalEmitter != null ? _normalEmitter.AudioSource.volume : 0f;
        var fastStart = _fastEmitter != null ? _fastEmitter.AudioSource.volume : 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            var k = 1f - Mathf.Clamp01(t / Mathf.Max(duration, 0.001f));
            if (_musicEmitter != null) _musicEmitter.AudioSource.volume = musicStart * k;
            if (_normalEmitter != null) _normalEmitter.AudioSource.volume = normalStart * k;
            if (_fastEmitter != null) _fastEmitter.AudioSource.volume = fastStart * k;
            yield return null;
        }

        StopEmitter(ref _normalEmitter);
        StopEmitter(ref _fastEmitter);
        StopEmitter(ref _musicEmitter);
        CleanupMusicLowpass();
        _stopRoutine = null;
    }

    private void CleanupMusicLowpass()
    {
        if (_musicLowpass != null)
        {
            if (_ownsMusicLowpass)
            {
                Destroy(_musicLowpass);
            }
            else
            {
                _musicLowpass.cutoffFrequency = healthyCutoff;
                _musicLowpass.enabled = false;
            }

            _musicLowpass = null;
            _ownsMusicLowpass = false;
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (!_inCombat) return;
        RefreshState(current, max, false);
    }

    private void RefreshState(int current, int max, bool instant)
    {
        var isLowHealth = current <= lowHealthThreshold;

        // 1. Latidos normales: siempre en combate, excepto con poca vida.
        if (!isLowHealth)
            _normalEmitter = EnsurePlaying(_normalEmitter, heartbeatNormal, heartbeatVolume);
        else
            StopEmitter(ref _normalEmitter);

        // 2. Latidos rápidos: solo poca vida (su mixer ya trae Lowpass).
        if (isLowHealth)
            _fastEmitter = EnsurePlaying(_fastEmitter, heartbeatFast, heartbeatVolume);
        else
            StopEmitter(ref _fastEmitter);

        // 3. Lowpass en la música con poca vida.
        ApplyMusicLowpass(isLowHealth, instant);
    }

    private void ApplyMusicLowpass(bool low, bool instant)
    {
        var target = low ? lowHealthCutoff : healthyCutoff;

        // Vía mixer (snapshots o parámetro expuesto), si está configurado.
        if (combatMusicMixer != null)
        {
            if (normalSnapshot != null && lowHealthSnapshot != null)
                (low ? lowHealthSnapshot : normalSnapshot).TransitionTo(instant ? 0f : snapshotTransitionTime);
            else
                combatMusicMixer.SetFloat(combatMusicCutoffParam, target);
        }

        // Respaldo garantizado: filtro en el source de la música.
        if (_musicLowpass != null)
        {
            if (_cutoffRoutine != null)
            {
                StopCoroutine(_cutoffRoutine);
                _cutoffRoutine = null;
            }

            if (instant || cutoffTransitionTime <= 0f)
                _musicLowpass.cutoffFrequency = target;
            else
                _cutoffRoutine = StartCoroutine(LerpCutoff(_musicLowpass, target));
        }
    }

    private IEnumerator LerpCutoff(AudioLowPassFilter filter, float target)
    {
        var start = filter.cutoffFrequency;
        var t = 0f;
        while (t < cutoffTransitionTime)
        {
            t += Time.unscaledDeltaTime;
            var k = Mathf.Clamp01(t / cutoffTransitionTime);
            if (filter != null)
                filter.cutoffFrequency = Mathf.Lerp(start, target, k);
            yield return null;
        }

        if (filter != null)
            filter.cutoffFrequency = target;
        _cutoffRoutine = null;
    }

    private void FadeAmbientTo(float target, float duration)
    {
        if (_ambientEmitter == null) return;
        FadeEmitter(_ambientEmitter, target, duration, ref _ambientFade);
    }

    private void FadeEmitter(SoundEmitter emitter, float target, float duration, ref Coroutine slot)
    {
        if (emitter == null) return;
        // Parar un fade anterior (o uno ya terminado: StopCoroutine es no-op) y arrancar el nuevo.
        if (slot != null)
        {
            StopCoroutine(slot);
            slot = null;
        }

        if (duration <= 0f)
        {
            emitter.AudioSource.volume = target;
            return;
        }

        slot = StartCoroutine(LerpVolume(emitter, target, duration));
    }

    private IEnumerator LerpVolume(SoundEmitter emitter, float target, float duration)
    {
        var start = emitter != null ? emitter.AudioSource.volume : target;
        var t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            var k = Mathf.Clamp01(t / duration);
            if (emitter != null)
                emitter.AudioSource.volume = Mathf.Lerp(start, target, k);
            else
                break;
            yield return null;
        }

        if (emitter != null)
            emitter.AudioSource.volume = target;
    }

    private SoundEmitter PlayLooped(SoundData data, float volume)
    {
        if (data == null || data.clip == null)
        {
            Debug.LogWarning($"[CombatAudio] Falta asignar un SoundData o su clip ({nameof(CombatAudioController)}).");
            return null;
        }

        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("[CombatAudio] No hay SoundManager en escena.");
            return null;
        }

        data.loop = true; // Ambiente y loops de combate.
        var emitter = SoundManager.Instance.CreateSound().WithSoundData(data).Play();
        if (emitter != null)
            emitter.AudioSource.volume = volume;
        return emitter;
    }

    private SoundEmitter EnsurePlaying(SoundEmitter current, SoundData data, float volume)
    {
        if (current != null && current.gameObject.activeInHierarchy)
            return current;
        return PlayLooped(data, volume);
    }

    private void StopEmitter(ref SoundEmitter emitter)
    {
        if (emitter != null)
        {
            try
            {
                emitter.Stop();
            }
            catch
            {
                /* ya devuelto al pool */
            }

            emitter = null;
        }
    }
}