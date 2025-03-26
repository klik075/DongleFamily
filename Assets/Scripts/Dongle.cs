using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Dongle : MonoBehaviour
{
    Coroutine coHide;
    public GameManager manager;
    public ParticleSystem effect;
    public int level;
    public bool isDrag;
    public bool isMerge;
    public bool isAttach;

    public Rigidbody2D rigid;
    Animator animator;
    CircleCollider2D circleCollider;
    SpriteRenderer spriteRenderer;

    float deadTime;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        circleCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        animator.SetInteger("Level", level);
    }
    private void OnDisable()
    {
        if(coHide != null)
            StopCoroutine(coHide);

        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circleCollider.enabled = true;
        spriteRenderer.sortingOrder = 1;
        animator.enabled = true;
        coHide = null;

    }
    void Update()
    {
        if (isDrag)
        {
            Vector3 mousPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float leftBorder = -4.2f + transform.localScale.x / 2f;
            float rightBorder = 4.2f - transform.localScale.x / 2f;

            if (mousPos.x < leftBorder)
            {
                mousPos.x = leftBorder;
            }
            else if (mousPos.x > rightBorder)
            {
                mousPos.x = rightBorder;
            }
            mousPos.y = 8;
            mousPos.z = 0;
            transform.position = Vector3.Lerp(transform.position, mousPos, 0.2f);
        }
    }
    public void Drag()
    {
        isDrag = true;
    }
    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine("AttackRoutine");
    }
    IEnumerator AttackRoutine()
    {
        if (isAttach)
            yield break;

        isAttach = true;

        manager.SFXPlay(SFX.Attach);

        yield return new WaitForSeconds(0.2f);

        isAttach = false;

    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();
            
            if(level == other.level && !isMerge && !other.isMerge && level <= 7)
            {
                float myX = transform.position.x;
                float myY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                if (myY < otherY || (myY == otherY && myX > otherX))
                {
                    other.Hide(transform.position);
                    LevelUp();
                }
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;
            if(deadTime > 2)
            {
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }
            if (deadTime > 3)
            {
                manager.GameOver();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Finish")
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }

    private void LevelUp()
    {
        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        if (level >= 7)
        {
            Hide(Vector3.up * 100);
            manager.SFXPlay(SFX.LevelUp);
            return;
        }

        StartCoroutine("LevelUpRoutine");
    }
    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        animator.SetInteger("Level", level + 1);
        EffectPlay();
        manager.SFXPlay(SFX.LevelUp);

        yield return new WaitForSeconds(0.3f);
        level++;

        manager.maxLevel = Mathf.Max(manager.maxLevel, level);

        isMerge = false;
    }
    private void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
    public void Hide(Vector3 targetPos)
    {
        isMerge = true;

        rigid.simulated = false;
        circleCollider.enabled = false;
        animator.enabled = false;
        spriteRenderer.sortingOrder = -1;

        if (targetPos == Vector3.up * 100)
            EffectPlay();

        coHide = StartCoroutine(HideRoutine(targetPos));
    }
    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;
        int scale = 1;
        Vector3 scaleVector = new Vector3(1, 1, 1);
        while(frameCount < 20)
        {
            frameCount++;
            if (targetPos != Vector3.up * 100)
            {
                scale = scale * (1 - (frameCount / 20));
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
                transform.localScale = Vector3.Lerp(transform.localScale, scale * scaleVector, 0.5f);
            }
            else if (targetPos == Vector3.up * 100)
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            
            yield return null;
        }

        manager.score += (int)Mathf.Pow(2, level);

        isMerge = false;
        gameObject.SetActive(false);
    }
}
