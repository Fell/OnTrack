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
}

public class RythmGameController : MonoBehaviour
{
    public AudioSource audioSource;
    public Image rythmTarget;
    public GameObject noteAPrefab;
    public GameObject noteBPrefab;
    public GameObject noteXPrefab;
    public GameObject noteYPrefab;

    float noteRampTime = 1;

    Queue<Note> beatmap;
    List<Note> activeNotes = new List<Note>();

    float timer = 0.0f;
    Note nextNote;

    float resetFlashTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        InputController.Instance.OnAButtonDown += OnNoteA;
        InputController.Instance.OnBButtonDown += OnNoteB;
        InputController.Instance.OnXButtonDown += OnNoteX;
        InputController.Instance.OnYButtonDown += OnNoteY;
        InputController.Instance.OnResetButtonDown += Reset;

        Reset();
    }

    private void Reset()
    {
        timer = 0;
        beatmap = new Queue<Note>();

        beatmap.Enqueue(new Note(1f, NoteType.A));
        beatmap.Enqueue(new Note(1.4f, NoteType.A));
        beatmap.Enqueue(new Note(1.8f, NoteType.B));
        beatmap.Enqueue(new Note(2.0f, NoteType.B));
        beatmap.Enqueue(new Note(2.2f, NoteType.B));

        beatmap.Enqueue(new Note(2.6f, NoteType.A));
        beatmap.Enqueue(new Note(3f, NoteType.X));
        beatmap.Enqueue(new Note(3.4f, NoteType.Y));
        beatmap.Enqueue(new Note(3.8f, NoteType.B));

        beatmap.Enqueue(new Note(4.2f, NoteType.A));
        beatmap.Enqueue(new Note(4.6f, NoteType.A));
        beatmap.Enqueue(new Note(5f, NoteType.B));
        beatmap.Enqueue(new Note(5.2f, NoteType.B));
        beatmap.Enqueue(new Note(5.4f, NoteType.B));

        beatmap.Enqueue(new Note(5.8f, NoteType.A));
        beatmap.Enqueue(new Note(6.2f, NoteType.X));
        beatmap.Enqueue(new Note(6.6f, NoteType.Y));
        beatmap.Enqueue(new Note(7.0f, NoteType.B));

        nextNote = beatmap.Dequeue();
    }

    // Update is called once per frame
    void Update()
    {
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
            float diff = timer - note.hitTime;
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
        foreach(Note note in activeNotes)
        {
            if(note.type == type && timer - note.hitTime < 0.1f && timer - note.hitTime > -0.1f)
            {
                Destroy(note.noteObject);
                note.delete = true;
            }
        }

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

        GameObject noteObject = GameObject.Instantiate(prefab, transform);
        note.noteObject = noteObject;
        activeNotes.Add(note);
    }
}
