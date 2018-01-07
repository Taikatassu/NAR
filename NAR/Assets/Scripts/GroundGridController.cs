using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundGridController : MonoBehaviour
{
    Material gridMaterial;
    float gridSpacing = 3;

    enum EOffsetDirection
    {
        XOffset,
        ZOffset
    }

    private void Start()
    {
        gridMaterial = GetComponent<Renderer>().material;
        gridSpacing = gridMaterial.GetFloat("_GridSpacing");
    }

    private void OnEnable()
    {
        EventManager.OnPlayerMovement += OnPlayerMovement;
        EventManager.OnRequestGridOffset += OnRequestGridOffset;
        EventManager.OnLevelRestart += OnLevelRestart;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerMovement -= OnPlayerMovement;
        EventManager.OnRequestGridOffset -= OnRequestGridOffset;
        EventManager.OnLevelRestart -= OnLevelRestart;
    }

    private void OnLevelRestart()
    {
        ModifyGridOffset(EOffsetDirection.XOffset, 0, false);
        ModifyGridOffset(EOffsetDirection.ZOffset, 0, false);
    }

    //Fake player movement by changing the ground grid offset
    private void ModifyGridOffset(EOffsetDirection offsetToChange, float offsetValue, bool addToExistingOffset = true)
    {
        string offsetFloatName = "";

        switch (offsetToChange)
        {
            case EOffsetDirection.XOffset:
                offsetFloatName = "_PosXOffset";
                break;
            case EOffsetDirection.ZOffset:
                offsetFloatName = "_PosZOffset";
                break;
            default:
                break;
        }

        if (addToExistingOffset)
        {
            float oldOffset = gridMaterial.GetFloat(offsetFloatName);
            float newOffset = oldOffset + offsetValue;
            if (newOffset >= gridSpacing / 2)
            {
                newOffset -= gridSpacing;
            }
            else if (newOffset <= -gridSpacing / 2)
            {
                newOffset += gridSpacing;
            }

            gridMaterial.SetFloat(offsetFloatName, newOffset);
        }
        else
        {
            gridMaterial.SetFloat(offsetFloatName, offsetValue);
        }
    }

    private void OnPlayerMovement(Vector2 playerMovementVector)
    {
        if (playerMovementVector.y != 0)
        {
            ModifyGridOffset(EOffsetDirection.ZOffset, playerMovementVector.y);
        }
        if (playerMovementVector.x != 0)
        {
            ModifyGridOffset(EOffsetDirection.XOffset, playerMovementVector.x);
        }
    }

    private Vector2 OnRequestGridOffset()
    {
        Vector2 gridOffset = new Vector2(gridMaterial.GetFloat("_PosXOffset"), gridMaterial.GetFloat("_PosZOffset"));
        return gridOffset;
    }
}
