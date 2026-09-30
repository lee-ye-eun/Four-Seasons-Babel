using UnityEngine;

[CreateAssetMenu(fileName = "BuildingSO", menuName = "Babel/Building")]
public class BuildingSO : ScriptableObject
{
    [Header("Info")]
    public string buildingName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Stats")]
    public int cost;
    public float useTime = 3f;
    public int faithProduction = 0;

    [Header("Capacity per Season")]
    // 값이 0인 계절은 자동으로 폐쇄 처리됨 — currentOccupants가 항상 정원 이상이 되어 아무도 입장할 수 없음
    public int capacitySpring = 1;
    public int capacitySummer = 1;
    public int capacityAutumn = 1;
    public int capacityWinter = 1;

    public int GetCapacity(Season season) => season switch
    {
        Season.Spring => capacitySpring,
        Season.Summer => capacitySummer,
        Season.Autumn => capacityAutumn,
        Season.Winter => capacityWinter,
        _ => 0
    };

    [Header("Visual")]
    public Sprite springVisualSprite;
    public Sprite summerVisualSprite;
    public Sprite autumnVisualSprite;
    public Sprite winterVisualSprite;
}
