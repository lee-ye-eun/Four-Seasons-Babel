using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingCatalog", menuName = "Babel/BuildingCatalog")]
public class BuildingCatalog : ScriptableObject
{
    public List<BuildingSO> buildings;
}
