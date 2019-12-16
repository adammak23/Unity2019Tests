using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeLoad : MonoBehaviour
{
    public koparaController kopara;
    public float MaxLoad;
    public float debugLoad;
    public Transform chunk1, chunk2;
    public float LoadIncrement = 5;
    public float ParticleLoad = 10;
    public GameObject BuildingParticlePrefab, SmoothingParticlePrefab;
    public Transform ParticleSpawnPoint;
    public Vector3 ParticleSpawnExtent;
    public bool AllowRemoveCargo = true;

    float loadCounter = 0;
    //float secondaryLoadCounter = 0;
    Vector3 SpawnPosition;

    public float CurrentLoad
    {
        get { return _currentLoad; }
        set
        {
            _currentLoad = value;
            OnLoadChanged.Invoke(value);
        }
    }
    private float _currentLoad;

    public FloatEvent OnLoadChanged;

    public Vector3 MinScale, MaxScale;

    public void Scale(float load)
    {
        chunk1.transform.localScale = Vector3.Lerp(MinScale, MaxScale, load / MaxLoad);
        chunk2.transform.localScale = chunk1.transform.localScale;
    }

    private void Start()
    {
        loadCounter = debugLoad;

        //Voxeland.current.OnAlterEvent.AddListener(AddCargo);
        kopara.OnAngularLimitChange.AddListener(RemoveCargo);
    }

    private void Update()
    {
        Scale(debugLoad);
    }

    public void AllowRemovingCargo(object detectedGround)
    {
        if (detectedGround == null) AllowRemoveCargo = true;
        else AllowRemoveCargo = false;
    }

    public void AddCargo()
    {
        if (!AllowRemoveCargo) return;

        if (debugLoad < MaxLoad)
        {
            debugLoad += LoadIncrement;
            loadCounter = debugLoad;
            //secondaryLoadCounter = debugLoad;
        }
    }

    public void RemoveCargo(float amount)
    {
        if (!AllowRemoveCargo) return;

        if (debugLoad > 0)
        {
            debugLoad -= amount;

            // Every {ParticleLoad} amount substracted from Load,
            if (loadCounter - ParticleLoad > debugLoad)
            {
                // spawn particle in random point between
                // { -ParticleSpawnExtent} and {+ParticleSpawnExtent} from {ParticleSpawnPoint}
                SpawnPosition = ParticleSpawnPoint.position + Vector3.Scale(Random.insideUnitSphere, ParticleSpawnExtent);

                GameObject particle = Instantiate(BuildingParticlePrefab, SpawnPosition, Random.rotation);
                //particle.GetComponent<VoxelandCollider>().brush.extent = (int)Mathf.Lerp(1, 4, debugLoad / MaxLoad);
                loadCounter = debugLoad;
            }
            // Every {2*ParticleLoad} amount substracted from Load,
            //if (secondaryLoadCounter - (2 * ParticleLoad) > debugLoad)
            //{
            //    GameObject particle = Instantiate(SmoothingParticlePrefab, SpawnPosition, Quaternion.identity);
            //    particle.GetComponent<VoxelandCollider>().brush.extent = (int)Mathf.Lerp(1, 5, debugLoad / MaxLoad);
            //    secondaryLoadCounter = debugLoad;
            //}
        }
    }

    public void SpawnSmoothingParticle()
    {
        Instantiate(SmoothingParticlePrefab, ParticleSpawnPoint.position, Quaternion.identity);
    }
}
