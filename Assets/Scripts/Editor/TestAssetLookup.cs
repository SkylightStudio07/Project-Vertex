using UnityEditor;

// 테스트용 에셋 찾기. 카드 파일 이름 뒤에 카드 이름이 붙어도(CH_02_C02 → CH_02_C02_전술 개량형 투척 창술)
// 번호 접두어로 찾아서, 표시용 이름을 바꿔도 테스트가 깨지지 않게 한다.
public static class TestAssetLookup
{
    // folder 안에서 파일 이름이 prefix 이거나 "prefix_..."인 카드. 없으면 null
    public static CardData CardByPrefix(string folder, string prefix)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:CardData", new[] { folder.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string file = System.IO.Path.GetFileNameWithoutExtension(path);
            if (file == prefix || file.StartsWith(prefix + "_")) return AssetDatabase.LoadAssetAtPath<CardData>(path);
        }
        return null;
    }
}
