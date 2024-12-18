using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ImageTracker : MonoBehaviour
{
    public GameObject hydrogenPrefab; // Prefab for Hydrogen Atom
    public GameObject heliumPrefab;   // Prefab for Helium Atom
    public GameObject lithiumPrefab;  // Prefab for Lithium Atom
    public GameObject carbonPrefab;   // Prefab for Carbon Atom
    public GameObject oxygenPrefab;   // Prefab for Oxygen Atom
    public ReactionManager reactionManager; // Reference to the ReactionManager script

    private ARTrackedImageManager trackedImageManager;

    // Configurable offset to adjust atom position above QR code
    public float verticalOffset = 0.1f; // Distance above the QR code in meters

    // Dictionary to store instantiated atoms keyed by tracked image unique ID
    private Dictionary<string, GameObject> instantiatedAtoms = new Dictionary<string, GameObject>();

    void Awake()
    {
        // Get the ARTrackedImageManager component
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        // Subscribe to the trackedImagesChanged event
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        // Unsubscribe from the trackedImagesChanged event
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // Handle added tracked images
        foreach (var trackedImage in args.added)
        {
            Debug.Log($"Detected image: {trackedImage.referenceImage.name}");
            HandleTrackedImage(trackedImage);
        }

        // Handle updated tracked images
        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                Debug.Log($"Tracking updated for: {trackedImage.referenceImage.name}");
                UpdateTrackedImage(trackedImage);
            }
            else
            {
                // Hide atom instead of removing it
                HideTrackedImage(trackedImage);
            }
        }

        // Handle removed tracked images
        foreach (var trackedImage in args.removed)
        {
            Debug.Log($"Removed tracking for: {trackedImage.referenceImage.name}");
            RemoveTrackedImage(trackedImage);
        }
    }

    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        string uniqueId = trackedImage.trackableId.ToString();

        // Check if the atom for this image has already been instantiated
        if (!instantiatedAtoms.ContainsKey(uniqueId))
        {
            // Determine the prefab to instantiate
            GameObject prefabToInstantiate = null;

            if (imageName == "Hydrogen") prefabToInstantiate = hydrogenPrefab;
            else if (imageName == "Helium") prefabToInstantiate = heliumPrefab;
            else if (imageName == "Lithium") prefabToInstantiate = lithiumPrefab;
            else if (imageName == "Carbon") prefabToInstantiate = carbonPrefab;
            else if (imageName == "Oxygen1" || imageName == "Oxygen2") prefabToInstantiate = oxygenPrefab;

            if (prefabToInstantiate != null)
            {
                // Calculate the position directly above the QR code
                Vector3 qrCodePosition = trackedImage.transform.position + new Vector3(0, verticalOffset, 0);
                Quaternion qrCodeRotation = trackedImage.transform.rotation;

                // Instantiate the atom and parent it to the QR code
                GameObject atom = Instantiate(prefabToInstantiate, qrCodePosition, qrCodeRotation, trackedImage.transform);
                atom.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Adjust size as needed

                // Correct the rotation of the electron orbit for top-down view
                Transform electronOrbit = atom.transform.Find("Electron Orbit");
                if (electronOrbit != null)
                {
                    // Reset the orbit rotation explicitly to ensure top-down view
                    electronOrbit.localRotation = Quaternion.Euler(0, 90, 0);
                }

                // Notify ReactionManager about the added atom
                reactionManager.AddAtom(atom);

                // Store the instantiated atom in the dictionary with a unique ID
                instantiatedAtoms[uniqueId] = atom;

                Debug.Log($"Instantiated {imageName} at position: {qrCodePosition}");
            }
        }
        else
        {
            // If already instantiated, ensure it is active and positioned correctly
            UpdateTrackedImage(trackedImage);
        }
    }

    private void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        string uniqueId = trackedImage.trackableId.ToString();

        // Ensure the atom's position and rotation follow the QR code
        if (instantiatedAtoms.ContainsKey(uniqueId))
        {
            GameObject atom = instantiatedAtoms[uniqueId];
            atom.SetActive(true); // Ensure atom is visible
            atom.transform.position = trackedImage.transform.position + new Vector3(0, verticalOffset, 0);
            atom.transform.rotation = trackedImage.transform.rotation;
        }
    }

    private void HideTrackedImage(ARTrackedImage trackedImage)
    {
        string uniqueId = trackedImage.trackableId.ToString();

        // Hide the atom instead of removing it
        if (instantiatedAtoms.ContainsKey(uniqueId))
        {
            GameObject atom = instantiatedAtoms[uniqueId];
            atom.SetActive(false); // Temporarily hide the atom
            Debug.Log($"Hiding atom: {atom.name}");
        }
    }

    private void RemoveTrackedImage(ARTrackedImage trackedImage)
    {
        string uniqueId = trackedImage.trackableId.ToString();

        // Remove the atom from the ReactionManager
        if (instantiatedAtoms.ContainsKey(uniqueId))
        {
            GameObject atom = instantiatedAtoms[uniqueId];
            reactionManager.RemoveAtom(atom);

            // Destroy the atom and remove it from the dictionary
            Destroy(atom);
            instantiatedAtoms.Remove(uniqueId);

            Debug.Log($"Removed atom with ID: {uniqueId}");
        }
    }
}
