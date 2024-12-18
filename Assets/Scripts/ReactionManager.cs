using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReactionManager : MonoBehaviour
{
    public GameObject co2Prefab; // Prefab for the CO₂ molecule
    public Button reactButton;  // Reference to the React Button
    public AudioSource audioSource; // Reference to the Audio Source for sound
    public AudioClip reactionSound; // Sound clip to play on reaction

    private List<GameObject> detectedAtoms = new List<GameObject>(); // List of detected atoms

    void Start()
    {
        // Disable the React Button initially
        reactButton.interactable = false;

        // Assign the React function to the button
        reactButton.onClick.AddListener(React);
    }

    public void AddAtom(GameObject atom)
    {
        // Add the detected atom to the list
        detectedAtoms.Add(atom);

        // Check if reaction conditions are met
        CheckForReaction();
    }

    public void RemoveAtom(GameObject atom)
    {
        // Remove the atom from the list
        detectedAtoms.Remove(atom);

        // Re-check reaction conditions
        CheckForReaction();
    }

    private void CheckForReaction()
    {
        int carbonCount = 0;
        int oxygenCount = 0;

        // Count Carbon and Oxygen atoms
        foreach (var atom in detectedAtoms)
        {
            if (atom.name.Contains("Carbon")) carbonCount++;
            if (atom.name.Contains("Oxygen")) oxygenCount++;
        }

        // Enable the React Button only if 1 Carbon and 2 Oxygens are detected
        reactButton.interactable = (carbonCount == 1 && oxygenCount == 2);
    }

    public void React()
    {
        Debug.Log("Reacting to form CO₂!");

        // Play the reaction sound
        if (audioSource != null && reactionSound != null)
        {
            audioSource.PlayOneShot(reactionSound);
        }

        // Find the position of the Carbon atom
        Vector3 reactionPosition = Vector3.zero;
        GameObject carbonAtom = null;

        foreach (var atom in detectedAtoms)
        {
            if (atom.name.Contains("Carbon"))
            {
                reactionPosition = atom.transform.position;
                carbonAtom = atom;
                break;
            }
        }

        // Destroy the individual atoms (Carbon and Oxygens)
        List<GameObject> atomsToDestroy = new List<GameObject>();

        foreach (var atom in detectedAtoms)
        {
            if (atom.name.Contains("Carbon") || atom.name.Contains("Oxygen"))
            {
                atomsToDestroy.Add(atom);
            }
        }

        foreach (var atom in atomsToDestroy)
        {
            detectedAtoms.Remove(atom);
            Destroy(atom);
        }

        // Instantiate the CO₂ molecule at the Carbon atom's position
        if (carbonAtom != null)
        {
            Instantiate(co2Prefab, reactionPosition, Quaternion.identity);
            Debug.Log("CO₂ molecule created at position: " + reactionPosition);
        }
        else
        {
            Debug.LogError("Error: Carbon atom position not found for CO₂ creation.");
        }

        // Disable the React Button after the reaction
        reactButton.interactable = false;
    }
}
