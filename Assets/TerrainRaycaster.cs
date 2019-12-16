using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.TerrainAPI;

public class TerrainRaycaster : MonoBehaviour
{
    public Space RaycastDirecitonSpace = Space.Self;
    public LayerMask modificationLayerMask = ~0;
    public Vector3 RaycastDireciton = Vector3.forward;
    public TerrainPaintUtility.BuiltinPaintMaterialPasses builtinPaintMaterialPasses;
    public float brushSize = 1;
    public float brushStrength = 0.01f;
    // [-1 to 1] -1 lowers the peaks, 1 makes holes less shallow, 0 average smooth
    public float smoothDirection = 0;
    public Texture brushTexture;
    public float range;
    public Material PaintMaterial;
    public float RaycastInterval = 1f;
    public bool Active = false;
    public UnityEvent OnRaycast;


    private Terrain targetTerrain;
    RaycastHit hit;
    Vector3 direction;

    void OnEnable()
    {
        StartCoroutine(RaycastGround());
    }

    public void Activate(float state)
    {
        if (state > 0) Active = true;
        else Active = false;
    }

    IEnumerator RaycastGround()
    {
        while (true)
        {
            if (!Active)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            //TODO: move all painting operations to external (singleton?) module

            if (RaycastDirecitonSpace == Space.Self)
            {
                if (RaycastDireciton == Vector3.forward) direction = transform.forward;
                else
                if (RaycastDireciton == -Vector3.forward) direction = -transform.forward;
                else
                if (RaycastDireciton == Vector3.up) direction = transform.up;
                else
                if (RaycastDireciton == -Vector3.up) direction = -transform.up;
                else
                if (RaycastDireciton == Vector3.right) direction = transform.right;
                else
                if (RaycastDireciton == -Vector3.right) direction = -transform.right;
            }
            else
            {
                direction = RaycastDireciton;
            }

            if (Physics.Raycast(new Ray(transform.position, direction), out hit, range, modificationLayerMask))
            {
                //Debug.DrawLine(transform.position, hit.point, Color.red, RaycastInterval);

                targetTerrain = GetTerrainAtObject(hit.transform.gameObject);
                if (targetTerrain)
                {
                    BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(targetTerrain, hit.textureCoord, brushSize, 0.0f);
                    PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(targetTerrain, brushXform.GetBrushXYBounds(), 1);

                    ApplyBrushInternal(paintContext, brushStrength, brushTexture, brushXform);
                    TerrainPaintUtility.EndPaintHeightmap(paintContext, "");
                    OnRaycast.Invoke();
                }
            }
            yield return new WaitForSeconds(RaycastInterval);
        }
    }

    private void ApplyBrushInternal(PaintContext paintContext, float brushStrength, Texture brushTexture, BrushTransform brushXform)
    {
        Vector4 brushParams = new Vector4(0.01f * brushStrength, 0.0f, 0.0f, 0.0f);
        PaintMaterial.SetTexture("_BrushTex", brushTexture);
        PaintMaterial.SetVector("_BrushParams", brushParams);
        Vector4 smoothWeights = new Vector4(
            Mathf.Clamp01(1.0f - Mathf.Abs(smoothDirection)),   // centered
            Mathf.Clamp01(-smoothDirection),                    // min
            Mathf.Clamp01(smoothDirection),                     // max
            0.0f);                                          // unused
        PaintMaterial.SetVector("_SmoothWeights", smoothWeights);

        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, PaintMaterial);

        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, PaintMaterial, (int)builtinPaintMaterialPasses);
    }

    public Terrain GetTerrainAtObject(GameObject gameObject)
    {
        if (gameObject.GetComponent<Terrain>())
        {
            return gameObject.GetComponent<Terrain>();
        }

        return default;
    }
}