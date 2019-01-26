﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum NoteType
{
    A,
    B,
    X,
    Y
}

public class Note
{
    public Note(float time, NoteType type)
    {
        this.hitTime = time;
        this.type = type;
    }

    public float hitTime;
    public NoteType type;
    public GameObject noteObject;
    public bool delete;
}

public class RythmGameController : MonoBehaviour
{
    public static RythmGameController Instance { get; private set; }

    bool isPlaying = false;

    public AudioSource audioSource;
    public Image rythmTarget;
    public TMPro.TextMeshProUGUI feedbackText;
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI comboText;
    public GameObject noteAPrefab;
    public GameObject noteBPrefab;
    public GameObject noteXPrefab;
    public GameObject noteYPrefab;

    public UIParticleSystem[] ParticleSystems;

    public GameObject GameHUDGO = null;
    public GameObject StartTextGO = null;

    float noteRampTime = 1.5f;
    float noteCalibration = -0.016f;

    Queue<Note> easyBeatmap;
    Queue<Note> normalBeatmap;
    Queue<Note> hardBeatmap;

    Queue<Note> beatmap;
    List<Note> activeNotes = new List<Note>();

    float timer = 0.0f;
    Note nextNote;

    float resetFlashTimer = 0.0f;

    int score = 0;
    int combo = 0;

    // Awake function
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        InputController.Instance.OnAButtonDown += OnNoteA;
        InputController.Instance.OnBButtonDown += OnNoteB;
        InputController.Instance.OnXButtonDown += OnNoteX;
        InputController.Instance.OnYButtonDown += OnNoteY;
        InputController.Instance.OnResetButtonDown += Reset;
        InputController.Instance.OnStartButtonDown += Play;

