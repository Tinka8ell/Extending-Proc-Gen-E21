using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class StarBand
{
    [SerializeField] public float proportion, fromXZ, toXZ, fromY, toY;
}

public class ScatterMyStars : MonoBehaviour
{
    [SerializeField] ParticleSystem starsParticleSystem;
    [SerializeField] int starCount = 1000;
    [SerializeField] StarBand[] spreads;

    private ParticleSystem.Particle[] particles;
    private byte[] alphas;
    private Color32 color32;
    private float radius; //, spreadSum, spreadChooser, sum, angleXZ, angleY;
    private int chosenSpread;
    private Vector3 position;

    private DayNightController dayNightController;

    void Start()
    {
        float spreadSum, spreadChooser, sum, angleXZ, angleY, sinY;
        if (starsParticleSystem == null) {
            starsParticleSystem = GetComponent<ParticleSystem>();
        }
        alphas = new byte[starCount];
        particles = new ParticleSystem.Particle[starCount];

        starsParticleSystem.Emit(starCount);
        starsParticleSystem.GetParticles(particles, starCount, 0);
        radius = starsParticleSystem.shape.radius;
        spreadSum = 0f;
        foreach (StarBand spread in spreads)
        {
            spreadSum += spread.proportion;
        }
        for (int partIndex = 0; partIndex < particles.Length; partIndex++)
        {
            alphas[partIndex] = particles[partIndex].startColor.a;
            spreadChooser = Random.Range(0f, spreadSum);
            sum = 0f;
            for (int spreadIndex = 0; spreadIndex < spreads.Length; spreadIndex++)
            {
                sum += spreads[spreadIndex].proportion;
                if (spreadChooser < sum)
                {
                    chosenSpread = spreadIndex;
                    break;
                    //spreadIndex = spreads.Length;
                }
            }
            angleXZ = Mathf.Deg2Rad * Random.Range(spreads[chosenSpread].fromXZ, spreads[chosenSpread].toXZ);
            angleY = Mathf.Deg2Rad * Random.Range(spreads[chosenSpread].fromY, spreads[chosenSpread].toY);
            sinY = Mathf.Sin(angleY);
            position.x = radius * Mathf.Cos(angleXZ) * sinY;
            position.z = radius * Mathf.Sin(angleXZ) * sinY;
            position.y = radius * Mathf.Cos(angleY);
            particles[partIndex].position = position;
        }
        starsParticleSystem.SetParticles(particles, starCount);

        dayNightController = GetComponent<DayNightController>();
        dayNightController.WorldSpunEvent.AddListener(UpdateStars);
    }

    private void OnDestroy(){
        if (dayNightController != null)
            dayNightController.WorldSpunEvent.RemoveListener(UpdateStars);
    }


    private void UpdateStars(float rotation)
    {
        // Update stars here
        for (int i = 0; i < particles.Length; i++)
        {
            color32 = particles[i].startColor;
            color32.a = (byte)Mathf.Clamp(alphas[i] * (starsParticleSystem.transform.TransformPoint(particles[i].position).y - starsParticleSystem.transform.position.y)
                / radius, 0, alphas[i]);
            particles[i].startColor = color32;
        }
        starsParticleSystem.SetParticles(particles, particles.Length, 0);
    }
}
