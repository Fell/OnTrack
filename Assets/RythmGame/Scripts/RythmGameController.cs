using System.Collections;
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
    public bool missed;
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
    public TMPro.TextMeshProUGUI healthText;
    public GameObject noteAPrefab;
    public GameObject noteBPrefab;
    public GameObject noteXPrefab;
    public GameObject noteYPrefab;

    public Transform NoteParent = null;

    public UIParticleSystem[] ParticleSystems;

    public GameObject GameHUDGO = null;
    public GameObject StartTextGO = null;

    public GameObject EndHUD;
    public TMPro.TextMeshProUGUI finalRatingText;
    public TMPro.TextMeshProUGUI finalScoreText;

    public Animator RythmTargetAnimator = null;

    float noteRampTime = 1.5f;
    float noteCalibration = -0.016f;

    Queue<Note> easyBeatmap;
    Queue<Note> normalBeatmap;
    Queue<Note> hardBeatmap;

    Queue<Note> beatmap;
    List<Note> activeNotes = new List<Note>();

    float timer = 0.0f;
    Note nextEasyNote;
    Note nextNormalNote;
    Note nextHardNote;

    float resetFlashTimer = 0.0f;

    int score = 0;
    int combo = 0;
    int health = 100;
    int lerpedScore = 0;
    float smoothVel = 0;

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
        EndHUD.SetActive(false);
        timer = 0;
        score = 0;
        lerpedScore = 0;
        combo = 0;
        health = 100;
        isPlaying = false;

        scoreText.SetText(score.ToString());
        comboText.SetText(combo.ToString());
        healthText.SetText(health.ToString()+"%");

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
            "----AA-X-X-Y-Y-Y" +
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
            "----AA-X-X-Y-Y-Y" +
            "-----A-A--------" +
            "----A-XX--------" + // SLAP
            "---A-A-B-B-Y-Y--" +
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

        easyBeatmap = MakeBeatmap(172, "++" +
            "--A-A--A-A-A-A-A" + // CHORUS 1
            "-A--------------" +
            "--B-B--B-B-B-B-B" +
            "+" +
            "--A-A--A-A-A-A-A" +
            "-A--------------" +
            "--B-B--B-B-B-B-B" +
            "+" +
            "B-B----A--A-----" + // VERSE 1
            "A-A----B--B-----" +
            "B-B----A--A-----" +
            "A-A----B--B-----" +
            "B-B----A--A-----" + // VERSE 1
            "A-A----B--B-----" +
            "B-B----A--A-----" +
            "A-A----B--B-----" +
            "--A-A--A-A-A-A-A" + // CHORUS 1
            "-A--------------" +
            "--B-B--B-B-B-B-B" +
            "+" +
            "--A-A--A-A-A-A-A" +
            "-A--------------" +
            "--B-B--B-B-B-B-B" +
            "+" +
            "B-B----A--A-----" + // VERSE 1
            "A-A----B--B-----" +
            "B-B----A--A-----" +
            "A-A----B--B-----" +
            "B-B----A--A-----" + // VERSE 1
            "A-A----B--B-----" +
            "B-B----A--A-----" +
            "A-A----B--B-----" +
            "----A--A--------" + // SLAP
            "---A-A-A-A-A-A--" +
            "----A--A--------" +
            "------------B-B-" + // SOLO
            "B------A-A-A-A-A" +
            "-----B-B------B-" +
            "B--A-A-A-A-A--A-" +
            "A--A--A--A--A-A-" +
            "--A-A--A-A-A-A-A" + // CHORUS 3
            "-A--------------" +
            "--B-B--B-B-B-B-B" +
            "+" +
            "--A-A--A-A-A-A-A" +
            "-A--------------" +
            "--B-B--B-B-B-B-B" +
            "+" +
            "--A-A--A-A-A-A-A" + // ENDING
            "-A----BB--------");

        nextEasyNote = (easyBeatmap.Count > 0) ? easyBeatmap.Dequeue() : null;
        nextNormalNote = (normalBeatmap.Count > 0) ? normalBeatmap.Dequeue() : null;
        nextHardNote = (hardBeatmap.Count > 0) ? hardBeatmap.Dequeue() : null;
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
        EndHUD.SetActive(false);
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
        if(nextEasyNote != null && timer > nextEasyNote.hitTime - noteRampTime)
        {
            if(VehicleController.Instance.LaneId == 2)
                SpawnNote(nextEasyNote);

            nextEasyNote = (easyBeatmap.Count > 0) ? easyBeatmap.Dequeue() : null;
        }

        if(nextNormalNote != null && timer > nextNormalNote.hitTime - noteRampTime)
        {
            if(VehicleController.Instance.LaneId == 1)
                SpawnNote(nextNormalNote);

            nextNormalNote = (normalBeatmap.Count > 0) ? normalBeatmap.Dequeue() : null;
        }

        if(nextHardNote != null && timer > nextHardNote.hitTime - noteRampTime)
        {
            if(VehicleController.Instance.LaneId == 0)
                SpawnNote(nextHardNote);

            nextHardNote = (hardBeatmap.Count > 0) ? hardBeatmap.Dequeue() : null;
        }

        // Update active notes
        foreach(Note note in activeNotes)
        {
            float diff = timer - note.hitTime + noteCalibration;
            note.noteObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1060 * (diff / noteRampTime) * -1, 0);

            // Remove off-screen notes
            if(diff > 0.100f && !note.missed)
            {
                feedbackText.SetText("Miss");
                feedbackText.color = Color.red;
                combo = 0;
                comboText.SetText(combo.ToString());
                health -= 10;
                healthText.SetText(health.ToString()+"%");
                note.missed = true;
            }

            if(diff > noteRampTime)
            {
                Destroy(note.noteObject);
                note.delete = true;
            }
        }

        if(health <= 0)
            EndSong(false);

        health = Mathf.Clamp(health, 0, 100);
        healthText.SetText(health.ToString() + "%");

        activeNotes.RemoveAll(n => n.delete);

        // Ring color
        if(resetFlashTimer > 0)
            resetFlashTimer -= deltaTime;

        if(resetFlashTimer < 0)
        {
            rythmTarget.color = Color.white;
            resetFlashTimer = 0;
        }

        // Score counting effect
        lerpedScore = (int)Mathf.SmoothDamp(lerpedScore, score, ref smoothVel, 0.25f);
        scoreText.SetText(lerpedScore.ToString("N0"));
    }

    void OnNoteA()
    {
        HitNote(NoteType.A);
        FlashRing(Color.green);
        RythmTargetAnimator.SetTrigger("Hit");
    }

    void OnNoteB()
    {
        HitNote(NoteType.B);
        FlashRing(Color.red);
        RythmTargetAnimator.SetTrigger("Hit");
    }

    void OnNoteX()
    {
        HitNote(NoteType.X);
        FlashRing(Color.blue);
        RythmTargetAnimator.SetTrigger("Hit");
    }

    void OnNoteY()
    {
        HitNote(NoteType.Y);
        FlashRing(Color.yellow);
        RythmTargetAnimator.SetTrigger("Hit");
    }

    int LaneMultiplier()
    {
        switch(VehicleController.Instance.LaneId)
        {
            case 0:
                return 4;
            case 1:
                return 2;
            default:
            case 2:
                return 1;
        }
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
                    feedbackText.SetText("Perfect " + combo);
                    score += 250 * LaneMultiplier() + 10 * combo;
                    combo++;
                    health += 7;
                }
                else if(diff < 0.050f && diff > -0.050f)
                {
                    feedbackText.SetText("Good " + combo);
                    score += 100 * LaneMultiplier() + 10 * combo;
                    combo++;
                    health += 5;
                }
                else if(diff < 0.066f && diff > -0.066f)
                {
                    feedbackText.SetText("OK");
                    score += 50 * LaneMultiplier();
                    combo = 0;
                    health += 2;
                }
                else
                {
                    feedbackText.SetText("Bad");
                    score += 10 * LaneMultiplier();
                    combo = 0;
                }
                feedbackText.color = Color.white;

                Destroy(note.noteObject);
                note.delete = true;
                
                ParticleSystems[(int)note.type].Play();
            }
        }

        //scoreText.SetText(score.ToString());
        healthText.SetText(health.ToString()+"%");
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

        GameObject noteObject = GameObject.Instantiate(prefab, NoteParent);
        note.noteObject = noteObject;
        activeNotes.Add(note);
    }

    void EndSong(bool cleared)
    {
        StartTextGO.SetActive(false);
        GameHUDGO.SetActive(false);
        EndHUD.SetActive(true);

        isPlaying = false;

        finalScoreText.SetText(score.ToString("N0"));
        finalRatingText.SetText(cleared ? "Welcome Home!" : "You are lost...");
    }
}
