using UnityEngine;

public class Spin : MonoBehaviour
{
    [Header("Normal spin")]
    [SerializeField] private float degreesPerSecond = 90f;

    [Header("Fast spin (on collect)")]
    [SerializeField] private int extraSpins = 2;
    [SerializeField] private float fastSpinDuration = 0.6f;

    // How the extra spins play out over time (0 -> 1).
    // Default: fast kick at the start, easing smoothly back into the normal speed.
    [SerializeField] private AnimationCurve fastSpinCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 2f, 2f),
        new Keyframe(1f, 1f, 0f, 0f));

    private float fastSpinTimer = -1f;   // < 0 means no fast spin is playing
    private float fastSpinDegreesDone;   // extra degrees already applied this fast spin

    // Call this when a coin is collected
    public void PlayFastSpins()
    {
        // Restarting mid-spin just begins a new set of extra spins from the current angle (no snapping)
        fastSpinTimer = 0f;
        fastSpinDegreesDone = 0f;
    }

    void Update()
    {
        // Unscaled so the icon keeps spinning while the game is paused
        float dt = Time.unscaledDeltaTime;
        float rotationThisFrame = degreesPerSecond * dt;

        if (fastSpinTimer >= 0f)
        {
            fastSpinTimer += dt;
            float t = Mathf.Clamp01(fastSpinTimer / fastSpinDuration);

            // Where the extra rotation should be by now, minus what's already been applied
            float extraTarget = fastSpinCurve.Evaluate(t) * 360f * extraSpins;
            rotationThisFrame += extraTarget - fastSpinDegreesDone;
            fastSpinDegreesDone = extraTarget;

            if (t >= 1f) fastSpinTimer = -1f; // done, back to normal speed
        }

        transform.Rotate(0f, rotationThisFrame, 0f, Space.World);
    }
}