        Reset();
    }

    private void Reset()
    {
        StartTextGO.SetActive(true);
        GameHUDGO.SetActive(false);
        timer = 0;
        score = 0;
        combo = 0;
        isPlaying = false;

        scoreText.SetText(score.ToString());
        comboText.SetText(combo.ToString());

        hardBeatmap = MakeBeatmap(172, "++" +
            "--A-A-BB-A-A-A-X" + // CHORUS 1
            "-X--------------" +
            "--A-A-BB-A-X-Y-Y" +
            "+" +
            "--A-A-BB-A-A-A-X" +
            "-X--------------" +
            "--A-A-BB-A-X-Y-Y" +
            "+" +
            "Y-Y----A-AA-----" + // VERSE 1
            "X-X----A-AB-----" +
            "Y-Y----A-AA-----" +
            "X-X----A-AB-----" +
            "X-----AA----A---" + // CHIMES 1
            "X-----YY--------" +
            "----AA-B-B-B-B-B" +
            "-----X-X--------" +
            "--A-A-BB-A-A-A-X" + // CHORUS 2
            "-X--------------" +
            "--A-A-BB-A-X-Y-Y" +
            "X-X--X-X-X-X-X--" +
            "--A-A-BB-A-A-A-X" +
            "-X--------------" +
            "--A-A-BB-A-X-Y-Y" +
            "X-X--X-X-X-X-X--" +
            "Y-Y----A-AA-----" + // VERSE 2
            "X-X----A-AB-----" +
            "Y-Y----A-AA-----" +
            "X-X----A-AB-----" +
            "X-----AA----A---" + // CHIMES 2
            "X-----YY--------" +
            "----AA-A-B-A-X-A" +
            "-----A-A--------" +
            "----A-XX--------" + // SLAP
            "---A-A-B-B-X-X--" +
            "----A-XX--------" +
            "------------Y-Y-" + // SOLO
            "Y-----XX-A-A-A-A" +
            "-----Y-B------B-" +
            "B-AA-A-A-A-A--A-" +
            "A--B--Y--X--A-X-" + // CHORUS 3
            "--X-X-YY-X-X-X-B" +
            "-B--------------" +
            "--X-X-YY-X-A-B-B" +
            "+" +
            "--X-X-YY-X-X-X-B" +
            "-B--------------" +
            "--X-X-YY-X-A-B-B" +
            "+" +
            "--A-A-BB-A-A-A-X" + // ENDING
            "-X----YY--------");

        normalBeatmap = MakeBeatmap(172, "++" +
            "--A-A-BB-A-A-A-A" + // CHORUS 1
            "-A--------------" +
            "--A-A-BB-A-A-B-B" +
            "+" +
            "--A-A-BB-A-A-A-A" +
            "-A--------------" +
            "--A-A-BB-A-A-B-B" +
            "+" +
            "B-B----A-AA-----" + // VERSE 1
            "A-A----A-AB-----" +
            "B-B----A-AA-----" +
            "A-A----A-AB-----" +
            "A-----AA----A---" + // CHIMES 1
            "A-----BB--------" +
            "----AA-B-B-B-B-B" +
            "-----A-A--------" +
            "--A-A-BB-A-A-A-A" + // CHORUS 2
            "-A--------------" +
            "--A-A-BB-A-A-B-B" +
            "+" +
            "--A-A-BB-A-A-A-A" +
            "-A--------------" +
            "--A-A-BB-A-A-B-B" +
            "+" +
            "B-B----A-AA-----" + // VERSE 2
            "A-A----A-AB-----" +
            "B-B----A-AA-----" +
            "A-A----A-AB-----" +
            "A-----AA----A---" + // CHIMES 2
            "A-----BB--------" +
            "----AA-A-B-A-A-A" +
            "-----A-A--------" +
            "----A-AA--------" + // SLAP
            "---A-A-B-B-A-A--" +
            "----A-AA--------" +
            "------------B-B-" + // SOLO
            "B-----AA-A-A-A-A" +
            "-----B-B------B-" +
            "B-AA-A-A-A-A--A-" +
            "A--B--B--A--A-A-" + // CHORUS 3
            "--A-A-BB-A-A-A-B" +
            "-B--------------" +
            "--A-A-BB-A-A-B-B" +
            "+" +
            "--A-A-BB-A-A-A-B" +
            "-B--------------" +
            "--A-A-BB-A-A-B-B" +
            "+" +
            "--A-A-BB-A-A-A-A" + // ENDING
            "-A----BB--------");

        beatmap = hardBeatmap;

        nextNote = beatmap.Dequeue();
    }

    Queue<Note> MakeBeatmap(int bpm, string data)
    {
        Queue<Note> result = new Queue<Note>();

        float timeStep = (1f / (bpm / 60f)) / 2f;
        float pos = 0;
        foreach(char c in data)
        { 
            switch(c)
            {
                case 'A':
                    result.Enqueue(new Note(pos, NoteType.A));
                    break;
                case 'B':
                    result.Enqueue(new Note(pos, NoteType.B));
                    break;
                case 'X':
                    result.Enqueue(new Note(pos, NoteType.X));
                    break;
                case 'Y':
                    result.Enqueue(new Note(pos, NoteType.Y));
                    break;
                case '+':
                    pos += timeStep * 15;
                    break;
            }
            pos += timeStep;
        }

        return result;
    }

    void Play()
    {
        isPlaying = true;

        StartTextGO.SetActive(false);
        GameHUDGO.SetActive(true);
    }

    public void StartModifier(PowerUpTypes types)
    {
        Debug.Log("Enter");
    }

    public void EndModifier(PowerUpTypes types)
    {
        Debug.Log("Exit");
    }

    // Update is called once per frame
    void Update()
    {
        if(!isPlaying)
            return;

        float deltaTime = Time.deltaTime;      
        timer += deltaTime;

        // Spawn overdue notes
        if(nextNote != null && timer > nextNote.hitTime - noteRampTime)
        {
            SpawnNote(nextNote);

            if(beatmap.Count > 0)
                nextNote = beatmap.Dequeue();
            else
                nextNote = null;
        }

        // Update active notes
        foreach(Note note in activeNotes)
        {
            float diff = timer - note.hitTime + noteCalibration;
            note.noteObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1060 * (diff / noteRampTime) * -1, 0);

            // Remove off-screen notes
            if(diff > noteRampTime)
            {
                Destroy(note.noteObject);
                note.delete = true;
            }
        }

        activeNotes.RemoveAll(n => n.delete);

        // Ring color
        if(resetFlashTimer > 0)
            resetFlashTimer -= deltaTime;

        if(resetFlashTimer < 0)
        {
            rythmTarget.color = Color.white;
            resetFlashTimer = 0;
        }
    }

    void OnNoteA()
    {
        HitNote(NoteType.A);
        FlashRing(Color.green);
    }

    void OnNoteB()
    {
        HitNote(NoteType.B);
        FlashRing(Color.red);
    }

    void OnNoteX()
    {
        HitNote(NoteType.X);
        FlashRing(Color.blue);
    }

    void OnNoteY()
    {
        HitNote(NoteType.Y);
        FlashRing(Color.yellow);
    }

    void HitNote(NoteType type)
    {
        feedbackText.SetText("");
        foreach(Note note in activeNotes)
        {
            float diff = timer - note.hitTime + noteCalibration;
            if(note.type == type && diff < 0.1f && diff > -0.1f)
            {
                if(diff < 0.033f && diff > -0.033f)
                {
                    feedbackText.SetText("Perfect");
                    score += 250 + 10 * combo;
                    combo++;
                }
                else if(diff < 0.050f && diff > -0.050f)
                {
                    feedbackText.SetText("Good");
                    score += 100 + 10 * combo;
                    combo++;
                }
                else if(diff < 0.066f && diff > -0.066f)
                {
                    feedbackText.SetText("OK");
                    score += 50;
                    combo = 0;
                }
                else
                {
                    feedbackText.SetText("Bad");
                    score += 10;
                    combo = 0;
                }

                //TODO(Felix): Reset combo when nothing is pressed

                Destroy(note.noteObject);
                note.delete = true;
                
                ParticleSystems[(int)note.type].Play();
            }
        }

        scoreText.SetText(score.ToString());
        comboText.SetText(combo.ToString());

        activeNotes.RemoveAll(n => n.delete);
        audioSource.PlayOneShot(audioSource.clip);
    }

    void FlashRing(Color color)
    {
        rythmTarget.color = color;
        resetFlashTimer = 0.100f;
    }

    void SpawnNote(Note note)
    {
        GameObject prefab = noteAPrefab;
        switch(note.type)
        {
            case NoteType.A:
                prefab = noteAPrefab;
                break;
            case NoteType.B:
                prefab = noteBPrefab;
                break;
            case NoteType.X:
                prefab = noteXPrefab;
                break;
            case NoteType.Y:
                prefab = noteYPrefab;
                break;
        }

        GameObject noteObject = GameObject.Instantiate(prefab, GameHUDGO.transform);
        note.noteObject = noteObject;
        activeNotes.Add(note);
    }
}
