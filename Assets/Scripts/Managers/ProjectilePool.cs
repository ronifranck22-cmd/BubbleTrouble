using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    public GameObject projectilePrefab;
    public int initialPoolSize = 5;

    private readonly List<GameObject> pool = new List<GameObject>();

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform);
            projectile.SetActive(false);
            pool.Add(projectile);
        }
    }

    public GameObject GetProjectile()
    {
        foreach (GameObject projectile in pool)
        {
            if (!projectile.activeInHierarchy)
                return projectile;
        }

        GameObject newProjectile = Instantiate(projectilePrefab, transform);
        newProjectile.SetActive(false);
        pool.Add(newProjectile);
        return newProjectile;
    }

    public void ReturnProjectile(GameObject projectile)
    {
        projectile.SetActive(false);
    }
}
