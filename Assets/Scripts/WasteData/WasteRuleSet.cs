using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WasteRule
{
    public WasteItemData wasteItem;
    public WasteCategoryData wasteCategory;

    [Tooltip("Kenapa begitu. Teks ini yang muncul sebagai kartu fakta ke pemain.")]
    [TextArea(2, 4)]
    public string note;
}

[CreateAssetMenu(fileName = "Aturan_", menuName = "Pilah Sampah/Aturan Wilayah")]
public class WasteRuleSet : ScriptableObject
{
    public string regionName = "Taiwan";

    [Tooltip("Aturan pemilahan sampah di wilayah ini.")]
    public List<WasteCategoryData> categories = new List<WasteCategoryData>();
    public List<WasteRule> rules = new List<WasteRule>();

    private Dictionary<WasteItemData, WasteRule> map;
    private void OnEnable() => map = null;

    private void Build()
    {
        map = new Dictionary<WasteItemData, WasteRule>();
        foreach (WasteRule r in rules)
        {
            if (r == null || r.wasteItem == null)
            {
                continue;
            }
            if (!map.ContainsKey(r.wasteItem))
            {
                Debug.LogWarning($"{name}: ada dua aturan untuk '{r.wasteItem.displayName}'. Yang pertama dipakai.");
                continue;
            }
            map[r.wasteItem] = r;
        }
    }

    private WasteRule Find(WasteItemData item)
    {
        if (item == null) return null;
        if (map == null) Build();
        return map.TryGetValue(item, out WasteRule r) ? r : null;
    }

}
