using System.Collections;
using UnityEngine;

public class HoleManager : MonoBehaviour
{
    //********** VARIABLES **********

    //Ref
    [Header("References")]
    [SerializeField] private Hole[] holes;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private PlayerBall playerBall;

    //Change holes Color Parameters
    [Header("State Change Settings")]
    [SerializeField] private float minChangeTime = 2f;
    [SerializeField] private float maxChangeTime = 5f;
    [SerializeField] private float warningFlashTime = 1f;


    //********** START & change color loop ( Coroutine ) **********
    private void Start()
    {
        StartCoroutine(RandomHoleChangeRoutine());
    }

    private IEnumerator RandomHoleChangeRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minChangeTime, maxChangeTime);// random between 3 and 7 sec
            yield return new WaitForSeconds(waitTime);

            while (playerBall != null && playerBall.IsAiming)
                yield return null;

            if (holes == null || holes.Length == 0)
                continue;

            Hole randomHole = GetRandomHole();

            if (randomHole == null)
                continue;

            yield return randomHole.FlashWarning(warningFlashTime); // call FlashWarning from holeLogic.cs

            while (playerBall != null && playerBall.IsAiming)
                yield return null;

            ChangeOneHoleType(randomHole);
        }
    }


    //********** Return Random Hole **********
    private Hole GetRandomHole()
    {
        if (holes == null || holes.Length == 0)
            return null;

        return holes[Random.Range(0, holes.Length)];
    }


    //********** After Flashing -> Change Hole color, if there is only 1 blue or more then 3 => new type red! **********
    private void ChangeOneHoleType(Hole hole)
    {
        if (hole == null)
            return;

        int goodHoleCount = CountGoodHoles();

        bool newTypeShouldBeGood = Random.value > 0.5f;

        // If this is the only blue hole it cannot become red.
        if (hole.HoleType == HoleType.Good && goodHoleCount <= 1)
            newTypeShouldBeGood = true;

        // If there are already 3 blue holes no more holes can become blue.
        if (hole.HoleType == HoleType.Bad && goodHoleCount >= 3)
            newTypeShouldBeGood = false;

        hole.SetHoleType(newTypeShouldBeGood ? HoleType.Good : HoleType.Bad);

        MakeSureAtLeastOneGoodHole();
    }


    //********** Helper How Many Blue Holes Game Have **********
    private int CountGoodHoles()
    {
        int count = 0;

        foreach (Hole hole in holes)
            if (hole != null && hole.HoleType == HoleType.Good)
                count++;

        return count;
    }


    //********** At Least One Hole always Shoudl Be Blue **********
    private void MakeSureAtLeastOneGoodHole()
    {
        if (holes == null || holes.Length == 0)
            return;

        if (CountGoodHoles() > 0)
            return;

        Hole randomHole = GetRandomHole();

        if (randomHole != null)
            randomHole.SetHoleType(HoleType.Good);
    }


    //********** If Player Hits blue/red Holes OR Water, Move Holes to new locations base on randomized index **********
    public void MoveHolesAfterShot(Hole hitHole)
    {
        if (playerBall != null && playerBall.IsAiming)
            return;

        if (holes == null || holes.Length == 0)
            return;

        if (spawnPoints == null || spawnPoints.Length <= holes.Length)
            return;

        //is there a spawn index which is blocked ( no spawn at this place) after hit
        int blockedIndex = hitHole != null ? hitHole.CurrentSpawnIndex : -1;

        //get randomly sorted spawn indexes
        int[] spawnIndexes = GetRandomSpawnIndexes();

        int spawnPointer = 0;

        //Set Holes positions based on spawn index avoids blocked one!
        for (int i = 0; i < holes.Length; i++)
        {
            if (holes[i] == null)
                continue;

            // if spawnIndexes[spawnPointer] is blocked => increase spawnPointer and try next one, to avoid spawn in same place
            while (spawnPointer < spawnIndexes.Length && spawnIndexes[spawnPointer] == blockedIndex) 
                spawnPointer++;

            int selectedIndex = spawnIndexes[spawnPointer]; // not blocked index

            holes[i].transform.position = spawnPoints[selectedIndex].position; // sets hole position
            holes[i].SetCurrentSpawnIndex(selectedIndex); // save new index ( for future block if player hits this hole)

            spawnPointer++;
        }
    }

    //********** Returns Randomised index list (mixing) **********
    private int[] GetRandomSpawnIndexes()
    {
        int[] indexes = new int[spawnPoints.Length]; // int[] size of random locations

        for (int i = 0; i < indexes.Length; i++) // assign sorted  index 123456....
            indexes[i] = i;

        //sorting randomly and swaping places
        for (int i = 0; i < indexes.Length; i++)
        {
            int randomIndex = Random.Range(i, indexes.Length); 

            int temp = indexes[i];
            indexes[i] = indexes[randomIndex];
            indexes[randomIndex] = temp;
        }

        return indexes;
    }
}