using UnityEngine;

namespace DefaultNamespace
{
  public class RealtorHeadAnimator : MonoBehaviour
  {
    public Transform animRoot;

    public float frequency = 25f;

    public float amplitude = 1f;

    protected Vector3 maximumAngularShake;

    private float seed;

    protected Vector3 startRot;
    
    void Start()
    {
      seed = Random.value;
      startRot = animRoot.localEulerAngles;
      maximumAngularShake = Vector3.one * amplitude;
    }
    
    void Update()
    {
      animRoot.localRotation = Quaternion.Euler(new Vector3(
        startRot.x + (maximumAngularShake.x * (Mathf.PerlinNoise(seed + 3, Time.time * frequency) * 2 - 1)),
        startRot.y + (maximumAngularShake.y * (Mathf.PerlinNoise(seed + 4, Time.time * frequency) * 2 - 1)),
        startRot.z + (maximumAngularShake.z * (Mathf.PerlinNoise(seed + 5, Time.time * frequency) * 2 - 1))
      ));
    }
  }
}