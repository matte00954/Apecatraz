// Author: Sebastian Klötz
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    private static bool isPausedWhileReading;
    private static bool isActive;

    [SerializeField] private Text nameText;
    [SerializeField] private Text dialogueText;

    [SerializeField] private Animator animator;
    [SerializeField] private Animator blur;
    [SerializeField] private Animator light;

    private Queue<string> sentences;

    public static bool IsPausedWhileReading { get => isPausedWhileReading; set => isPausedWhileReading = value; }
    public static bool IsActive { get => isActive; set => isActive = value; }

    public void StartDialogue(Dialogue dialogue)
    {
        Debug.Log("Starting conversation with " + dialogue.name);

        animator.SetBool("IsOpen", true);
        blur.SetBool("IsHere", true);
        light.SetBool("Reading", true);

        isActive = true;
        Time.timeScale = 0;
        isPausedWhileReading = true;

        nameText.text = dialogue.name;
        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
            sentences.Enqueue(sentence);

        ////DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
        Debug.Log(sentence);
    }

    public void EndDialogue()
    {
        Debug.Log("End of conversation");
        animator.SetBool("IsOpen", false);
        blur.SetBool("IsHere", false);
        light.SetBool("Reading", false);
        isActive = false;
        Time.timeScale = 1;
        isPausedWhileReading = false;
    }

    private void Start() => sentences = new Queue<string>();

    /*private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            DisplayNextSentence();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            EndDialogue();
        }
    }
    */

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = string.Empty;
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }
}
