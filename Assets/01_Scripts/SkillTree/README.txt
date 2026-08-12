스킬트리 사용법
-SkillEffectContext에 값을 조정할 클래스 넣기
-SkillEffectSO를 부모로 스크립터블 오브젝트 스크립트 만들고, 스크립터블 오브젝트까지 만들기(AttackDamageEffectSO 참고)
-SkillDataSO로 스크립터블 오브젝트 만들어서, Effect에 바로 위에서 만든 오브젝트 넣고, ID(ID 겹치지 않게), Value에 각 레벨별 수치 넣고, 
최대 레벨(Value보다 높으면 안됨)등 수치 작성하기
-SkillTreeManager의 Skill List에 바로 위에서 작성한 SO 넣기

-버튼에 SkillTreeButton을 넣고, Skill Data에 작성한 SO 넣고, SkillTreeManager연결하기
-SkillTreeManager에 Player 넣기
