using System.Collections;
using UnityEngine;

public class HoleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HoleLogic[] holes;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private PlayerBall playerBall;

    [Header("State Change Settings")]
    [SerializeField] private float minChangeTime = 3f;
    [SerializeField] private float maxChangeTime = 7f;
    [SerializeField] private float warningFlashTime = 1f;

    private void Start()
    {
        MakeSureAtLeastOneGoodHole();
        StartCoroutine(RandomHoleChangeRoutine());
    }

    private IEnumerator RandomHoleChangeRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minChangeTime, maxChangeTime);
            yield return new WaitForSeconds(waitTime);

            while (playerBall != null && playerBall.IsAiming)
                yield return null;

            if (holes == null || holes.Length == 0)
                continue;

            HoleLogic randomHole = GetRandomHole();

            if (randomHole == null)
                continue;

            yield return randomHole.FlashWarning(warningFlashTime);

            while (playerBall != null && playerBall.IsAiming)
                yield return null;

            ChangeOneHoleType(randomHole);
        }
    }

    private HoleLogic GetRandomHole()
    {
        if (holes == null || holes.Length == 0)
            return null;

        return holes[Random.Range(0, holes.Length)];
    }

    private void ChangeOneHoleType(HoleLogic hole)
    {
        if (hole == null)
            return;

        int goodHoleCount = CountGoodHoles();

        bool newTypeShouldBeGood = Random.value > 0.5f;

        // If this is the only blue hole, it cannot become red.
        if (hole.HoleType == HoleType.Good && goodHoleCount <= 1)
            newTypeShouldBeGood = true;

        // If there are already 2 blue holes, no more holes can become blue.
        if (hole.HoleType == HoleType.Bad && goodHoleCount >= 2)
            newTypeShouldBeGood = false;

        hole.SetHoleType(newTypeShouldBeGood ? HoleType.Good : HoleType.Bad);

        MakeSureAtLeastOneGoodHole();
    }

    private int CountGoodHoles()
    {
        int count = 0;

        if (holes == null)
            return count;

        foreach (HoleLogic hole in holes)
        {
            if (hole != null && hole.HoleType == HoleType.Good)
                count++;
        }

        return count;
    }

    private void MakeSureAtLeastOneGoodHole()
    {
        if (holes == null || holes.Length == 0)
            return;

        if (CountGoodHoles() > 0)
            return;

        HoleLogic randomHole = GetRandomHole();

        if (randomHole != null)
            randomHole.SetHoleType(HoleType.Good);
    }

    public void MoveHolesAfterShot()
    {
        if (playerBall != null && playerBall.IsAiming)
            return;

        if (holes == null || holes.Length == 0)
            return;

        if (spawnPoints == null || spawnPoints.Length < holes.Length)
            return;

        Transform[] shuffledPoints = new Transform[spawnPoints.Length];
        spawnPoints.CopyTo(shuffledPoints, 0);

        for (int i = 0; i < shuffledPoints.Length; i++)
        {
            int randomIndex = Random.Range(i, shuffledPoints.Length);

            Transform temp = shuffledPoints[i];
            shuffledPoints[i] = shuffledPoints[randomIndex];
            shuffledPoints[randomIndex] = temp;
        }

        for (int i = 0; i < holes.Length; i++)
        {
            if (holes[i] != null)
                holes[i].transform.position = shuffledPoints[i].position;
        }
    }
}