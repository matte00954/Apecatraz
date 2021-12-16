using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public Text nameText;
    public Text dialogueText;

    public Animator animator;
    public Animator blur;

    private Queue<string> sentences;

    public static bool isActive;

    
    
    void Start()
    {
        sentences = new Queue<string>();
    }

    private void Update()
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

    

    public void StartDialogue (Dialogue dialogue)
    {
        Debug.Log("Starting conversation with " + dialogue.name);

        animator.SetBool("IsOpen", true);

        blur.SetBool("IsHere", true);

        isActive = true;

        nameText.text = dialogue.name;

        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        //DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if(sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
        Debug.Log(sentence);
    }

   

    IEnumerator TypeSentence (string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    void EndDialogue()
    {
        Debug.Log("End of conversation");
        animator.SetBool("IsOpen", false);
        blur.SetBool("IsHere", false);
        isActive = false;
    }

}
