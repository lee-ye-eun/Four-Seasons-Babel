// 이 파일은 반드시 Assets 하위의 "Editor" 폴더 안에 두어야 합니다.
// 예: Assets/Editor/BuildingCsvUtility.cs
// (UnityEditor API를 사용하므로 Editor 폴더 밖에 두면 빌드 에러가 납니다.)

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class BuildingCsvUtility
{
    // 새로 만드는 BuildingSO 애셋이 저장될 기본 폴더.
    // 이미 다른 폴더에 BuildingSO들이 있어도 Export/Import 자체는 상관없이 동작합니다.
    private const string DefaultFolder = "Assets/Data/Buildings";

    // CSV 컬럼 순서. 헤더 이름을 기준으로 읽으므로 시트에서 컬럼 순서를 바꿔도 안전합니다.
    private static readonly string[] Columns =
    {
        "assetName", "buildingName", "description",
        "iconPath", "cost", "useTime", "faithProduction",
        "capacitySpring", "capacitySummer", "capacityAutumn", "capacityWinter",
        "springVisualSpritePath", "summerVisualSpritePath", "autumnVisualSpritePath", "winterVisualSpritePath"
    };

    [MenuItem("Babel/Building/Export to CSV")]
    public static void Export()
    {
        var path = EditorUtility.SaveFilePanel("Export Buildings to CSV", "Assets", "Buildings", "csv");
        if (string.IsNullOrEmpty(path)) return;

        var guids = AssetDatabase.FindAssets("t:BuildingSO");
        var buildings = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<BuildingSO>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(b => b != null)
            .OrderBy(b => b.name)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", Columns));

        foreach (var b in buildings)
        {
            var row = new[]
            {
                Csv(b.name),
                Csv(b.buildingName),
                Csv(b.description),
                Csv(SpriteAssetPath(b.icon)),
                b.cost.ToString(),
                b.useTime.ToString("F2", CultureInfo.InvariantCulture),
                b.faithProduction.ToString(),
                b.capacitySpring.ToString(),
                b.capacitySummer.ToString(),
                b.capacityAutumn.ToString(),
                b.capacityWinter.ToString(),
                Csv(SpriteAssetPath(b.springVisualSprite)),
                Csv(SpriteAssetPath(b.summerVisualSprite)),
                Csv(SpriteAssetPath(b.autumnVisualSprite)),
                Csv(SpriteAssetPath(b.winterVisualSprite)),
            };
            sb.AppendLine(string.Join(",", row));
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log($"[BuildingCsvUtility] {buildings.Count}개 건물을 CSV로 내보냈습니다: {path}");
    }

    [MenuItem("Babel/Building/Import from CSV")]
    public static void Import()
    {
        var path = EditorUtility.OpenFilePanel("Import Buildings from CSV", "Assets", "csv");
        if (string.IsNullOrEmpty(path)) return;

        var rows = ParseCsv(File.ReadAllText(path));
        if (rows.Count == 0)
        {
            Debug.LogWarning("[BuildingCsvUtility] CSV가 비어 있습니다.");
            return;
        }

        var header = rows[0];
        var colIndex = new Dictionary<string, int>();
        for (var i = 0; i < header.Count; i++) colIndex[header[i]] = i;

        var created = 0;
        var updated = 0;

        for (var r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            var assetName = Get(row, colIndex, "assetName");
            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogWarning($"[BuildingCsvUtility] {r + 1}번째 행: assetName이 비어 있어 건너뜁니다.");
                continue;
            }

            var assetPath = $"{DefaultFolder}/{Sanitize(assetName)}.asset";
            var so = AssetDatabase.LoadAssetAtPath<BuildingSO>(assetPath);

            if (so == null)
            {
                // 기본 폴더에 없으면 프로젝트 전체에서 같은 이름의 BuildingSO를 한 번 더 찾아본다.
                so = AssetDatabase.FindAssets("t:BuildingSO")
                    .Select(g => AssetDatabase.LoadAssetAtPath<BuildingSO>(AssetDatabase.GUIDToAssetPath(g)))
                    .FirstOrDefault(b => b != null && b.name == assetName);
            }

            var isNew = so == null;
            if (isNew)
            {
                so = ScriptableObject.CreateInstance<BuildingSO>();
                EnsureFolder(DefaultFolder);
                AssetDatabase.CreateAsset(so, assetPath);
                created++;
            }
            else
            {
                updated++;
            }

            so.buildingName = Get(row, colIndex, "buildingName");
            so.description = Get(row, colIndex, "description");
            so.icon = LoadSprite(Get(row, colIndex, "iconPath"));
            so.cost = ParseInt(Get(row, colIndex, "cost"));
            so.useTime = ParseFloat(Get(row, colIndex, "useTime"));
            so.faithProduction = ParseInt(Get(row, colIndex, "faithProduction"));
            so.capacitySpring = ParseInt(Get(row, colIndex, "capacitySpring"));
            so.capacitySummer = ParseInt(Get(row, colIndex, "capacitySummer"));
            so.capacityAutumn = ParseInt(Get(row, colIndex, "capacityAutumn"));
            so.capacityWinter = ParseInt(Get(row, colIndex, "capacityWinter"));
            so.springVisualSprite = LoadSprite(Get(row, colIndex, "springVisualSpritePath"));
            so.summerVisualSprite = LoadSprite(Get(row, colIndex, "summerVisualSpritePath"));
            so.autumnVisualSprite = LoadSprite(Get(row, colIndex, "autumnVisualSpritePath"));
            so.winterVisualSprite = LoadSprite(Get(row, colIndex, "winterVisualSpritePath"));

            EditorUtility.SetDirty(so);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[BuildingCsvUtility] 가져오기 완료. 생성 {created}개, 갱신 {updated}개.");
    }

    // ---------- helpers ----------

    // 스프라이트 경로는 아틀라스 대응을 위해 "texturePath[spriteName]" 형식으로 저장
    private static string SpriteAssetPath(Sprite sprite)
    {
        if (sprite == null) return "";
        var path = AssetDatabase.GetAssetPath(sprite);
        return $"{path}[{sprite.name}]";
    }

    // "[spriteName]" 접미사가 있으면 LoadAllAssetsAtPath로 서브-스프라이트 검색
    private static Sprite LoadSprite(string pathWithName)
    {
        if (string.IsNullOrEmpty(pathWithName)) return null;

        var bracketIdx = pathWithName.IndexOf('[');
        if (bracketIdx >= 0)
        {
            var texPath    = pathWithName.Substring(0, bracketIdx);
            var spriteName = pathWithName.Substring(bracketIdx + 1).TrimEnd(']');
            return AssetDatabase.LoadAllAssetsAtPath(texPath)
                .OfType<Sprite>()
                .FirstOrDefault(s => s.name == spriteName);
        }

        // 이전 포맷(이름 없는 경로) 하위 호환
        return AssetDatabase.LoadAssetAtPath<Sprite>(pathWithName);
    }

    private static int ParseInt(string s) => int.TryParse(s, out var v) ? v : 0;
    private static float ParseFloat(string s) =>
        float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0f;

    private static string Get(List<string> row, Dictionary<string, int> colIndex, string col) =>
        colIndex.TryGetValue(col, out var i) && i < row.Count ? row[i] : "";

    private static string Sanitize(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        var parts = folder.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    // RFC4180 방식 CSV: 콤마/줄바꿈/따옴표가 포함된 필드를 따옴표로 감싸고 이스케이프 처리.
    private static string Csv(string field)
    {
        if (field == null) return "";
        var mustQuote = field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r");
        if (!mustQuote) return field;
        return "\"" + field.Replace("\"", "\"\"") + "\"";
    }

    private static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var i = 0;
        var len = text.Length;

        while (i < len)
        {
            var c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < len && text[i + 1] == '"') { field.Append('"'); i += 2; continue; }
                    inQuotes = false; i++; continue;
                }
                field.Append(c); i++; continue;
            }

            switch (c)
            {
                case '"':
                    inQuotes = true; i++; break;
                case ',':
                    row.Add(field.ToString()); field.Clear(); i++; break;
                case '\r':
                    i++; break;
                case '\n':
                    row.Add(field.ToString()); field.Clear();
                    rows.Add(row); row = new List<string>();
                    i++; break;
                default:
                    field.Append(c); i++; break;
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        return rows.Where(r => r.Count > 1 || !string.IsNullOrEmpty(r[0])).ToList();
    }
}