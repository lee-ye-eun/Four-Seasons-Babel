using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Babel/Tutorial/Create Tutorial Sequence 메뉴로 10단계 SO를 자동 생성한다.
/// requiredBuilding(WaysideAltar, IceCave)은 Inspector에서 직접 연결해야 한다.
/// </summary>
public static class TutorialSequenceFactory
{
    [MenuItem("Babel/Tutorial/Create Tutorial Sequence")]
    public static void Create()
    {
        var so = ScriptableObject.CreateInstance<TutorialSequenceSO>();
        so.steps = BuildSteps();

        const string path = "Assets/Data/Tutorial/TutorialSequence.asset";
        EnsureFolder("Assets/Data/Tutorial");
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = so;
        Debug.Log($"[TutorialFactory] TutorialSequence 생성 완료: {path}\n" +
                  "Step 4의 requiredBuilding(WaysideAltar SO)과 Step 6·9의 requiredBuilding(IceCave SO)을 Inspector에서 연결하세요.");
    }

    private static List<TutorialStepData> BuildSteps() => new()
    {
        // ── 1 ─────────────────────────────────────────────────────────────
        new TutorialStepData
        {
            description =
                "화면 왼쪽 위에는 '믿음(Faith)' 자원과 바벨탑의 현재 체력이 표시됩니다.\n" +
                "믿음은 건물을 건설하는 데 사용되며, 건물을 통해 생산됩니다.\n\n" +
                "화면을 클릭해 계속하세요.",
            highlightTarget = TutorialTargetKey.TopLeftUI,
            advanceType     = TutorialAdvanceType.ClickAnywhere,
        },

        // ── 2 ─────────────────────────────────────────────────────────────
        new TutorialStepData
        {
            description =
                "화면 오른쪽 위에는 현재 웨이브 정보가 표시됩니다.\n" +
                "웨이브가 진행되면 인간들이 바벨탑을 향해 몰려옵니다.\n\n" +
                "화면을 클릭해 계속하세요.",
            highlightTarget = TutorialTargetKey.TopRightUI,
            advanceType     = TutorialAdvanceType.ClickAnywhere,
        },

        // ── 3 ─────────────────────────────────────────────────────────────
        new TutorialStepData
        {
            description =
                "오른쪽 패널에 건설·파괴·보관 버튼이 있습니다.\n" +
                "건설 버튼을 눌러 건물 목록을 열어보세요!",
            highlightTarget = TutorialTargetKey.RightUI,
            advanceType     = TutorialAdvanceType.WaitForSignal,
            requiredSignal  = TutorialSignal.ConstructionButtonClicked,
        },

        // ── 4 ─────────────────────────────────────────────────────────────
        new TutorialStepData
        {
            description =
                "건물 카드를 길게 누른 채 드래그하여 건설 스폿에 놓으면 설치됩니다.\n" +
                "'노변 제단'을 드래그해 스폿에 설치해보세요!\n\n" +
                "※ requiredBuilding: WaysideAltar SO 연결 필요",
            highlightTarget = TutorialTargetKey.BuildingListPanel,
            advanceType     = TutorialAdvanceType.WaitForSignal,
            requiredSignal  = TutorialSignal.BuildingPlacedByDrag,
            // requiredBuilding = WaysideAltar SO ← Inspector에서 연결
        },

        // ── 5 ─────────────────────────────────────────────────────────────
        new TutorialStepData
        {
            description =
                "건물을 배치했다면 '실행' 버튼을 눌러 첫 번째 웨이브를 시작하세요!",
            highlightTarget = TutorialTargetKey.RunButton,
            advanceType     = TutorialAdvanceType.WaitForSignal,
            requiredSignal  = TutorialSignal.RunButtonClicked,
        },

        // ── 6 ─────────────────────────────────────────────────────────────
        // hideDescription = true → 화면에 아무것도 표시 안 하고 조용히 대기
        // StageEnd 패널이 뜨는 순간 신호를 받아 다음 스텝(설명 표시)으로 자동 진행
        new TutorialStepData
        {
            hideDescription = true,
            highlightTarget = TutorialTargetKey.None,
            advanceType     = TutorialAdvanceType.WaitForSignal,
            requiredSignal  = TutorialSignal.StageEndShown,
        },

        // ── 7 ─────────────────────────────────────────────────────────────
        new TutorialStepData
        {
            description =
                "정비 버튼을 눌러 다음 시간으로 향하세요.",
            highlightTarget = TutorialTargetKey.NextStageButton,
            advanceType     = TutorialAdvanceType.WaitForSignal,
            requiredSignal  = TutorialSignal.NextStageButtonClicked,
        },

        // ── 8 ─────────────────────────────────────────────────────────────
        new TutorialStepData
        {
            description =
                "웨이브가 끝났습니다!\n" +
                "이번엔 '얼음 동굴'을 스폿에 설치해보세요.\n\n" +
                "※ requiredBuilding: IceCave SO 연결 필요",
            highlightTarget      = TutorialTargetKey.BuildingListPanel,
            advanceType          = TutorialAdvanceType.WaitForSignal,
            requiredSignal       = TutorialSignal.BuildingPlacedByDrag,
            hasAlternateSignal   = true,
            alternateRequiredSignal = TutorialSignal.BuildingPlacedByClick,
            // requiredBuilding = IceCave SO ← Inspector에서 연결
        },

        // ── 9 ─────────────────────────────────────────────────────────────
        new TutorialStepData
        {
            description =
                "건물을 배치했다면 다시 웨이브를 시작해보세요!",
            highlightTarget = TutorialTargetKey.RunButton,
            advanceType     = TutorialAdvanceType.WaitForSignal,
            requiredSignal  = TutorialSignal.RunButtonClicked,
        },

        // ── 10 ────────────────────────────────────────────────────────────
        new TutorialStepData
        {
            description =
                "마음에 들지 않는 건물은 파괴 버튼으로 철거할 수 있습니다.\n" +
                "파괴 버튼을 눌러보세요!",
            highlightTarget = TutorialTargetKey.DestroyButton,
            advanceType     = TutorialAdvanceType.WaitForSignal,
            requiredSignal  = TutorialSignal.DestroyButtonClicked,
        },

        // ── 12 ────────────────────────────────────────────────────────────
        // TODO: 씬에 IceCave가 없으면 이 스텝이 영원히 진행되지 않습니다.
        //       튜토리얼 시작 전 IceCave가 배치되어 있어야 하거나,
        //       파괴할 대상이 없는 경우를 위한 예외 처리 로직이 필요합니다.
        new TutorialStepData
        {
            description =
                "파괴 모드에서 건물을 클릭하면 철거됩니다.\n" +
                "'얼음 동굴'을 클릭해 철거해보세요!\n\n" +
                "※ requiredBuilding: IceCave SO 연결 필요",
            highlightTarget = TutorialTargetKey.None,
            advanceType     = TutorialAdvanceType.WaitForSignal,
            requiredSignal  = TutorialSignal.BuildingDestroyed,
            // requiredBuilding = IceCave SO ← Inspector에서 연결
        },

        // ── 13 ────────────────────────────────────────────────────────────
        new TutorialStepData
        {
            description =
                "파괴 모드를 종료하려면 파괴 버튼을 다시 누르세요.",
            highlightTarget = TutorialTargetKey.DestroyButton,
            advanceType     = TutorialAdvanceType.WaitForSignal,
            requiredSignal  = TutorialSignal.DestroyModeExited,
        },
    };

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        var parts = folder.Split('/');
        var current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            var next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
