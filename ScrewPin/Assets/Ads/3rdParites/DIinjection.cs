using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DIinjection : MonoBehaviour
{
    private Player player;
    // Start is called before the first frame update
    void Start()
    {
        player = new Player();
        // IWeapon weapon = new LaserGun();
        player.SetWeapon(new Gun());
        player.Attack();

    }

    // Update is called once per frame
    void Update()
    {

    }
}

public interface IWeapon
{
    void Shot();

}
public class Gun : IWeapon
{
    public void Shot()
    {
        Debug.Log("Duzz Duzz");

    }
}


public class LaserGun : IWeapon
{
    public void Shot()
    {
        Debug.Log("Zuuu Zuuu");
    }

}

public class Player : MonoBehaviour
{
    private IWeapon weapon;

    public void SetWeapon(IWeapon weapon)
    {
        this.weapon = weapon;
    }


    public void Attack()
    {
        weapon.Shot();
    }
}
