using UnityEngine;
using UnityEditor;

public class SetSpriteRectTool
{
    // [사용법]
    // 1. Project 창에서 텍스처(이미지)들을 선택합니다.
    // 2. 상단 메뉴 Tools > 'Set Sprite Rect to 0,0,128,128' 클릭
    // 3. 끝.

    [MenuItem("Tools/Set Sprite Rect to 0,0,128,128")]
    static void SetRect128()
    {
        // 원하는 Rect 값 (x, y, width, height)
        Rect targetRect = new Rect(0, 0, 128, 128);

        foreach (Object obj in Selection.objects)
        {
            // 텍스처인지 확인
            if (obj is Texture2D)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    // 1. Sprite Mode가 'Single'이면 'Multiple'로 변경해야 Rect 수정이 자유롭습니다.
                    // (Single 모드는 이미지 전체 크기를 따라가므로 강제로 자르려면 Multiple이 낫습니다.)
                    if (importer.spriteImportMode == SpriteImportMode.Single)
                    {
                        importer.spriteImportMode = SpriteImportMode.Multiple;
                        // Single에서 넘어왔으니 데이터가 없을 수 있어 하나 만들어줍니다.
                        SpriteMetaData[] newSheet = new SpriteMetaData[1];
                        newSheet[0] = new SpriteMetaData
                        {
                            name = obj.name,
                            rect = targetRect,
                            alignment = (int)SpriteAlignment.Center, // 기본 피벗
                            pivot = new Vector2(0.5f, 0.5f)
                        };
                        importer.spritesheet = newSheet;
                    }
                    else if (importer.spriteImportMode == SpriteImportMode.Multiple)
                    {
                        // 2. 이미 Multiple 모드라면 기존 스프라이트들의 Rect만 수정
                        SpriteMetaData[] sheet = importer.spritesheet;
                        
                        // 데이터가 없으면 하나 생성
                        if (sheet == null || sheet.Length == 0)
                        {
                             sheet = new SpriteMetaData[1];
                             sheet[0] = new SpriteMetaData { name = obj.name };
                        }

                        for (int i = 0; i < sheet.Length; i++)
                        {
                            sheet[i].rect = targetRect; // 여기서 값 적용!
                        }
                        importer.spritesheet = sheet;
                    }

                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    Debug.Log($"[완료] {obj.name}의 Position(Rect)을 {targetRect}로 변경했습니다.");
                }
            }
        }
    }
}