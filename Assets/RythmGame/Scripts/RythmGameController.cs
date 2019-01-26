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

public struct Note
{
    public float hitTime;
    public NoteType type;
}

public class RythmGameController : MonoBehaviour
{
    public Image rythmTarget;
    public GameObject noteAPrefab;
    public GameObject noteBPrefab;
    public GameObject noteXPrefab;
    public GameObject noteYPrefab;

    public float noteSpeed = 1;
    public float noteRampTime = 0.5f;

    float timer = 0.0f;

    Queue<Note> beatmap;
    List<GameObject> activeNotes = new List<GameObject>();
    
    Note nextNote;

    // Start is called before the first frame update
    void Start()
    {
        //TODO(Felix): Clear this!
        beatmap = new Queue<Note>();

        Note n = new Note();
        n.hitTime = 1.0f;
        n.type = NoteType.A;
        beatmap.Enqueue(n);

        n = new Note();
        n.hitTime = 1.5f;
        n.type = NoteType.A;
        beatmap.Enqueue(n);

        n = new Note();
        n.hitTime = 2f;
        n.type = NoteType.A;
        beatmap.Enqueue(n);

        n = new Note();
        n.hitTime = 2.5f;
        n.type = NoteType.B;
        beatmap.Enqueue(n);

        n = new Note();
        n.hitTime = 3f;
        n.type = NoteType.A;
        beatmap.Enqueue(n);

        nextNote = beatmap.Dequeue();
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime;
        timer += deltaTime;

        if(timer > nextNote.hitTime - noteRampTime && beatmap.Count > 0)
        {
            switch(nextNote.type)
            {
                case NoteType.A:
                    GameObject note = GameObject.Instantiate(noteAPrefab, transform);
                    note.GetComponent<RectTransform>().anchoredPosition = new Vector2(-600, 0);
                    activeNotes.Add(note);
                    break;
                case NoteType.B:
                    GameObject note2 = GameObject.Instantiate(noteBPrefab, transform);
                    note2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-600, 0);
                    activeNotes.Add(note2);
                    break;
            }

            if(beatmap.Count > 0)
                nextNote = beatmap.Dequeue();
        }

        // Update active notes
        foreach(GameObject note in activeNotes)
        {
            Vector3 velocity = Vector3.Normalize(rythmTarget.transform.localPosition - note.transform.localPosition) * noteSpeed * deltaTime;
            note.transform.Translate(velocity);
        }
    }
}
