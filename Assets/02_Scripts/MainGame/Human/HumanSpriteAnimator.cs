using System;
using UnityEngine;

[RequireComponent(typeof(HumanAgent))]
[RequireComponent(typeof(SpriteRenderer))]
public class HumanSpriteAnimator : MonoBehaviour
{
    [Serializable]
    public class SpriteSet
    {
        public Sprite[] rightSprites;
        public Sprite[] backSprites;
    }

    [SerializeField] private SpriteSet[] spriteSets;
    [SerializeField] private float frameRate = 6f;

    private HumanAgent agent;
    private SpriteRenderer sr;
    private SpriteSet activeSet;
    private float timer;
    private int frameIndex;

    private void Awake()
    {
        agent = GetComponent<HumanAgent>();
        sr    = GetComponent<SpriteRenderer>();

        if (spriteSets != null && spriteSets.Length > 0)
            activeSet = spriteSets[UnityEngine.Random.Range(0, spriteSets.Length)];
    }

    private void Update()
    {
        if (activeSet == null) return;
        if (agent.CurrentState == AgentState.Despawned) return;
        if (!sr.enabled) return;

        Vector2 dir = agent.MoveDirection;
        if (dir == Vector2.zero) return;

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer = 0f;
            frameIndex = (frameIndex + 1) % 2;
        }

        bool horizontal = Mathf.Abs(dir.x) >= Mathf.Abs(dir.y);
        Sprite[] sprites = horizontal ? activeSet.rightSprites : activeSet.backSprites;
        if (sprites == null || sprites.Length < 2) return;

        sr.sprite = sprites[frameIndex];
        sr.flipX  = dir.x < 0f;
    }
}
