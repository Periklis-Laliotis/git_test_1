using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepManager : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioSource audioSource;
    public AudioClip[] footstepClips;

    [Tooltip("Χρόνος μεταξύ βημάτων όταν ο παίκτης κινείται με κανονική ταχύτητα")]
    public float baseStepInterval = 0.5f;

    [Tooltip("Ελάχιστη ταχύτητα για να παίζει ήχος βημάτων")]
    public float minSpeed = 0.1f;

    [Tooltip("Πολλαπλασιαστής ταχύτητας για ταχύ περπάτημα ή τρέξιμο")]
    public float stepSpeedFactor = 2.0f;

    private CharacterController controller;
    private float stepTimer = 0f;
    private float lastSpeed = 0f;

    private Vector3 lastPosition;


    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.spatialBlend = 1f; // 3D ήχος
        audioSource.volume = 0.6f;
        audioSource.playOnAwake = false;
    }

    void Update()
{
    if (controller == null)
        return;

    // Αν ο χαρακτήρας δεν χρησιμοποιεί σωστά velocity, υπολόγισε την ταχύτητα με βάση την αλλαγή θέσης
    float speed = controller.velocity.magnitude;
    if (speed < 0.01f)
    {
        // fallback: αν το velocity δεν ενημερώνεται, μετράμε μεταβολή θέσης
        Vector3 horizontalMove = new Vector3(transform.position.x, 0, transform.position.z);
        float distance = (horizontalMove - lastPosition).magnitude;
        speed = distance / Time.deltaTime;
    }

    if (speed > minSpeed)
    {
        // Υπολόγισε δυναμικά το διάστημα ανάμεσα στα βήματα
        float dynamicInterval = baseStepInterval / Mathf.Clamp(speed * stepSpeedFactor, 0.5f, 3f);

        stepTimer += Time.deltaTime;
        if (stepTimer >= dynamicInterval)
        {
            PlayFootstep(speed);
            stepTimer = 0f;
        }
    }
    else
    {
        // ❗ Δεν μηδενίζουμε το stepTimer εδώ — απλώς τον αφήνουμε να συνεχίσει φυσικά
    }

    lastSpeed = speed;
    lastPosition = new Vector3(transform.position.x, 0, transform.position.z);
}


    void PlayFootstep(float currentSpeed)
    {
        if (footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        float volume = Mathf.Lerp(0.3f, 0.7f, currentSpeed / 5f); // πιο δυνατός ήχος όσο αυξάνει η ταχύτητα
        audioSource.volume = volume;
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(footstepClips[index]);
    }

    public void ChangeFootstepClips(AudioClip[] newClips)
    {
        footstepClips = newClips;
    }
}
